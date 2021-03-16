using HES.Core.Constants;
using HES.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rsk.Saml.Services;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    public class SingleLogOutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISamlInteractionService _samlInteractionService;

        public string ReturnUrl { get; set; }

        public SingleLogOutModel(SignInManager<ApplicationUser> signInManager, ISamlInteractionService samlInteractionService)
        {
            _signInManager = signInManager;
            _samlInteractionService = samlInteractionService;
        }

        public async Task<IActionResult> OnGetAsync(string requestId)
        {
            if (requestId == null)
            {
                return LocalRedirect(Routes.SingleSignOn);
            }

            ReturnUrl = await _samlInteractionService.GetLogoutCompletionUrl(requestId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }

            if (ReturnUrl != null)
            {
                return Redirect(ReturnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}