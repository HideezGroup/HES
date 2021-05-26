using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.LicenseOrders
{
    public class RenewLicenseOrder
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_ContactEmail), ResourceType = typeof(Resources.Resource))]
        [RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessageResourceName = nameof(Resources.Resource.Validation_EmailAddress), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ContactEmail { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Note), ResourceType = typeof(Resources.Resource))]
        public string Note { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_EndDate), ResourceType = typeof(Resources.Resource))]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);

        public List<HardwareVault> HardwareVaults { get; set; }

        public string SearchText { get; set; } = string.Empty;
    }
}