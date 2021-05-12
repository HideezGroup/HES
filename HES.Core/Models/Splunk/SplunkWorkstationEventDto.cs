using System;
using System.Text.Json;

namespace HES.Core.Models.Splunk
{
    public class SplunkWorkstationEventDto
    {
        public DateTime Date { get; set; }
        public string Event { get; set; }
        public string Severity { get; set; }
        public string Note { get; set; }
        public string Workstation { get; set; }
        public string UserSession { get; set; }
        public string HardwareVault { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Account { get; set; }
        public string AccountType { get; set; }

        public string ToSplunkJson()
        {
            return JsonSerializer.Serialize(new
            {
                @event = new
                {
                    Date,
                    Event,
                    Severity,
                    Note,
                    Workstation,
                    UserSession,
                    HardwareVault,
                    Employee,
                    Company,
                    Department,
                    Account,
                    AccountType
                }
            });
        }
    }
}