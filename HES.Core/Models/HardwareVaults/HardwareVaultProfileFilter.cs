using System;

namespace HES.Core.Models.HardwareVaults
{
    public class HardwareVaultProfileFilter
    {
        public string Name { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
        public DateTime? CreateddAtTo { get; set; }
        public DateTime? UpdatedAtFrom { get; set; }
        public DateTime? UpdatedAtTo{ get; set; }
        public int? HardwareVaultsCount { get; set; }
    }
}
