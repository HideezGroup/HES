using HES.Core.Enums;
using Hideez.SDK.Communication;
using System;

namespace HES.Core.Models.Audit
{
    public class WorkstationEventFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public WorkstationEventType? Event { get; set; }
        public WorkstationEventSeverity? Severity { get; set; }
        public string Note { get; set; }
        public string Workstation { get; set; }
        public string UserSession { get; set; }
        public string HardwareVault { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Account { get; set; }
        public AccountType? AccountType { get; set; }
    }
}