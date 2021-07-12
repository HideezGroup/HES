using HES.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class LicensingSettings
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_ApiKey), ResourceType = typeof(Resources.Resource))]
        public string ApiKey { get; set; }
    }
}