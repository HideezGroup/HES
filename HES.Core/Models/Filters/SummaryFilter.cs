using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Filters
{
    public class SummaryFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_StartDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_EndDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? EndDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Employee), ResourceType = typeof(Resources.Resource))]
        public string Employee { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Workstation), ResourceType = typeof(Resources.Resource))]
        public string Workstation { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Company), ResourceType = typeof(Resources.Resource))]
        public string Company { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Department), ResourceType = typeof(Resources.Resource))]
        public string Department { get; set; }
    }
}