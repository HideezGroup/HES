using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Identity
{
    public class UserProfileModel
    {
        [Required]
        public string UserId { get; set; }

        [Display(Name = "Name")]
        public string FullName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }
}