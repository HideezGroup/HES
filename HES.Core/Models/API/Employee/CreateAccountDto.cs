using HES.Core.Attributes;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class CreateAccountDto
    {
        [Required]
        public string EmployeeId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Urls { get; set; }

        public string Apps { get; set; }

        public LoginType LoginType { get; set; }

        [Display(Name = "Login")]
        [ValidateLogin(nameof(LoginType))]
        public string Login { get; set; }

        [Display(Name = "Domain")]
        [ValidateDomain(nameof(LoginType), ErrorMessage = "The Domain field is required.")]
        public string Domain { get; set; }

        [Required]
        public string Password { get; set; }

        public string OtpSecret { get; set; }
    }
}