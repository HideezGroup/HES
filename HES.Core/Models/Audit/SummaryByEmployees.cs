using System;

namespace HES.Core.Models.Audit
{
    public class SummaryByEmployees
    {
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public int WorkstationsCount { get; set; }
        public int WorkingDaysCount { get; set; }
        public int TotalSessionsCount { get; set; }
        public TimeSpan TotalSessionsDuration { get; set; }
        public TimeSpan AvgSessionsDuration { get; set; }
        public decimal AvgSessionsCountPerDay { get; set; }
        public TimeSpan AvgWorkingHoursPerDay { get; set; }
    }
}