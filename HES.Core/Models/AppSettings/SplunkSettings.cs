using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class SplunkSettings
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_Host), ResourceType = typeof(Resources.Resource))]
        [Url(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Url), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Host { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Dispaly_Token), ResourceType = typeof(Resources.Resource))]
        public string Token { get; set; }
    }
}