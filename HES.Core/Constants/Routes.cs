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
    }
}