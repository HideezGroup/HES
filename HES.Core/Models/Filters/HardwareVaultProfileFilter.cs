using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Filters
{
    public class HardwareVaultProfileFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_CreatedAtDateFrom), ResourceType = typeof(Resources.Resource))]
        public DateTime? CreatedAtFrom { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_CreatedAtDateTo), ResourceType = typeof(Resources.Resource))]
        public DateTime? CreatedAtTo { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_UpdatedAtFrom), ResourceType = typeof(Resources.Resource))]
        public DateTime? UpdatedAtFrom { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_UpdatedAtFrom), ResourceType = typeof(Resources.Resource))]
        public DateTime? UpdatedAtTo{ get; set; }

        [Display(Name = nameof(Resources.Resource.Display_HardwareVaultsCount), ResourceType = typeof(Resources.Resource))]
        public int? HardwareVaultsCount { get; set; }
    }
}