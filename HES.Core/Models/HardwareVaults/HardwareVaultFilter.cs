using HES.Core.Enums;
using System;

namespace HES.Core.Models.HardwareVaults
{
    public class HardwareVaultFilter
    {
        public string Id { get; set; }
        public string MAC { get; set; }
        public string Model { get; set; }
        public string RFID { get; set; }
        public string Battery { get; set; }
        public string Firmware { get; set; }
        public DateTime? LastSyncedStartDate { get; set; }
        public DateTime? LastSyncedEndDate { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public VaultStatus? Status { get; set; }
        public VaultLicenseStatus? LicenseStatus { get; set; }
        public DateTime? LicenseEndDate { get; set; }
    }
}