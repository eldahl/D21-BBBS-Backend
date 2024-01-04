using BBBSBackend.DBModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using BBBSBackend.Controllers;

namespace BBBSBackend
{
    public class EmailDispatcher : INotifyDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailDispatcher> _logger;
        public EmailDispatcher(IServiceScopeFactory scopeFactory, ILogger<EmailDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task DispatchBookingCanceled(Guid bookingId)
        {
            throw new NotImplementedException();
        }

        public async Task DispatchBookingConfirmed(Guid bookingId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BbbsContext>();

                var bookingQuery = db.Bookings.Where(b => b.Id == bookingId).Include(b => b.Customer);

                // Error: We need there to be a booking in order to notify.
                bool any = await bookingQuery.AnyAsync();
                if (!any)
                {
                    _logger.LogInformation("[EmailDispatcher]: Failed to find booking for supplied ID.");
                    return;
                }

                var booking = await bookingQuery.FirstAsync();

                var smtpClient = new SmtpClient("smtp.gmail.com") {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("bedbreakfastbookingsystem@gmail.com", "[GOOGLE APP CODE REDACTED]")
                };

                string cancelationToken = Util.CreateCancelationToken(bookingId, booking.Customer.Email);
                string cancelationURL = "http://123.123.123.123:3000/Booking/CancelBooking?cancelationToken=" + cancelationToken;

                var message = new MailMessage("bedbreakfastbookingsystem@gmail.com", booking.Customer.Email) {
                    Subject = "Bed & Breakfast | Booking Confirmed",
                    Body = "Hello!\n\n" +
                           "Thanks for making a booking at Iller Slot!.\n" +
                           "This is a confirmation that your booking has been finalized.\n\n" +
                           "If you wish to cancel the booking, please click the following link: " + cancelationURL + "\n\n" +
                           "We can't wait to see you!\n" +
                           "Iller Slot",
                    IsBodyHtml = false
                };
                smtpClient.Send(message);
            }
        }

        public async Task DispatchBookingConfirmedAdmin(Guid bookingId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BbbsContext>();

                var bookingQuery = db.Bookings.Where(b => b.Id == bookingId).Include(b => b.Customer);

                // Error: We need there to be a booking in order to notify.
                bool any = await bookingQuery.AnyAsync();
                if (!any)
                {
                    Trace.WriteLine("[EmailDispatcher]: Failed to find booking for supplied ID.");
                    return;
                }

                var booking = await bookingQuery.FirstAsync();


                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("bedbreakfastbookingsystem@gmail.com", "[GOOGLE APP CODE REDACTED]")
                };

                var message = new MailMessage("bedbreakfastbookingsystem@gmail.com", "bedbreakfastbookingsystem@gmail.com")
                {
                    Subject = booking.Id.ToString(),
                    Body = "A booking has made on the " + booking.CreatedAt + " from the customer: "
                       + "\nName: " + booking.Customer.FirstName + " " + booking.Customer.LastName
                       + "\nPhone: " + booking.Customer.Phone
                       + "\nEmail: " + booking.Customer.Email
                       + "\nCity: " + booking.Customer.City + " | " + booking.Customer.PostalCode
                       + "\nAddress: " + booking.Customer.Address
                       + "\n"
                       + "\nArrival: " + booking.ArrivalDate
                       + "\nDeparture: " + booking.DepartureDate
                       + "\nNumber of people: " + booking.NumberOfPeople
                       + "\n"
                       + "\nComment: "
                       + "\n" + booking.Comment
                       + "\n"
                       ,
                    IsBodyHtml = false
                };

                smtpClient.Send(message);
            }
        }
    }
}
