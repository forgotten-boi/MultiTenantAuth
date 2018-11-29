using System.Threading.Tasks;
using AuthServer.Resources;
using BusinessAccess.Interfaces;
using Entities.Args;
using Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AuthServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public TokenController(IUserService userService, IStringLocalizer<SharedResource> localizer)
        {
            _userService = userService;
            _localizer = localizer;
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] TokenRequestArgs args)
        {
            if (args == null)
                return BadRequest(_localizer.GetValue(SharedResource.SpecifyRequiredParameters));

            var grantType = args.grant_type.ToLower();
            if (grantType == "password")
            {
                var token = await _userService.GetTokenAsync(args);
                return Ok(token);
            }
            if (grantType == "refresh_token")
            {
                var token = await _userService.GetRefreshTokenAsync(args);
                return Ok(token);
            }
            return BadRequest();
        }
        
    }
}
