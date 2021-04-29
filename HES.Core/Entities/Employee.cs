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
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        [Display(Name = "Position")]
        public string PositionId { get; set; }
        [Display(Name = "Last Seen")]
        public DateTime? LastSeen { get; set; }
        public DateTime? WhenChanged { get; set; }
        public string PrimaryAccountId { get; set; }
        public string ActiveDirectoryGuid { get; set; }
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
        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}".Trim();
        [NotMapped]
        public int VaultsCount => (HardwareVaults == null ? 0 : HardwareVaults.Count) + (SoftwareVaults == null ? 0 : SoftwareVaults.Count);
    }
}