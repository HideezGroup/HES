using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserProfileModel
    {
        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_FirstName), ResourceType = typeof(Resources.Resource))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_LastName), ResourceType = typeof(Resources.Resource))]
        public string LastName { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_PhoneNumber), ResourceType = typeof(Resources.Resource))]
        public string PhoneNumber { get; set; }
    }
}