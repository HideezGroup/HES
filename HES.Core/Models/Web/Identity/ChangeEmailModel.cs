using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Identity
{
    public class ChangeEmailModel
    {
        [Display(Name = "Current Email")]
        [EmailAddress]
        public string CurrentEmail { get; set; }

        [Display(Name = "New Email")]
        [Required]
        public string NewEmail { get; set; }
    }
}