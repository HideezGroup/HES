using System;

namespace HES.Core.Models.Filters
{
    public class WorkstationFilter
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string ClientVersion { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        public string OS { get; set; }
        public string IP { get; set; }
        public DateTime? LastSeenStartDate { get; set; }  
        public DateTime? LastSeenEndDate { get; set; }
        public bool? RFID { get; set; }
        public bool? Approved { get; set; }
        public bool? Online { get; set; }
    }
}