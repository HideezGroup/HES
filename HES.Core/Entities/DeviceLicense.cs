﻿using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class DeviceLicense
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string LicenseOrderId { get; set; }
        public DateTime? ImportedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime? EndDate { get; set; }
        public byte[] Data { get; set; }
    }
}