using System;

namespace HES.Core.Models.Audit
{
    public class SummaryByDepartments
    {
        public string Company { get; set; }
        public string Department { get; set; }
        public int EmployeesCount { get; set; }
        public int WorkstationsCount { get; set; }
        public int TotalSessionsCount { get; set; }
        public TimeSpan TotalSessionsDuration { get; set; }
        public TimeSpan AvgSessionsDuration { get; set; }
        public TimeSpan AvgTotalDuartionByEmployee { get; set; }
        public decimal AvgTotalSessionsCountByEmployee { get; set; }
    }
}