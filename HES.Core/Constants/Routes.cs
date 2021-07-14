namespace HES.Core.Constants
{
    public class Routes
    {
        public const string Login = "/login";
        public const string Logout = "/logout";
        public const string Invite = "/invite";
        public const string LoginWith2Fa = "/login-with-2fa";
        public const string LoginWithRecoveryCode = "/login-with-recovery-code";
        public const string Lockout = "/lockout";
        public const string AccessDenied = "/access-denied";
        public const string ResetPassword = "/reset-password";
        public const string ConfirmEmailChange = "/confirm-email-change";

        public const string Profile = "/profile";

        public const string SingleSignOn = "/sso";
        public const string SingleLogOut = "/slo";
        public const string SSO = "/saml/login";
        public const string RegisterSecurityKey = "/register-security-key";

        public const string Dashboard = "/";
        public const string Alarm = "/alarm";
        public const string WorkstationEvents = "/audit/workstation-events";
        public const string WorkstationSessions = "/audit/workstation-sessions";
        public const string WorkstationSummaries = "/audit/workstation-summaries";
        public const string Employees = "/employees";
        public const string EmployeesDetails = "/employees/details/";
        public const string Groups = "/groups";
        public const string GroupsDetails = "/groups/details/";
        public const string HardwareVaults = "/hardware-vaults";
        public const string SharedAccounts = "/shared-accounts";
        public const string SoftwareVaults = "/software-vaults";
        public const string Templates = "/templates";
        public const string Workstations = "/workstations";
        public const string WorkstationsDetails = "/workstations/details/";
        public const string Administrators = "/settings/administrators";
        public const string DataProtection = "/settings/data-protection";
        public const string DataProtectionActivate = "/settings/data-protection/activate";
        public const string HardwareVaultAccessProfile = "/settings/hardware-vault-access-profile";
        public const string LicenseOrders = "/settings/license-orders";
        public const string OrgStructure = "/settings/org-structure";
        public const string Parameters = "/settings/parameters";
        public const string Update = "/update";

        public const string SamlMetadata = "/saml/metadata";
        public const string SamlDownloadMetadata = "/saml/metadata?download=true";
        public const string SamlDownloadCert = "/saml/cert";
        
        public const string ApiLoginWithPassword = "/api/Identity/LoginWithPassword";
        public const string ApiLoginWithFido2 = "/api/Identity/LoginWithFido2";
        public const string ApiLogout = "/api/Identity/Logout";
        public const string ApiUpdateProfileInfo = "/api/Identity/UpdateProfileInfo";
        public const string ApiUpdateAccountPassword = "/api/Identity/UpdateAccountPassword";

        public const string ApiGetTwoFactorInfo = "/api/Identity/GetTwoFactorInfo";
        public const string ApiGenerateNewTwoFactorRecoveryCodes = "/api/Identity/GenerateNewTwoFactorRecoveryCodes";
        public const string ApiLoadSharedKeyAndQrCodeUri = "/api/Identity/LoadSharedKeyAndQrCodeUri";
        public const string ApiVerifyTwoFactor = "/api/Identity/VerifyTwoFactor";
        public const string ApiResetAuthenticatorKey = "/api/Identity/ResetAuthenticatorKey";
        public const string ApiDisableTwoFactor = "/api/Identity/DisableTwoFactor";
        public const string ForgetTwoFactorClient = "/api/Identity/ForgetTwoFactorClient";

        public const string ApiDeletePersonalData = "/api/Identity/DeletePersonalData";
    }
}