using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppSettings
{
    public class LdapSettings
    {
        [Required]
        [Display(Name = "Domain Name")]
        public string Host { get; set; }
        [Required]
        [Display(Name = "User Logon Name")]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        [Range(1, 180)]
        [Display(Name = "Auto Password Change")]
        public int MaxPasswordAge { get; set; } = 28;
    }
}