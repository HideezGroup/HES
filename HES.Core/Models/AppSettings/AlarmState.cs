using System;

namespace HES.Core.Models.AppSettings
{
    public class AlarmState
    {
        public bool IsAlarm { get; set; }
        public string AdminName { get; set; }
        public DateTime Date { get; set; }
    }
}
