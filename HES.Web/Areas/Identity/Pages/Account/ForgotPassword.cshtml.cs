using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;
        private readonly IEmployeeService _employeeService;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager,
                                   IEmailSenderService emailSender,
                                   IEmployeeService employeeService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _employeeService = employeeService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage(Routes.ForgotPasswordConfirmation);
                }

                var employee = await _employeeService.EmployeeQuery().FirstOrDefaultAsync(e => e.Email == Input.Email);
                var resetPasswordUrl = "/Account/ResetPassword";
                var emailBody = $"Dear {Input.Email} <br/> Please reset your password by";

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var email = Input.Email;
                var callbackUrl = Url.Page(
                    resetPasswordUrl,
                    pageHandler: null,
                    values: new { code, email },
                    protocol: Request.Scheme);

                await _emailSender.SendUserResetPasswordAsync(Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

                return RedirectToPage(Routes.ForgotPasswordConfirmation);
            }

            return Page();
        }
    }
}