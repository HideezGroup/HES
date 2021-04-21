using System;

namespace HES.Core.Models.Filters
{
    public class EmployeeFilter
    {
        public string Employee { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public DateTime? LastSeenStartDate { get; set; }
        public DateTime? LastSeenEndDate { get; set; }
        public int? VaultsCount { get; set; }
    }
}