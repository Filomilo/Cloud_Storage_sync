using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cloud_Storage_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        public IActionResult login(string email, string password)
        {
            //test
            User user = new User()
            {
                mail = email,
                password = password
            };
            return Ok(AuthService.GenerateToken(user));
        }
    }
}
