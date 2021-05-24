using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserInviteModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [EmailAddress(ErrorMessageResourceName = nameof(Resources.Resource.Validation_EmailAddress), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_FirstName), ResourceType = typeof(Resources.Resource))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_LastName), ResourceType = typeof(Resources.Resource))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Password), ResourceType = typeof(Resources.Resource))]
        [StringLength(100, ErrorMessageResourceName = nameof(Resources.Resource.Validation_StringLength), MinimumLength = 6, ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_ConfirmPassword), ResourceType = typeof(Resources.Resource))]
        [Compare("Password", ErrorMessageResourceName = nameof(Resources.Resource.Validation_ComparePassword), ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}