using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace HES.Core.Entities
{
    public class SamlRelyingParty
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Issuer")]
        public string Issuer { get; set; }

        [Url]
        [Display(Name = "Assertion Consumer Service")]
        public string SingleSignOnDestination { get; set; }

        [Url]
        [Display(Name = "Single Logout Service")]
        public string SingleLogoutResponseDestination { get; set; }

        [Display(Name = "Public x509 Certificate")]
        public string SignatureValidationCertificateBase64 { get; set; }

        [NotMapped]
        [Display(Name = "Public x509 Certificate")]
        public X509Certificate2 SignatureValidationCertificate
        {
            get
            {
                return string.IsNullOrWhiteSpace(SignatureValidationCertificateBase64) ? null : new X509Certificate2(System.Text.Encoding.ASCII.GetBytes(SignatureValidationCertificateBase64));
            }
            set
            {
                SignatureValidationCertificateBase64 = Convert.ToBase64String(value.Export(X509ContentType.Cert));
            }
        }
    }
}