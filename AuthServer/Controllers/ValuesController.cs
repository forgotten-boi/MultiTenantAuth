using AuthServer.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    [Route("api/Values")]
    public class ValuesController : Controller
    {
        [HttpGet]
        [Authorize("scope1")]
        public IActionResult Get()
        {
            return Ok("Hello");
        }
    }
}