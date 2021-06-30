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

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Issuer), ResourceType = typeof(Resources.Resource))]
        public string Issuer { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_AssertionConsumerService), ResourceType = typeof(Resources.Resource))]
        [Url(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Url), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string SingleSignOnDestination { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_SingleLogoutService), ResourceType = typeof(Resources.Resource))]
        [Url(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Url), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string SingleLogoutResponseDestination { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_PublicX509Certificate), ResourceType = typeof(Resources.Resource))]
        public string SignatureValidationCertificateBase64 { get; set; }

        [NotMapped]
        [Display(Name = nameof(Resources.Resource.Display_PublicX509Certificate), ResourceType = typeof(Resources.Resource))]
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

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_NameIdentifierFormat), ResourceType = typeof(Resources.Resource))]
        public Uri NameIdentifierFormat { get; set; }
    }
}