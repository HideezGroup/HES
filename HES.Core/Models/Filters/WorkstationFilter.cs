using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Filters
{
    public class WorkstationFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Domain), ResourceType = typeof(Resources.Resource))]
        public string Domain { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_ClientVersion), ResourceType = typeof(Resources.Resource))]
        public string ClientVersion { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Company), ResourceType = typeof(Resources.Resource))]
        public string Company { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Department), ResourceType = typeof(Resources.Resource))]
        public string Department { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_OS), ResourceType = typeof(Resources.Resource))]
        public string OS { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_IP), ResourceType = typeof(Resources.Resource))]
        public string IP { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastSeenStartDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSeenStartDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastSeenEndDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSeenEndDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_RFID), ResourceType = typeof(Resources.Resource))]
        public bool? RFID { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Approved), ResourceType = typeof(Resources.Resource))]
        public bool? Approved { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Online), ResourceType = typeof(Resources.Resource))]
        public bool? Online { get; set; }
    }
}