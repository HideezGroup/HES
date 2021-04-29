using System;

namespace HES.Core.Helpers
{
    public static class UnixTimeHelper
    {
        public static uint GetUnixTimeUtcNow()
        {
            return ConvertToUnixTime(DateTime.UtcNow);
        }

        public static uint ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (uint)(datetime - sTime).TotalSeconds;
        }

        public static DateTime UnixTimeToDateTime(uint unixtime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return sTime.AddSeconds(unixtime).ToLocalTime();
        }
    }
}