using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class LicensingSettings
    {
        [Required]
        [Display(Name = "Api Key")]
        public string ApiKey { get; set; }
        
        [Url]
        [Required]
        [Display(Name = "Api Address")]
        public string ApiAddress { get; set; }
    }
}