﻿using Microsoft.AspNetCore.Identity;
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
        EmployeeRequiresLastName,
        EmployeeRequiresEmail,

        // Hardware Vault
        HardwareVaultNotFound,
        HardwareVaultNotFoundWithParam,
        OneHardwareVaultConstraint,
        HardwareVaultCannotReserve,
        HardwareVaultUntieBeforeRemove,
        HardwareVaultВoesNotAllowToRemove,

        // Workstations
        WorkstationNotFound,
        WorkstationNotFoundWithParam,
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
        IncorrectPassword,
        IncorrectOldPassword,

        // Templates
        TemplateNotFound,
        TemplateExist,

        // SharedAccounts
        SharedAccountNotFound,
        SharedAccountExist,

        // Fido2
        SecurityKeyNotFound,
        AuthenticatorNotFIDO2,

        // Companies
        CompanyNameAlreadyInUse,
        CompanyNotFound,
        DepartmentNameAlreadyInUse,
        DepartmentNotFound,
        PositionNameAlreadyInUse,
        PositionNotFound,

        // License Orders
        LicenseOrderNotFound,
        LicenseForHardwareVaultNotFound,

        // API
        ApiKeyEmpty,

        // Hardware Vault Profile
        HardwareVaultProfileNotFound,
        CannotDeleteDefaultProfile,
        ProfileNameAlreadyInUse,
        ActivationCodeNotFound,

        // Data Protection
        DataProtectionNotFinishedPasswordChange,
        DataProtectionNotActivated,
        DataProtectionNotEnabled,
        DataProtectionParametersNotFound,
        DataProtectionParametersIsEmpty,
        DataProtectionIsAlreadyActivated,
        DataProtectionIsAlreadyEnabled,
        DataProtectionIsBusy,

        // LDAP
        TheLDAPServerIsUnavailable
    }

    public class HESException : Exception
    {
        public HESCode Code { get; set; }

        public HESException(HESCode code) : base(GetMessage(code))
        {
            Code = code;
        }

        public HESException(HESCode code, string[] parameters) : base(string.Format(GetMessage(code), parameters))
        {
            Code = code;
        }

        public static string GetMessage(HESCode code)
        {
            return GetErrorsDictionary()[code];
        }

        private static Dictionary<HESCode, string> GetErrorsDictionary()
        {
            return new Dictionary<HESCode, string>()
            {
                { HESCode.None, "Something went wrong." },
                { HESCode.UserNotFound, "User not found." },
                { HESCode.InvalidLoginAttempt, "Invalid login attempt." },
                { HESCode.AccountLockout, "Account lockout" },
                { HESCode.IncorrectCurrentPassword, "Incorrect current password." },
                { HESCode.EmailAlreadyTaken, "This email already taken." },
                { HESCode.RequiresRelogin, "Requires relogin." },
                { HESCode.RequiresTwoFactor, "Requires two factor." },

                { HESCode.EmployeeNotFound, "Employee not found." },
                { HESCode.ActiveDirectoryUserNotFound,  "This employee was removed from active directory so it was changed to local user." },
                { HESCode.EmployeeAlreadyExist, "Employee with current name already exists." },
                { HESCode.EmployeeRequiresLastName, Resources.Resource.Exception_EmployeeRequiresLastName },
                { HESCode.EmployeeRequiresEmail, Resources.Resource.Exception_EmployeeRequiresEmail },

                { HESCode.HardwareVaultNotFound, "Hardware Vault not found." },
                { HESCode.HardwareVaultNotFoundWithParam,  "Hardware Vault {0} not found." },

                { HESCode.OneHardwareVaultConstraint,  "Cannot add more than one hardware vault." },
                { HESCode.HardwareVaultCannotReserve,  "Vault in a status that does not allow to reserve." },
                { HESCode.HardwareVaultUntieBeforeRemove,  "First untie the hardware vault before removing." },
                { HESCode.HardwareVaultВoesNotAllowToRemove,  "Vault in a status that does not allow to remove." },

                { HESCode.WorkstationNotFound,  "Workstation not found." },
                { HESCode.WorkstationNotFoundWithParam,  "Workstation {0} not found." },
                { HESCode.WorkstationHardwareVaultPairNotFound,  "Workstation and Hardware Vault pair not found." },
                { HESCode.WorkstationHardwareVaultPairAlreadyExist,  "Hardware Vault already added to workstation." },

                { HESCode.LdapSettingsNotSet,  "LDAP settings not set." },
                { HESCode.AppSettingsNotFound,  "Application settings not found." },

                { HESCode.HardwareVaultProfileNotFound,  "Hardware Vault profile not found" },
                { HESCode.CannotDeleteDefaultProfile,  "Cannot delete default profile" },
                { HESCode.ProfileNameAlreadyInUse,  "Profile name already in use" },
                { HESCode.ActivationCodeNotFound,  "Activation code not found" },

                { HESCode.AccountNotFound, "Account not found." },
                { HESCode.AccountExist, "Account with the same name and login exist." },
                { HESCode.IncorrectUrl, "Incorrect URL address." },
                { HESCode.IncorrectOtp, "Incorrect OTP secret." },
                { HESCode.IncorrectPassword, "Incorrect password." },
                { HESCode.IncorrectOldPassword, "Incorrect old password." },

                { HESCode.TemplateNotFound, "Template not found." },
                { HESCode.TemplateExist, "Template with current name already exists." },

                { HESCode.SharedAccountNotFound, "Shared Account not found." },
                { HESCode.SharedAccountExist, "Shared Account with the same name and login exist." },
                { HESCode.SecurityKeyNotFound, "Security key not found." },
                { HESCode.AuthenticatorNotFIDO2, "Authenticator not FIDO2." },

                { HESCode.CompanyNameAlreadyInUse, "Company name already in use." },
                { HESCode.CompanyNotFound, "Company not found." },
                { HESCode.DepartmentNameAlreadyInUse, "Department name already in use" },
                { HESCode.DepartmentNotFound, "Department not found." },
                { HESCode.PositionNameAlreadyInUse, "Position name already in use." },
                { HESCode.PositionNotFound, "Position not found." },

                { HESCode.LicenseOrderNotFound, "License Order not found." },
                { HESCode.LicenseForHardwareVaultNotFound, "Hardware vault licenses not found." },

                { HESCode.ApiKeyEmpty, "Api Key is empty." },

                { HESCode.DataProtectionNotFinishedPasswordChange, "Data protection not finished password change." },
                { HESCode.DataProtectionNotActivated, "Data protection is not activated." },
                { HESCode.DataProtectionNotEnabled, "Data protection is not enabled." },
                { HESCode.DataProtectionParametersNotFound, "Data protection parameters not found." },
                { HESCode.DataProtectionParametersIsEmpty, "Data Protection parameters is empty." },
                { HESCode.DataProtectionIsAlreadyActivated, "Data protection is already activated." },
                { HESCode.DataProtectionIsAlreadyEnabled, "Data protection is already enabled." },
                { HESCode.DataProtectionIsBusy, "Data protection is busy." },

                { HESCode.TheLDAPServerIsUnavailable, Resources.Resource.Exception_TheLDAPServerIsUnavailable },
            };
        }

        public static string GetIdentityResultErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(". ", errors.Select(x => x.Description).ToArray());
        }
    }
}