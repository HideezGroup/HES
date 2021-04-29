using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.LicenseOrders
{
    public class RenewLicenseOrder
    {
        [Required]
        [Display(Name = "Contact Email")]
        [RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string ContactEmail { get; set; }

        public string Note { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);

        public List<HardwareVault> HardwareVaults { get; set; }

        public string SearchText { get; set; } = string.Empty;
    }
}