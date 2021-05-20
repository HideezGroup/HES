using HES.Core.Enums;
using HES.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class HardwareVault
    {
        [Display(Name = "ID")]
        [Key]
        public string Id { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_MAC), ResourceType = typeof(Resources.Resource))]
        [Required]
        public string MAC { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Model), ResourceType = typeof(Resources.Resource))]
        [Required]
        public string Model { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_RFID), ResourceType = typeof(Resources.Resource))]
        [Required]
        public string RFID { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Battery), ResourceType = typeof(Resources.Resource))]
        public int Battery { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Firmware), ResourceType = typeof(Resources.Resource))]
        [Required]
        public string Firmware { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Status), ResourceType = typeof(Resources.Resource))]
        public VaultStatus Status { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_StatusReason), ResourceType = typeof(Resources.Resource))]
        public VaultStatusReason StatusReason { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_StatusDescription), ResourceType = typeof(Resources.Resource))]
        public string StatusDescription { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastSynced), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSynced { get; set; }

        public bool NeedSync { get; set; }

        public string EmployeeId { get; set; }

        public string MasterPassword { get; set; }

        [Required]
        public string HardwareVaultProfileId { get; set; }

        public DateTime ImportedAt { get; set; }

        public uint Timestamp { get; set; }

        public bool HasNewLicense { get; set; }

        public bool IsStatusApplied { get; set; }

        public List<HardwareVaultTask> HardwareVaultTasks { get; set; }


        [Display(Name = nameof(Resources.Resource.Display_LicenseStatus), ResourceType = typeof(Resources.Resource))]
        public VaultLicenseStatus LicenseStatus { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LicenseEndDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? LicenseEndDate { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [Display(Name = "Profile")]
        [ForeignKey("HardwareVaultProfileId")]
        public HardwareVaultProfile HardwareVaultProfile { get; set; }

        [NotMapped]
        public bool IsOnline => RemoteDeviceConnectionsService.IsDeviceConnectedToHost(Id);

        [NotMapped]
        public bool Checked { get; set; }
    }
}