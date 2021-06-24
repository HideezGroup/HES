using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Template
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_UrlsSeparate), ResourceType = typeof(Resources.Resource))]
        public string Urls { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_AppsSeparate), ResourceType = typeof(Resources.Resource))]
        public string Apps { get; set; }
    }
}