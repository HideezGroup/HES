using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Models.Identity;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    [AllowAnonymous]
    public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

        public LoginWithRecoveryCodeModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginWithRecoveryCodeModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public UserRecoveryCodeModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            try
            {
                // Ensure the user has gone through the username & password screen first
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                if (user == null)
                {
                    ErrorMessage = Resources.Resource.Identity_LoginWith2fa_UnableToLoad;
                    return Page();
                }

                ReturnUrl = returnUrl;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                if (!NavigationManagerExtensions.IsLocalUrl(returnUrl))
                {
                    returnUrl = null;
                }

                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                if (user == null)
                {
                    ErrorMessage = Resources.Resource.Identity_LoginWith2fa_UnableToLoad;
                    return Page();
                }

                var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);
                var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl ?? Url.Content("~/"));
                }
                if (result.IsLockedOut)
                {
                    return LocalRedirect(Routes.Lockout);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Resources.Resource.Identity_LoginWithRecoveryCode_InvalidRecoveryCode);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}