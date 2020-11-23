using System;
using System.Collections.Generic;

namespace HES.Core.Exceptions
{
    public enum HESCode
    {
        None,
        // Employees
        ActiveDirectoryUserNotFound,

        // Settings
        LdapSettingsNotSet,
    }

    public class HESException : Exception
    {
        private static readonly Dictionary<HESCode, string> Errors = new Dictionary<HESCode, string>()
        {
            { HESCode.None,  "Something went wrong." },
            { HESCode.ActiveDirectoryUserNotFound,  "This employee was removed from active directory so it was changed to local user." },
            { HESCode.LdapSettingsNotSet,  "LDAP settings not set." },
        };

        public HESCode Code { get; set; }

        public HESException(HESCode code) : base(Errors[code])
        {
            Code = code;
        }

        public static string GetMessage(HESCode code)
        {
            return Errors[code];
        }
    }
}