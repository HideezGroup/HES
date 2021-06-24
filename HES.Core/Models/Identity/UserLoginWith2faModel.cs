using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserLoginWith2faModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [StringLength(100, ErrorMessageResourceName = nameof(Resources.Resource.Validation_StringLength), MinimumLength = 6, ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_AuthenticatorCode), ResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_RememberMachine), ResourceType = typeof(Resources.Resource))]
        public bool RememberMachine { get; set; }
    }
}