using HES.Core.Enums;
using System;

namespace HES.Core.Models.SoftwareVault
{
    public class SoftwareVaultInvitationFilter
    {
        public string Id { get; set; }
        public DateTime? CreatedAtStartDate { get; set; }
        public DateTime? CreatedAtEndDate { get; set; }
        public DateTime? ValidToStartDate { get; set; }
        public DateTime? ValidToEndDate { get; set; }
        public SoftwareVaultInvitationStatus? Status { get; set; }
        public string Email { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
    }
}