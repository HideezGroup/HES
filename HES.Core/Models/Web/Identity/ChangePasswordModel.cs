using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Identity
{
    public class ChangePasswordModel
    {
        [Required]
        public string UserId { get; set; }

        [Display(Name = "Current Password")]
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Display(Name = "New Password")]
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [Required]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}