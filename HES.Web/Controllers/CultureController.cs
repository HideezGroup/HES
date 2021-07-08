using HES.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class CultureController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CultureController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> SetCulture(string culture, string redirectUri)
        {
            if (!string.IsNullOrWhiteSpace(culture))
            {
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(culture)), new CookieOptions() { Expires = DateTime.Now.AddYears(10) });

                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.Culture != culture)
                {
                    user.Culture = culture;
                    await _userManager.UpdateAsync(user);
                }
            }

            return LocalRedirect(redirectUri);
        }
    }
}