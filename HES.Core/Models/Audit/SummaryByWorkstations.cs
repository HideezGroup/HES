using System;

namespace HES.Core.Models.Audit
{
    public class SummaryByWorkstations
    {
        public string Workstation { get; set; }
        public int EmployeesCount { get; set; }
        public int TotalSessionsCount { get; set; }
        public TimeSpan TotalSessionsDuration { get; set; }
        public TimeSpan AvgSessionsDuration { get; set; }
        public TimeSpan AvgTotalDuartionByEmployee { get; set; }
        public decimal AvgTotalSessionsCountByEmployee { get; set; }
    }
}