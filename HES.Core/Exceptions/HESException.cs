using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HES.Core.Exceptions
{
    public enum HESCode
    {
        None,

        // User
        UserNotFound,
        InvalidLoginAttempt,
        RequiresTwoFactor,
        AccountLockout,
        IncorrectCurrentPassword,
        EmailAlreadyTaken,
        RequiresRelogin,

        // Employees
        EmployeeNotFound,
        ActiveDirectoryUserNotFound,
        EmployeeAlreadyExist,

        // Hardware Vault
        HardwareVaultNotFound,
        OneHardwareVaultConstraint,
        HardwareVaultCannotReserve,
        HardwareVaultUntieBeforeRemove,
        HardwareVaultВoesNotAllowToRemove,

        // Workstations
        WorkstationNotFound,
        WorkstationHardwareVaultPairNotFound,
        WorkstationHardwareVaultPairAlreadyExist,

        // Settings
        LdapSettingsNotSet,
        AppSettingsNotFound,

        // Accounts
        AccountNotFound,
        AccountExist,
        IncorrectUrl,
        IncorrectOtp,

        //Templates
        TemplateNotFound,
        TemplateExist,

        // SharedAccounts
        SharedAccountNotFound,
        SharedAccountExist,

        // Fido2
        SecurityKeyNotFound,
        AuthenticatorNotFIDO2
    }

    public class HESException : Exception
    {
        private static readonly Dictionary<HESCode, string> Errors = new Dictionary<HESCode, string>()
        {
            { HESCode.None,  "Something went wrong." },
            { HESCode.UserNotFound,  "User not found." },
            { HESCode.InvalidLoginAttempt, "Invalid login attempt." },
            { HESCode.AccountLockout,"Account lockout" },
            { HESCode.IncorrectCurrentPassword,"Incorrect current password." },
            { HESCode.EmailAlreadyTaken,"This email already taken." },
            { HESCode.RequiresRelogin,"Requires relogin." },

            { HESCode.RequiresTwoFactor, "Requires two factor." },

            { HESCode.EmployeeNotFound,  "Employee not found." },
            { HESCode.ActiveDirectoryUserNotFound,  "This employee was removed from active directory so it was changed to local user." },
            { HESCode.EmployeeAlreadyExist,  "Employee with current name already exists." },

            { HESCode.HardwareVaultNotFound,  "Hardware Vault not found." },
            { HESCode.OneHardwareVaultConstraint,  "Cannot add more than one hardware vault." },
            { HESCode.HardwareVaultCannotReserve,  "Vault in a status that does not allow to reserve." },
            { HESCode.HardwareVaultUntieBeforeRemove,  "First untie the hardware vault before removing." },
            { HESCode.HardwareVaultВoesNotAllowToRemove,  "Vault in a status that does not allow to remove." },

            { HESCode.WorkstationNotFound,  "Workstation not found." },
            { HESCode.WorkstationHardwareVaultPairNotFound,  "Workstation and Hardware Vault pair not found." },
            { HESCode.WorkstationHardwareVaultPairAlreadyExist,  "Hardware Vault already added to workstation." },

            { HESCode.LdapSettingsNotSet,  "LDAP settings not set." },
            { HESCode.AppSettingsNotFound,  "Application settings not found." },

            { HESCode.AccountNotFound,  "Account not found." },
            { HESCode.AccountExist,  "Account with the same name and login exist." },
            { HESCode.IncorrectUrl,  "Incorrect URL address." },
            { HESCode.IncorrectOtp,  "Incorrect OTP secret." },

            { HESCode.TemplateNotFound,  "Template not found." },
            { HESCode.TemplateExist,  "Template with current name already exists." },

            { HESCode.SharedAccountNotFound,  "Shared Account not found." },
            { HESCode.SharedAccountExist,  "Shared Account with the same name and login exist." },
            { HESCode.SecurityKeyNotFound,  "Security key not found." },
            { HESCode.AuthenticatorNotFIDO2,  "Authenticator not FIDO2." },
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

        public static string GetIdentityResultErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(". ", errors.Select(x => x.Description).ToArray());
        }
    }
}