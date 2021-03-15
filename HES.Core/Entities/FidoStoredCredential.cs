using Fido2NetLib.Objects;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class FidoStoredCredential
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string SecurityKeyName { get; set; }
        public string Username { get; set; }
        public byte[] UserId { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }

        [NotMapped]
        public PublicKeyCredentialDescriptor Descriptor
        {
            get { return string.IsNullOrWhiteSpace(DescriptorJson) ? null : JsonConvert.DeserializeObject<PublicKeyCredentialDescriptor>(DescriptorJson); }
            set { DescriptorJson = JsonConvert.SerializeObject(value); }
        }
        public string DescriptorJson { get; set; }
    }
}