using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Identity;
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
    public class InviteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPageSyncService _synchronizationService;
        private readonly ILogger<InviteModel> _logger;

        public InviteModel(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IPageSyncService synchronizationService,
                           ILogger<InviteModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _synchronizationService = synchronizationService;
            _logger = logger;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public UserInviteModel Input { get; set; }

        public IActionResult OnGet(string code = null, string email = null)
        {
            try
            {
                if (code == null)
                {
                    ErrorMessage = Resources.Resource.Identity_Invite_CodeMustBeSupplied;
                    return Page();
                }
                else
                {
                    Input = new UserInviteModel
                    {
                        Code = code,
                        Email = email
                    };
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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ErrorMessage = Resources.Resource.Identity_Invite_EmailAddressDoesNotExist;
                    return Page();
                }

                var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    user.FirstName = Input.FirstName;
                    user.LastName = Input.LastName;
                    await _userManager.UpdateAsync(user);
                    await _synchronizationService.UpdateAdministratorState();

                    var loginResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, false, lockoutOnFailure: true);
                    if (loginResult.Succeeded)
                    {
                        return LocalRedirect(Routes.Dashboard);
                    }
                }

                ErrorMessage = HESException.GetIdentityResultErrors(result.Errors);
                return Page();
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