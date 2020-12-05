using HES.Core.Attributes;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditSharedAccountDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Urls { get; set; }
        public string Apps { get; set; }
        public LoginType LoginType { get; set; }
        [ValidateLogin(nameof(LoginType))]
        public string Login { get; set; }
        [ValidateDomain(nameof(LoginType), ErrorMessage = "The Domain field is required.")]
        public string Domain { get; set; }
    }
}