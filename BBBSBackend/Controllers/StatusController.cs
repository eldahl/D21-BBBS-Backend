using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BBBSBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("GetOnline")]
        public ActionResult<string> Get()
        {
            return Ok(JsonSerializer.Serialize("Online"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetOnlineAdmin")]
        public ActionResult<string> GetAdmin()
        {
            return Ok(JsonSerializer.Serialize("Get Online Admin"));
        }
    }
}
