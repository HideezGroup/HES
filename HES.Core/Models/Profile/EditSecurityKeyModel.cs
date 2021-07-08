using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Profile
{
    public class EditSecurityKeyModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }
    }
}