using BBBSBackend.Controllers;
using BBBSBackend.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BBBSBackend.Controllers
{
    public class PlaceReservationParams
    {
        public string? FirstName {  get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Telephone { get; set; }
        public bool ReceiveNewsletter { get; set; }
        public BookingData? BookingData { get; set; }
    }

    public class BookingData { 
        public RoomBookingEntry[]? Rooms { get; set; }
        public DateTimeOffset ArrivalDate { get; set; }
        public DateTimeOffset DepartureDate { get; set; }
        public int NumberOfPeople { get; set; }
        public AdditionalServiceBookingEntry[]? AddServices { get; set; }
        public string? Comment { get; set; }
    }

    public class RoomBookingEntry { 
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class AdditionalServiceBookingEntry {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool[]? Days { get; set; }
    }

    public class GetScheduleBookingsForPeriodParams
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }

    public class GetBookingsForPeriodDTO
    {
        public Booking? Booking {get; set; }
        public RoomForBooking? RoomForBooking { get; set; }
        public Room? Room { get; set; }
        public AdditionalServiceForBooking? AdditionalServiceForBooking { get; set; }
        public AdditionalService? AdditionalService { get; set; }
    }

    public class SearchAvailableRoomsParameters
    {
        public DateTimeOffset ArrivalDate { get; set; }
        public DateTimeOffset DepartureDate { get; set; }
        public int AmountOfGuests { get; set; }
    }

    public class GetOrderSummaryParam
    {
        public Guid Order { get; set; }
    }

    public class GetOrderSummaryData {
        public Guid Id { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public int NumberOfPeople { get; set; }
        public bool Paid { get; set; }
        public string Comment { get; set; } = null!;

        public virtual Customer Customer { get; set; } = null!;
        public Room[]? Rooms { get; set; }
        public AdditionalService[]? AddServices { get; set; }
    }

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BbbsContext _dataContext;
        private ReservationManager _rm;
        private EmailDispatcher _emailDispatcher;
        private readonly ILogger<BookingController> _logger;

        public BookingController(BbbsContext dataContext, ReservationManager rm, EmailDispatcher emailDispatcher, ILogger<BookingController> logger)
        {
            _dataContext = dataContext;
            _rm = rm;
            _emailDispatcher = emailDispatcher;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("GetAvailableServices")]
        public async Task<ActionResult<List<AdditionalService>>> GetAvailableServices()
        {

            List<AdditionalService> services = await _dataContext.AdditionalServices.ToListAsync();
            return Ok(services);
        }

        [AllowAnonymous]
        [HttpPost("GetAvailableRoomsForDates")]
        public async Task<ActionResult<List<Room>>> GetAvailableRoomsForDates([FromBody] SearchAvailableRoomsParameters parameters)
        {

            _logger.LogInformation(parameters.ArrivalDate.ToString());
            _logger.LogInformation(parameters.DepartureDate.ToString());

            // Filter available rooms based on the arrival and departure date
            List<Room> availableRooms = await (
                from room in _dataContext.Rooms
                where !(from rfb in _dataContext.RoomForBookings
                        join b in _dataContext.Bookings on rfb.BookingId equals b.Id
                        where rfb.RoomId == room.Id &&
                                b.ArrivalDate <= parameters.DepartureDate &&
                                b.DepartureDate >= parameters.ArrivalDate &&
                                b.ReservationTimeOut == false && b.Canceled == false
                        select rfb).Any()
                select room
            ).ToListAsync();


            // If there are more guests than available beds, then give an error.
            int sumCapacityOfRooms = 0;
            availableRooms.ForEach((room) => sumCapacityOfRooms += room.Capacity);
            if (parameters.AmountOfGuests > sumCapacityOfRooms)
                return BadRequest(JsonSerializer.Serialize("Not enough room for all guests!"));

            return Ok(availableRooms);
        }

        [AllowAnonymous]
        [HttpPost("GetOrderSummary")]
        public async Task<ActionResult<Booking>> GetOrderSummary([FromBody] GetOrderSummaryParam param)
        {
            Regex reg = new Regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");
            if (!reg.IsMatch(param.Order.ToString())) {
                _logger.LogInformation("[GetOrderSummary]: Bad order specified.");
                return BadRequest("Bad order specified.");
            }

            var bookingQuery = _dataContext.Bookings
                .Where(b => param.Order == b.Id)
                .Include(b => b.Customer)
                .Include(b => b.AdditionalServiceForBookings)
                    .ThenInclude(asfb => asfb.AdditionalService)
                .Include(b => b.RoomForBookings)
                    .ThenInclude(rfb => rfb.Room);

            if (bookingQuery.Any() is false) {
                _logger.LogInformation("[GetOrderSummary]: Order does not exist.");
                return BadRequest("Order does not exist.");
            }

            var booking = await bookingQuery.FirstAsync();

            GetOrderSummaryData data = new GetOrderSummaryData() {
                Id = booking.Id,
                ArrivalDate = booking.ArrivalDate,
                DepartureDate = booking.DepartureDate,
                NumberOfPeople = booking.NumberOfPeople,
                Paid = booking.Paid,
                Comment = booking.Comment,
                Customer = booking.Customer,
                Rooms = booking.RoomForBookings.Select(rfb => rfb.Room).Distinct().ToArray(),
                AddServices = booking.AdditionalServiceForBookings.Select(asfb => asfb.AdditionalService).Distinct().ToArray()
            };

            _logger.LogInformation("[GetOrderSummary]: Serving booking for order ID.");
            return Ok(booking);
        }

        [AllowAnonymous]
        [HttpGet("CancelBooking")]
        // Cancelation token is base64 encoded ([booking id] + [customer email]) 
        public async Task<ActionResult<string>> CancelBooking(string cancelationToken)
        {
            // We need a cancelation token for this opeartion to be carried out
            if (cancelationToken is null)
                return BadRequest(JsonSerializer.Serialize("Missing cancelation token."));

            // Decode from base64
            byte[] bytes = Convert.FromBase64String(cancelationToken);
            string cancelationTokenString = Encoding.UTF8.GetString(bytes);

            // Split into the two components of the cancelation token
            string bookingIDString = cancelationTokenString.Split(" | ")[0];
            string customerEmailString = cancelationTokenString.Split(" | ")[1];

            // Check that the booking ID is a valid GUID
            Regex reg = new Regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");
            if (!reg.IsMatch(bookingIDString))
            {
                _logger.LogInformation("[CancelBooking]: Base64 encoded cancelation token does not have a valid GUID.");
                return BadRequest("No booking found.");
            }

            // Parse booking ID string into a GUID booking ID
            Guid bookingID = Guid.Parse(bookingIDString);

            var bookingQuery = _dataContext.Bookings
                .Where(b => bookingID == b.Id);

            if (bookingQuery.Any() is false)
            {
                _logger.LogInformation("[CancelBooking]: Could not find booking for booking ID supplied by cancelation token.");
                return BadRequest("No booking found.");
            }

            var booking = await bookingQuery.FirstAsync();

            booking.Canceled = true;

            _dataContext.Update(booking);
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation("[CancelBooking]: Canceled booking.");
            return Ok("Your booking has been canceled.");
        }

        [AllowAnonymous]
        [HttpPost("/Booking/PlaceReservation")]
        public async Task<ActionResult<string>> PlaceReservation([FromBody] PlaceReservationParams bookingParams)
        {
            bool doDebug = false;
            // If customer exists, we use the existing customer information.
            var customerForEmailQuery = from cust in _dataContext.Customers
                        where cust.Email == bookingParams.Email
                        select cust;

            bool customerEmailAlreadyExists = await customerForEmailQuery.AnyAsync();
            
            DateTime creationTimestamp = DateTime.UtcNow;

            Customer customer = new Customer();
            if (customerEmailAlreadyExists) {
                customer = await customerForEmailQuery.FirstAsync();
            } 
            else {
                customer.FirstName = bookingParams.FirstName!;
                customer.LastName = bookingParams.LastName!;
                customer.CreatedAt = creationTimestamp;
                customer.Email = bookingParams.Email!;
                customer.City = bookingParams.City!;
                customer.Address = bookingParams.Address!;
                customer.Phone = bookingParams.Telephone!;
                customer.PostalCode = bookingParams.PostalCode!;
                customer.ReceiveNewsletter = bookingParams.ReceiveNewsletter;
                
                // Add customer to datacontext
                customer = (await _dataContext.Customers.AddAsync(customer)).Entity;
                await _dataContext.SaveChangesAsync();
            }

            Booking booking = new Booking();
            booking.ArrivalDate = bookingParams.BookingData!.ArrivalDate.DateTime;
            booking.DepartureDate = bookingParams.BookingData.DepartureDate.DateTime;
            booking.CreatedAt = creationTimestamp;
            booking.Comment = bookingParams.BookingData.Comment!;
            booking.NumberOfPeople = bookingParams.BookingData.NumberOfPeople;
            booking.Paid = false;
            booking.Customer = customer;

            List<RoomForBooking> rfbs = new List<RoomForBooking>();
            List<AdditionalServiceForBooking> asfbs = new List<AdditionalServiceForBooking>();
            if (doDebug) {
                _logger.LogInformation("Booking ID: " + booking.Id + " | " + "Customer ID: " + booking.CustomerId);
                _logger.LogInformation(new DateTime(booking.ArrivalDate.Ticks).ToString());
                _logger.LogInformation(new DateTime(booking.DepartureDate.Ticks).ToString());
                _logger.LogInformation("---------------------------------------------------");
            }
            for (int i = 0; i < (booking.DepartureDate - booking.ArrivalDate).TotalDays; i++) {
                DateTime date = new DateTime(booking.ArrivalDate.Ticks).AddDays(i);
                foreach (RoomBookingEntry room in bookingParams.BookingData.Rooms!) {
                    RoomForBooking rfb = new RoomForBooking();
                    rfb.Date = date;
                    rfb.Booking = booking;
                    rfb.RoomId = room.Id;
                    rfbs.Add(rfb);
                    
                    if (doDebug)
                        _logger.LogInformation("Inserting RoomForBooking | Room ID:" + room.Id + " | " + date);
                }
                foreach (AdditionalServiceBookingEntry addService in bookingParams.BookingData.AddServices!) {
                    if (addService.Days![i]) {
                        AdditionalServiceForBooking asfb = new AdditionalServiceForBooking();
                        asfb.Date = date;
                        asfb.Booking = booking;
                        asfb.AdditionalServiceId = addService.Id;
                        asfbs.Add(asfb);

                        if (doDebug)
                            _logger.LogInformation("Inserting AdditionalServiceForBooking | AdditionalService ID:" + addService.Id + " | " + date);
                    }
                }
            }

            booking.AdditionalServiceForBookings = asfbs;
            booking.RoomForBookings = rfbs;

            await _dataContext.Bookings.AddAsync(booking);
            await _dataContext.SaveChangesAsync();

            _rm.AddReservationTimer(booking.Id, creationTimestamp);
            await _emailDispatcher.DispatchBookingConfirmed(booking.Id);
            await _emailDispatcher.DispatchBookingConfirmedAdmin(booking.Id);

            return Ok(JsonSerializer.Serialize(booking.Id));
        }
    }
}
