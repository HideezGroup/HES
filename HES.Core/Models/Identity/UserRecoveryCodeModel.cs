using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserRecoveryCodeModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_RecoveryCode), ResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Text)]
        public string RecoveryCode { get; set; }
    }
}