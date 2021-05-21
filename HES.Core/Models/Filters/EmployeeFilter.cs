using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Filters
{
    public class EmployeeFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_Employee), ResourceType = typeof(Resources.Resource))]
        public string Employee { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Email), ResourceType = typeof(Resources.Resource))]
        public string Email { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_PhoneNumber), ResourceType = typeof(Resources.Resource))]
        public string PhoneNumber { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Company), ResourceType = typeof(Resources.Resource))]
        public string Company { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Department), ResourceType = typeof(Resources.Resource))]
        public string Department { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Position), ResourceType = typeof(Resources.Resource))]
        public string Position { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LastSeenStartDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSeenStartDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public DateTime? LastSeenEndDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_VaultsCount), ResourceType = typeof(Resources.Resource))]
        public int? VaultsCount { get; set; }
    }
}