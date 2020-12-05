using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace HES.Core.Helpers
{
    public static class EnumHelper
    {
        public static string GetDisplayName(Enum value)
        {
            return value.GetType()?.GetMember(value.ToString())?.First()?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }

        public static int StringArrToEnum<T>(string[] array) where T : struct
        {
            int result = 0;
            foreach (var item in array)
                result |= Convert.ToInt32(Enum.Parse<T>(item, true));

            return result;
        }

        public static List<T> ToList<T>(bool removeFirst = false)
        {
            var enums = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            if (removeFirst)
                enums.RemoveAt(0);

            return enums;
        }

        public static string[] EnumToStringArr(Enum value)
        {
            return value.ToString().Split(",").Select(x => x.Trim()).ToArray();
        }
    }
}