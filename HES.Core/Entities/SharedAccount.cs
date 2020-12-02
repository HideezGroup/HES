using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class SharedAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Display(Name = "Urls")]
        public string Urls { get; set; }

        [Display(Name = "Applications")]
        public string Apps { get; set; }

        [Display(Name = "Login Type")]
        public LoginType LoginType { get; set; }

        [Required]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Password Changed At")]
        public DateTime? PasswordChangedAt { get; set; }

        [Display(Name = "OTP Secret")]
        public string OtpSecret { get; set; }

        [Display(Name = "OTP Secret Changed At")]
        public DateTime? OtpSecretChangedAt { get; set; }

        public bool Deleted { get; set; }

        [NotMapped]
        public TimeSpan GetPasswordUpdated => (DateTime.UtcNow).Subtract(PasswordChangedAt ?? DateTime.UtcNow);

        [NotMapped]
        public TimeSpan GetOtpUpdated => (DateTime.UtcNow).Subtract(OtpSecretChangedAt ?? DateTime.UtcNow);
    }
}