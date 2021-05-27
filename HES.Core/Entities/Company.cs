using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        public List<Department> Departments { get; set; }
    }
}