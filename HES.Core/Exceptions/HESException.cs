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
        TheLDAPServerIsUnavailable,

        // SAML
        InvalidCertificate,
        Saml2RelyingPartyNotFound,
        Saml2IssuerAlreadyExist
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
                { HESCode.None, Resources.Resource.Exception_None },
                { HESCode.UserNotFound, Resources.Resource.Exception_UserNotFound },
                { HESCode.InvalidLoginAttempt, Resources.Resource.Exception_InvalidLoginAttempt },
                { HESCode.AccountLockout, Resources.Resource.Exception_AccountLockout },
                { HESCode.IncorrectCurrentPassword, Resources.Resource.Exception_IncorrectCurrentPassword },
                { HESCode.EmailAlreadyTaken, Resources.Resource.Exception_EmailAlreadyTaken },
                { HESCode.RequiresRelogin, Resources.Resource.Exception_RequiresRelogin },
                { HESCode.RequiresTwoFactor, Resources.Resource.Exception_RequiresTwoFactor },

                { HESCode.EmployeeNotFound, Resources.Resource.Exception_EmployeeNotFound },
                { HESCode.ActiveDirectoryUserNotFound,  Resources.Resource.Exception_ActiveDirectoryUserNotFound },
                { HESCode.EmployeeAlreadyExist, Resources.Resource.Exception_EmployeeAlreadyExist },
                { HESCode.EmployeeRequiresLastName, Resources.Resource.Exception_EmployeeRequiresLastName },
                { HESCode.EmployeeRequiresEmail, Resources.Resource.Exception_EmployeeRequiresEmail },

                { HESCode.HardwareVaultNotFound, Resources.Resource.Exception_HardwareVaultNotFound },
                { HESCode.HardwareVaultNotFoundWithParam, Resources.Resource.Exception_HardwareVaultNotFoundWithParam },

                { HESCode.OneHardwareVaultConstraint, Resources.Resource.Exception_OneHardwareVaultConstraint },
                { HESCode.HardwareVaultCannotReserve, Resources.Resource.Exception_HardwareVaultCannotReserve },
                { HESCode.HardwareVaultUntieBeforeRemove, Resources.Resource.Exception_HardwareVaultUntieBeforeRemove },
                { HESCode.HardwareVaultВoesNotAllowToRemove, Resources.Resource.Exception_HardwareVaultВoesNotAllowToRemove },

                { HESCode.WorkstationNotFound, Resources.Resource.Exception_WorkstationNotFound },
                { HESCode.WorkstationNotFoundWithParam, Resources.Resource.Exception_WorkstationNotFoundWithParam },
                { HESCode.WorkstationHardwareVaultPairNotFound, Resources.Resource.Exception_WorkstationHardwareVaultPairNotFound },
                { HESCode.WorkstationHardwareVaultPairAlreadyExist, Resources.Resource.Exception_WorkstationHardwareVaultPairAlreadyExist },

                { HESCode.LdapSettingsNotSet,  Resources.Resource.Exception_LdapSettingsNotSet },
                { HESCode.AppSettingsNotFound,  Resources.Resource.Exception_AppSettingsNotFound },

                { HESCode.HardwareVaultProfileNotFound,  Resources.Resource.Exception_HardwareVaultProfileNotFound },
                { HESCode.CannotDeleteDefaultProfile,  Resources.Resource.Exception_CannotDeleteDefaultProfile },
                { HESCode.ProfileNameAlreadyInUse,  Resources.Resource.Exception_ProfileNameAlreadyInUse },
                { HESCode.ActivationCodeNotFound,  Resources.Resource.Exception_ActivationCodeNotFound },

                { HESCode.AccountNotFound, Resources.Resource.Exception_AccountNotFound },
                { HESCode.AccountExist, Resources.Resource.Exception_AccountExist },
                { HESCode.IncorrectUrl, Resources.Resource.Exception_IncorrectUrl },
                { HESCode.IncorrectOtp, Resources.Resource.Exception_IncorrectOtp },
                { HESCode.IncorrectPassword, Resources.Resource.Exception_IncorrectPassword },
                { HESCode.IncorrectOldPassword, Resources.Resource.Exception_IncorrectOldPassword },

                { HESCode.TemplateNotFound, Resources.Resource.Exception_TemplateNotFound },
                { HESCode.TemplateExist, Resources.Resource.Exception_TemplateExist },

                { HESCode.SharedAccountNotFound, Resources.Resource.Exception_SharedAccountNotFound },
                { HESCode.SharedAccountExist, Resources.Resource.Exception_SharedAccountExist },
                { HESCode.SecurityKeyNotFound, Resources.Resource.Exception_SecurityKeyNotFound },
                { HESCode.AuthenticatorNotFIDO2, Resources.Resource.Exception_AuthenticatorNotFIDO2 },

                { HESCode.CompanyNameAlreadyInUse, Resources.Resource.Exception_CompanyNameAlreadyInUse },
                { HESCode.CompanyNotFound, Resources.Resource.Exception_CompanyNotFound },
                { HESCode.DepartmentNameAlreadyInUse, Resources.Resource.Exception_DepartmentNameAlreadyInUse },
                { HESCode.DepartmentNotFound, Resources.Resource.Exception_DepartmentNotFound },
                { HESCode.PositionNameAlreadyInUse, Resources.Resource.Exception_PositionNameAlreadyInUse },
                { HESCode.PositionNotFound, Resources.Resource.Exception_PositionNotFound },

                { HESCode.LicenseOrderNotFound, Resources.Resource.Exception_LicenseOrderNotFound },
                { HESCode.LicenseForHardwareVaultNotFound, Resources.Resource.Exception_LicenseForHardwareVaultNotFound },

                { HESCode.ApiKeyEmpty, Resources.Resource.Exception_ApiKeyEmpty },

                { HESCode.DataProtectionNotFinishedPasswordChange, Resources.Resource.Exception_DataProtectionNotFinishedPasswordChange },
                { HESCode.DataProtectionNotActivated, Resources.Resource.Exception_DataProtectionNotActivated },
                { HESCode.DataProtectionNotEnabled, Resources.Resource.Exception_DataProtectionNotEnabled },
                { HESCode.DataProtectionParametersNotFound, Resources.Resource.Exception_DataProtectionParametersNotFound },
                { HESCode.DataProtectionParametersIsEmpty, Resources.Resource.Exception_DataProtectionParametersIsEmpty },
                { HESCode.DataProtectionIsAlreadyActivated, Resources.Resource.Exception_DataProtectionIsAlreadyActivated },
                { HESCode.DataProtectionIsAlreadyEnabled, Resources.Resource.Exception_DataProtectionIsAlreadyEnabled },
                { HESCode.DataProtectionIsBusy, Resources.Resource.Exception_DataProtectionIsBusy },

                { HESCode.TheLDAPServerIsUnavailable, Resources.Resource.Exception_TheLDAPServerIsUnavailable },

                { HESCode.InvalidCertificate, "Invalid certificate." },
                { HESCode.Saml2RelyingPartyNotFound, "Service provider not found." },
                { HESCode.Saml2IssuerAlreadyExist, "Issuer already exist." }
            };
        }

        public static string GetIdentityResultErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(". ", errors.Select(x => x.Description).ToArray());
        }
    }
}