using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserChangePasswordModel
    {
        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_CurrentPassword), ResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_NewPassword), ResourceType = typeof(Resources.Resource))]
        [StringLength(100, ErrorMessageResourceName = nameof(Resources.Resource.Validation_StringLength), MinimumLength = 8, ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_ConfirmNewPassword), ResourceType = typeof(Resources.Resource))]
        [Compare("NewPassword", ErrorMessageResourceName = nameof(Resources.Resource.Validation_ComparePassword), ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}