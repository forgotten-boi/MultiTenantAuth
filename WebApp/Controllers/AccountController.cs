using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        public async Task<IActionResult> Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                await SignOutAsync();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            await SignInAsync(userName, password);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInAsync(string userName, string password, bool isPersistent = false)
        {
            var identity = new ClaimsIdentity(null, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal, new AuthenticationProperties
            {
                IsPersistent = isPersistent
            });
        }

        private async Task SignOutAsync()
        {
            await HttpContext.SignOutAsync();
            //clear sessions and caches also
            HttpContext.Session.Clear();
        }
    }
}