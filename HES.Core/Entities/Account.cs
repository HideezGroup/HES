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

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_AccountName), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Urls), ResourceType = typeof(Resources.Resource))]
        public string Urls { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Apps), ResourceType = typeof(Resources.Resource))]
        public string Apps { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Login), ResourceType = typeof(Resources.Resource))]
        public string Login { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_AccountType), ResourceType = typeof(Resources.Resource))]
        public AccountType AccountType { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LoginType), ResourceType = typeof(Resources.Resource))]
        public LoginType LoginType { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Created), ResourceType = typeof(Resources.Resource))]
        public DateTime CreatedAt { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Updated), ResourceType = typeof(Resources.Resource))]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_PasswordUpdated), ResourceType = typeof(Resources.Resource))]
        public DateTime PasswordUpdatedAt { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_OtpUpdated), ResourceType = typeof(Resources.Resource))]
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