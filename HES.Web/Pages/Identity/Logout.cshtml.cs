using HES.Core.Constants;
using HES.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPost()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                await _signInManager.UserManager.UpdateSecurityStampAsync(user);
                await _signInManager.SignOutAsync();
            }
            return LocalRedirect(Routes.Login);
        }
    }
}