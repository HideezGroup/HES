using HES.Core.Entities;
using HES.Core.Enums;
using Hideez.SDK.Communication;
using System;
using System.Linq;

namespace HES.Core.Models.Filters
{
    public class WorkstationSessionFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SessionSwitchSubject? UnlockedBy { get; set; }
        public string Workstation { get; set; }
        public string UserSession { get; set; }
        public string HardwareVault { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Account { get; set; }
        public AccountType? AccountType { get; set; }
        public IQueryable<WorkstationSession> Query { get; set; }
    }
}