using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class EmailSettings
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public bool EnableSSL { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}