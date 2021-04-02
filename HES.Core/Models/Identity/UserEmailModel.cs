using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserEmailModel
    {
        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}