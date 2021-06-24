using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class DataProtectionNewPasswordModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Password), ResourceType = typeof(Resources.Resource))]
        [StringLength(100, ErrorMessageResourceName = nameof(Resources.Resource.Validation_StringLength), MinimumLength = 6, ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_ConfirmPassword), ResourceType = typeof(Resources.Resource))]
        [CompareProperty("Password", ErrorMessageResourceName = nameof(Resources.Resource.Validation_ComparePassword), ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}