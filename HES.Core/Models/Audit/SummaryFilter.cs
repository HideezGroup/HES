using System;

namespace HES.Core.Models.Audit
{
    public class SummaryFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Employee { get; set; }
        public string Workstation { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
    }
}