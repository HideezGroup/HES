using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_FirstName), ResourceType = typeof(Resources.Resource))]
        public string FirstName { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastName), ResourceType = typeof(Resources.Resource))]
        public string LastName { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Email), ResourceType = typeof(Resources.Resource))]
        [RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_PhoneNumber), ResourceType = typeof(Resources.Resource))]
        public string PhoneNumber { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Department), ResourceType = typeof(Resources.Resource))]
        public string DepartmentId { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Position), ResourceType = typeof(Resources.Resource))]
        public string PositionId { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastSeen), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSeen { get; set; }

        public DateTime? WhenChanged { get; set; }

        public string PrimaryAccountId { get; set; }

        public string ActiveDirectoryGuid { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_External_Id), ResourceType = typeof(Resources.Resource))]
        public string ExternalId { get; set; }

        public List<Account> Accounts { get; set; }
        public List<HardwareVault> HardwareVaults { get; set; }
        public List<GroupMembership> GroupMemberships { get; set; }
        public List<SoftwareVault> SoftwareVaults { get; set; }
        public List<SoftwareVaultInvitation> SoftwareVaultInvitations { get; set; }
        public List<WorkstationEvent> WorkstationEvents { get; set; }
        public List<WorkstationSession> WorkstationSessions { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [ForeignKey("PositionId")]
        public Position Position { get; set; }

        [NotMapped]
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [NotMapped]
        public int VaultsCount => (HardwareVaults == null ? 0 : HardwareVaults.Count) + (SoftwareVaults == null ? 0 : SoftwareVaults.Count);
    }
}