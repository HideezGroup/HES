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
        public const string ForgotPassword = "/forgot-password";
        public const string ForgotPasswordConfirmation = "/forgot-password-confirmation";
        public const string ResetPassword = "/reset-password";   

        public const string Profile = "/profile";

        public const string SingleSignOn = "/sso";
        public const string SingleLogOut = "/slo";
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

    }
}