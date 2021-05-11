using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class SplunkSettings
    {
        [Url]
        [Required]
        public string Host { get; set; }

        [Required]
        public string Token { get; set; }
    }
}