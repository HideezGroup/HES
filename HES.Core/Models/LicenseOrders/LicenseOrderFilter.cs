using System;
using HES.Core.Enums;

namespace HES.Core.Models.LicenseOrders
{
    public class LicenseOrderFilter
    {
        public string Note { get; set; }
        public string ContactEmail { get; set; }
        public bool? ProlongLicense { get; set; }
        public DateTime? LicenseStartDateStart { get; set; }
        public DateTime? LicenseStartDateEnd { get; set; }
        public DateTime? LicenseEndDateStart { get; set; }
        public DateTime? LicenseEndDateEnd { get; set; }
        public DateTime? CreatedDateStart { get; set; }
        public DateTime? CreatedDateEnd { get; set; }
        public LicenseOrderStatus? OrderStatus { get; set; }
    }
}
