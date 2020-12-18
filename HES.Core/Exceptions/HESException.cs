using System;
using System.Collections.Generic;

namespace HES.Core.Exceptions
{
    public enum HESCode
    {
        None,
        // Employees
        EmployeeNotFound,
        ActiveDirectoryUserNotFound,

        // Hardware Vault
        HardwareVaultNotFound,

        // Settings
        LdapSettingsNotSet,

        // Accounts
        AccountNotFound,
        AccountExist,

        // SharedAccounts
        SharedAccountNotFound,
        SharedAccountExist
    }

    public class HESException : Exception
    {
        private static readonly Dictionary<HESCode, string> Errors = new Dictionary<HESCode, string>()
        {
            { HESCode.None,  "Something went wrong." },
            { HESCode.EmployeeNotFound,  "Employee not found." },
            { HESCode.ActiveDirectoryUserNotFound,  "This employee was removed from active directory so it was changed to local user." },
            { HESCode.HardwareVaultNotFound,  "Hardware Vault not found." },
            { HESCode.LdapSettingsNotSet,  "LDAP settings not set." },
            { HESCode.AccountNotFound,  "Account not found." },
            { HESCode.AccountExist,  "Account with the same name and login exist." },
            { HESCode.SharedAccountNotFound,  "Shared Account not found." },
            { HESCode.SharedAccountExist,  "Shared Account with the same name and login exist." },
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