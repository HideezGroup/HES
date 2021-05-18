using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserProfileModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }
}