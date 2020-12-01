using HES.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Urls")]
        public string Urls { get; set; }

        [Display(Name = "Application")]
        public string Apps { get; set; }

        [Required]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Display(Name = "Login Type")]
        public LoginType LoginType { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Password Updated")]
        public DateTime PasswordUpdatedAt { get; set; }

        [Display(Name = "OTP Updated")]
        public DateTime? OtpUpdatedAt { get; set; }

        public string Password { get; set; }

        public string OtpSecret { get; set; }

        public bool UpdateInActiveDirectory { get; set; }

        public bool Deleted { get; set; }

        [Required]
        public byte[] StorageId { get; set; }

        public uint Timestamp { get; set; }

        public string EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        public string SharedAccountId { get; set; }

        [ForeignKey("SharedAccountId")]
        public SharedAccount SharedAccount { get; set; }

        public List<WorkstationEvent> WorkstationEvents { get; set; }
        public List<WorkstationSession> WorkstationSessions { get; set; }
    }
}