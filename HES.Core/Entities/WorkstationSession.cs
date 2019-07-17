﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class WorkstationSession
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        [Display(Name = "Unlocked by")]
        public WorkstationUnlockId UnlockedBy { get; set; }
        public string WorkstationId { get; set; }
        [Display(Name = "Session")]
        public string UserSession { get; set; }
        public string DeviceId { get; set; }
        public string EmployeeId { get; set; }
        public string DepartmentId { get; set; }      
        public string DeviceAccountId { get; set; }

        [ForeignKey("WorkstationId")]
        public Workstation Workstation { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        [Display(Name = "Account")]
        [ForeignKey("DeviceAccountId")]
        public DeviceAccount DeviceAccount { get; set; }
    }

    public enum WorkstationUnlockId : byte
    {
        RFID,
        Dongle,
        Proximity,
        NonHideez
    }
}