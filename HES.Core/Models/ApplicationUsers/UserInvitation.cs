using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.ApplicationUsers
{
    public class UserInvitation
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Email), ResourceType = typeof(Resources.Resource))]
        [EmailAddress(ErrorMessageResourceName = nameof(Resources.Resource.Validation_EmailAddress), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Email { get; set; }
    }
}