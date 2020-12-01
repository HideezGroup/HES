using HES.Core.Attributes;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.SharedAccounts
{
    public class SharedAccountUpdateModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Display(Name = "Urls")]
        public string Urls { get; set; }

        [Display(Name = "Apps")]
        public string Apps { get; set; }

        [Display(Name = "Login Type")]
        public LoginType LoginType { get; set; }

        [Display(Name = "Login")]
        [ValidateLogin(nameof(LoginType))]
        public string Login { get; set; }

        [Display(Name = "Domain")]
        [ValidateDomain(nameof(LoginType), ErrorMessage = "The Domain field is required.")]
        public string Domain { get; set; }
    }
}