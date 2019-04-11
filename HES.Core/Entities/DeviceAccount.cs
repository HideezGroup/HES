﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class DeviceAccount
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Urls { get; set; }
        public string Apps { get; set; }
        [Required]
        public string Login { get; set; }
        public AccountType Type { get; set; }
        public AccountStatus Status { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PasswordUpdatedAt { get; set; }
        public DateTime? OtpUpdatedAt { get; set; }
        public bool Deleted { get; set; }
        public string EmployeeId { get; set; }
        public string DeviceId { get; set; }
        public string SharedAccountId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("SharedAccountId")]
        public SharedAccount SharedAccount { get; set; }
    }

    public enum AccountType
    {
        Personal,
        Shared
    }

    public enum AccountStatus
    {
        Creating,
        Updating,
        Removing,
        Done
    }
}