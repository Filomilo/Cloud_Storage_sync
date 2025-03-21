using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cloud_Storage_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelathController : ControllerBase
    {
        [HttpGet]
        [Route("health")]
        public ActionResult health()
        {
            return Ok("healthy");
        }

        [HttpGet]
        [Authorize]
        [Route("healthSecured")]
        public ActionResult healthSecured()
        {
            return Ok("healthy");
        }
    }
}
