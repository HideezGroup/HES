using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Filters
{
    public class HardwareVaultFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_Id), ResourceType = typeof(Resources.Resource))]
        public string Id { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_MAC), ResourceType = typeof(Resources.Resource))]
        public string MAC { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Model), ResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_RFID), ResourceType = typeof(Resources.Resource))]
        public string RFID { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Battery), ResourceType = typeof(Resources.Resource))]
        public string Battery { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Firmware), ResourceType = typeof(Resources.Resource))]
        public string Firmware { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastSeenStartDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSyncedStartDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastSeenEndDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSyncedEndDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Employee), ResourceType = typeof(Resources.Resource))]
        public string Employee { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Company), ResourceType = typeof(Resources.Resource))]
        public string Company { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Department), ResourceType = typeof(Resources.Resource))]
        public string Department { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Status), ResourceType = typeof(Resources.Resource))]
        public VaultStatus? Status { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LicenseStatus), ResourceType = typeof(Resources.Resource))]
        public VaultLicenseStatus? LicenseStatus { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LicenseEndDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? LicenseEndDate { get; set; }
    }
}