using HES.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class LicensingSettings
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_ApiKey), ResourceType = typeof(Resources.Resource))]
        public string ApiKey { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Url(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Url), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_ApiAddress), ResourceType = typeof(Resources.Resource))]
        public string ApiAddress { get; set; } = ServerConstants.LicenseAddress;
    }
}