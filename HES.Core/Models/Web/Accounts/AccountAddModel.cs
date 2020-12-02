using HES.Core.Attributes;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Accounts
{
    public class AccountAddModel
    {
        [Required]
        public string EmployeeId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Urls (separate by semicolon)")]
        public string Urls { get; set; }

        [Display(Name = "Applications (separate by semicolon)")]
        public string Apps { get; set; }

        [Display(Name = "Login Type")]
        public LoginType LoginType { get; set; }

        [Display(Name = "Login")]
        [ValidateLogin(nameof(LoginType))]
        public string Login { get; set; }

        [Display(Name = "Domain")]
        [ValidateDomain(nameof(LoginType), ErrorMessage = "The Domain field is required.")]
        public string Domain { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [CompareProperty("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "OTP Secret")]
        public string OtpSecret { get; set; }

        public bool UpdateInActiveDirectory { get; set; }

        public string GetLogin()
        {
            return LoginType switch
            {
                LoginType.WebApp => $"{Login}",
                LoginType.Local => $".\\{Login}",
                LoginType.Domain => $"{Domain}\\{Login}",
                LoginType.AzureAD => $"AzureAD\\{Login}",
                LoginType.Microsoft => $"@\\{Login}",
                _ => Login,
            };
        }
    }
}