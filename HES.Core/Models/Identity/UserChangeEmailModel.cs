using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserChangeEmailModel
    {
        [Display(Name = nameof(Resources.Resource.Display_CurrentEmail), ResourceType = typeof(Resources.Resource))]
        [EmailAddress(ErrorMessageResourceName = nameof(Resources.Resource.Validation_EmailAddress), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string CurrentEmail { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_NewEmail), ResourceType = typeof(Resources.Resource))]
        [EmailAddress(ErrorMessageResourceName = nameof(Resources.Resource.Validation_EmailAddress), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string NewEmail { get; set; }
    }
}