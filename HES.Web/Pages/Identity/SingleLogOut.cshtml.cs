using HES.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    public class SingleLogOutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        [TempData]
        public string ReturnUrl { get; set; }

        public SingleLogOutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }

            if (ReturnUrl != null)
            {
                return LocalRedirect(ReturnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}