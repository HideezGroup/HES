using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class LdapSettings
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_DomainName), ResourceType = typeof(Resources.Resource))]
        public string Host { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_UserLogonName), ResourceType = typeof(Resources.Resource))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Password), ResourceType = typeof(Resources.Resource))]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_AutoPasswordChange), ResourceType = typeof(Resources.Resource))]
        [Range(1, 180, ErrorMessageResourceName = nameof(Resources.Resource.Validation_Range), ErrorMessageResourceType = typeof(Resources.Resource))]
        public int MaxPasswordAge { get; set; } = 28;
    }
}