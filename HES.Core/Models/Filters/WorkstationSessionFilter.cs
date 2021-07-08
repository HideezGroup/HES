using HES.Core.Entities;
using HES.Core.Enums;
using Hideez.SDK.Communication;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HES.Core.Models.Filters
{
    public class WorkstationSessionFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_StartDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? StartDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_EndDate), ResourceType = typeof(Resources.Resource))]
        public DateTime? EndDate { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_UnlockedBy), ResourceType = typeof(Resources.Resource))]
        public SessionSwitchSubject? UnlockedBy { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Workstation), ResourceType = typeof(Resources.Resource))]
        public string Workstation { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_UserSession), ResourceType = typeof(Resources.Resource))]
        public string UserSession { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_HardwareVault), ResourceType = typeof(Resources.Resource))]
        public string HardwareVault { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Employee), ResourceType = typeof(Resources.Resource))]
        public string Employee { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Company), ResourceType = typeof(Resources.Resource))]
        public string Company { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Department), ResourceType = typeof(Resources.Resource))]
        public string Department { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Account), ResourceType = typeof(Resources.Resource))]
        public string Account { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_AccountType), ResourceType = typeof(Resources.Resource))]
        public AccountType? AccountType { get; set; }

        public IQueryable<WorkstationSession> Query { get; set; }
    }
}