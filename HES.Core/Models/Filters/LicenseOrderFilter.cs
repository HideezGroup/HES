using System;
using System.ComponentModel.DataAnnotations;
using HES.Core.Enums;

namespace HES.Core.Models.Filters
{
    public class LicenseOrderFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_Note), ResourceType = typeof(Resources.Resource))]
        public string Note { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_ContactEmail), ResourceType = typeof(Resources.Resource))]
        public string ContactEmail { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_ProlongLicense), ResourceType = typeof(Resources.Resource))]
        public bool? ProlongLicense { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LicenseStartDateFrom), ResourceType = typeof(Resources.Resource))]
        public DateTime? LicenseStartDateFrom { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LicenseStartDateTo), ResourceType = typeof(Resources.Resource))]
        public DateTime? LicenseStartDateTo { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LicenseEndDateFrom), ResourceType = typeof(Resources.Resource))]
        public DateTime? LicenseEndDateFrom { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LicenseEndDateTo), ResourceType = typeof(Resources.Resource))]
        public DateTime? LicenseEndDateTo { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_OrderCreatedAtDateFrom), ResourceType = typeof(Resources.Resource))]
        public DateTime? CreatedAtDateFrom { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_OrderCreatedAtDateTo), ResourceType = typeof(Resources.Resource))]
        public DateTime? CreatedAtDateTo { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_OrderStatus), ResourceType = typeof(Resources.Resource))]
        public LicenseOrderStatus? OrderStatus { get; set; }
    }
}