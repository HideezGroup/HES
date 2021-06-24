using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class PasswordSignInModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Email), ResourceType = typeof(Resources.Resource))]
        [EmailAddress(ErrorMessageResourceName = nameof(Resources.Resource.Validation_EmailAddress), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Password), ResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_RememberMe), ResourceType = typeof(Resources.Resource))]
        public bool RememberMe { get; set; }
    }
}