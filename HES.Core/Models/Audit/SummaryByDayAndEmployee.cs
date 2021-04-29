using System;

namespace HES.Core.Models.Audit
{
    public class SummaryByDayAndEmployee
    {
        public DateTime Date { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public int WorkstationsCount { get; set; }
        public TimeSpan AvgSessionsDuration { get; set; }
        public int SessionsCount { get; set; }
        public TimeSpan TotalSessionsDuration { get; set; }
    }
}