namespace HES.Core.Constants
{
    public class Routes
    {
        public const string Login = "/login";
        public const string Logout = "/logout";
        public const string LoginWith2Fa = "/Identity/Account/LoginWith2fa";
        public const string LoginWithRecoveryCode = "/Identity/Account/LoginWithRecoveryCode";
        public const string Lockout = "/Identity/Account/Lockout";
        public const string ForgotPassword = "/Identity/Account/ForgotPassword";
        public const string ForgotPasswordConfirmation = "/Identity/Account/ForgotPasswordConfirmation";
        public const string ResetPassword = "/Identity/Account/ResetPassword";
        public const string ResetPasswordConfirmation = "/Identity/Account/ResetPasswordConfirmation";

        public const string Profile = "/profile";

        public const string SingleSignOn = "/sso";
        public const string SingleLogOut = "/slo";

        public const string Dashboard = "/";
    }
}