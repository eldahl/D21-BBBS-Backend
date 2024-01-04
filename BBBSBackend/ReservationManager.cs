using BBBSBackend.DBModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BBBSBackend
{
    public class ReservationManager
    {
        private Dictionary<Guid, DateTime> _timers = new Dictionary<Guid, DateTime>();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationManager> _logger;

        public ReservationManager(IServiceScopeFactory scopeFactory, ILogger<ReservationManager> logger)
        {
            _scopeFactory = scopeFactory;

            // Load all bookings with reservationTimeOut = false, and paid = false into the timers list.
            StartupLoadReservationsFromDB();

            Timer periodicCaller = new Timer(new TimerCallback(CheckReservationTimes), null, 0, 60000);
            _logger = logger;
        }

        public async void StartupLoadReservationsFromDB() {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BbbsContext>();

                // Fetch the bookings to reserve
                var unpaidAndReservedBookings = await (from b in db.Bookings
                                where b.Paid == false && b.ReservationTimeOut == false
                                select b).ToListAsync();

                // Add to reservation timers
                foreach (var booking in unpaidAndReservedBookings) {
                    _timers.Add(booking.Id, booking.CreatedAt);
                }

                _logger.LogInformation("[ReservationManager]: Loaded reservations from DB.");
            }
        }

        public async void CheckReservationTimes(object? target) {
            using (var scope = _scopeFactory.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<BbbsContext>();

                foreach (var timer in _timers)
                {
                    TimeSpan elapsedTime = DateTime.UtcNow - timer.Value;

                    // If more than 30 minutes ago, label as archived
                    if (elapsedTime > TimeSpan.FromMinutes(30))
                    {
                        // Set reservationTimeOut to true, so that the booking can be discarded from searches.
                        var bookingQuery = (from b in db.Bookings
                                       where b.Id == timer.Key && b.ReservationTimeOut == false
                                       select b);

                        // Continue on error for now
                        bool any = await bookingQuery.AnyAsync();
                        if (!any)
                            continue;

                        // Get the booking
                        var booking = await bookingQuery.FirstAsync();

                        // Set reservationTimeOut
                        booking.ReservationTimeOut = true;

                        // Save changes to DB.
                        db.Update(booking);
                        await db.SaveChangesAsync();

                        _logger.LogInformation("[ReservationManager]: Booking reservation timed out.: " + booking.Id);
                    }
                }
            }
        }

        public async void ReservationPaid(Guid guid) {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BbbsContext>();

                DateTime reservationTime;
                if (_timers.TryGetValue(guid, out reservationTime))
                {
                    // If time since reservation is not more than 30 minutes back.
                    if (!((DateTime.UtcNow - reservationTime) > TimeSpan.FromMinutes(30)))
                        return;

                    // Remove entry
                    _timers.Remove(guid);

                    // Set paid field on booking to true
                    var booking = (from b in db.Bookings
                                   where b.Id == guid && b.ReservationTimeOut == false && b.Paid == false
                                   select b).First();

                    // Continue on error for now
                    if (booking == null)
                        return;

                    booking.Paid = true;

                    // Save changes to DB.
                    db.Update(booking);
                    await db.SaveChangesAsync();

                    _logger.LogInformation("[ReservationManager]: Booking reservation paid.: " + booking.Id);
                }
            }
        }

        public void AddReservationTimer(Guid guid, DateTime reservationTime) {
            _timers.Add(guid, reservationTime);
            _logger.LogInformation("[ReservationManager]: Booking reservation added.: " + guid);
        }
    }
}
