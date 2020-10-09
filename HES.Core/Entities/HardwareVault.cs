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

        [Required]
        public string MAC { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public string RFID { get; set; }

        public int Battery { get; set; }

        [Required]
        public string Firmware { get; set; }

        public VaultStatus Status { get; set; }

        [Display(Name = "Status Reason")]
        public VaultStatusReason StatusReason { get; set; }

        [Display(Name = "Status Description")]
        public string StatusDescription { get; set; }

        [Display(Name = "Last Synced")]
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


        [Display(Name = "License Status")]
        public VaultLicenseStatus LicenseStatus { get; set; }

        [Display(Name = "License End Date")]
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