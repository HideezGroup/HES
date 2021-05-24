using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSenderService emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public UserEmailModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return LocalRedirect(Routes.ForgotPasswordConfirmation);
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = $"{Request.Scheme}://{Request.Host}{Routes.ResetPassword}?code={WebUtility.UrlEncode(code)}&Email={Input.Email}";
                await _emailSender.SendUserResetPasswordAsync(Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

                return LocalRedirect(Routes.ForgotPasswordConfirmation);
            }

            return Page();
        }
    }
}