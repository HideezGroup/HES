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
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        public LoginWith2faModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public UserLoginWith2faModel Input { get; set; }

        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
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
                RememberMe = rememberMe;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
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

                returnUrl = returnUrl ?? Url.Content("~/");

                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                if (user == null)
                {
                    ErrorMessage = Resources.Resource.Identity_LoginWith2fa_UnableToLoad;
                    return Page();
                }

                var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
                var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    return LocalRedirect(Routes.Lockout);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Resources.Resource.Identity_LoginWith2fa_InvalidAuthenticatorCode);
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