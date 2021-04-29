using HES.Core.Enums;

namespace HES.Core.Models.SoftwareVault
{
    public class SoftwareVaultFilter
    {
        public string OS { get; set; }
        public string Model { get; set; }
        public string ClientAppVersion { get; set; }
        public VaultStatus? Status { get; set; }
        public VaultLicenseStatus? LicenseStatus { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
    }
}