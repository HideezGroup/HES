using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        public List<Employee> Employees { get; set; }
        public List<Workstation> Workstations { get; set; }
        public List<WorkstationEvent> WorkstationEvents { get; set; }
        public List<WorkstationSession> WorkstationSessions { get; set; }
    }
}