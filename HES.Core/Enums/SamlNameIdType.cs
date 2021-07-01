using System.ComponentModel.DataAnnotations;

namespace HES.Core.Enums
{
    public enum SamlNameIdentifierType
    {
        [Display(Name = nameof(Resources.Resource.Enum_Email), ResourceType = typeof(Resources.Resource))]
        Email,
        [Display(Name = nameof(Resources.Resource.Enum_External_Id), ResourceType = typeof(Resources.Resource))]
        External
    }
}