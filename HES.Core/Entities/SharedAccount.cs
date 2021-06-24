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
        public string Name { get; set; }

        public string Urls { get; set; }

        public string Apps { get; set; }

        public LoginType LoginType { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        public DateTime? PasswordChangedAt { get; set; }

        public string OtpSecret { get; set; }

        public DateTime? OtpSecretChangedAt { get; set; }

        public bool Deleted { get; set; }

        [NotMapped]
        public TimeSpan GetPasswordUpdated => (DateTime.UtcNow).Subtract(PasswordChangedAt ?? DateTime.UtcNow);

        [NotMapped]
        public TimeSpan GetOtpUpdated => (DateTime.UtcNow).Subtract(OtpSecretChangedAt ?? DateTime.UtcNow);
    }
}