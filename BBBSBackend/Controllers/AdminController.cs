using BBBSBackend.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BBBSBackend.Controllers
{
    public class User { 
        public String? Email { get; set; }
        public String? Password { get; set; }
    }

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly BbbsContext _dataContext;
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration config, BbbsContext dataContext) { 
            _configuration = config;
            _dataContext = dataContext;
        }

        [AllowAnonymous]
        [HttpPost("Auth")]
        public ActionResult<string> Auth([FromBody]User userParam)
        {
            /*
            // Check that we have arguments required to continue.
            if(userParam.Email == null || userParam.Password == null)
                return BadRequest("Authentication data was not supplied.");

            var validUsers = from u in _dataContext.AdminUsers
                             where u.Email == userParam.Email select u;

            // No users with given email? Error!
            if (!validUsers.Any()) {
                return NotFound("User is invalid.");
            }

            // Get the first occurance in query, as there should only be one unique instance of the email.
            var user = validUsers.First();
            */

            if (userParam.Email == "test" && userParam.Password == "test123") {
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor {
                    Subject = new ClaimsIdentity(new[] {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, userParam.Email!),
                        new Claim(JwtRegisteredClaimNames.Email, userParam.Email!),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, "Admin")
                    }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = credentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                return Ok(jwtToken);
            }
            return Unauthorized();
        }

        [HttpPost("GetScheduleBookingsForPeriod")]
        public async Task<ActionResult<List<Booking>>> GetScheduleBookingsForPeriod([FromBody] GetScheduleBookingsForPeriodParams PeriodParams)
        {
            var bookings = await _dataContext.Bookings
                .Where(b => PeriodParams.StartDate <= b.DepartureDate && PeriodParams.EndDate >= b.ArrivalDate)
                .Include(b => b.Customer)
                .Include(b => b.AdditionalServiceForBookings)
                    .ThenInclude(asfb => asfb.AdditionalService)
                .Include(b => b.RoomForBookings)
                    .ThenInclude(rfb => rfb.Room)
                .ToListAsync();

            return Ok(bookings);
        }

    }
}
