using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.ApplicationUsers
{
    public class VerificationCode
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [StringLength(7, ErrorMessageResourceName = nameof(Resources.Resource.Validation_StringLength), MinimumLength = 6, ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_VerificationCode), ResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Text)]
        public string Code { get; set; }
    }
}