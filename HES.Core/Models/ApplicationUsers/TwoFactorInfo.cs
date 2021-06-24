namespace HES.Core.Models.ApplicationUsers
{
    public class TwoFactorInfo
    {
        public bool Is2faEnabled { get; set; }
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public bool IsMachineRemembered { get; set; }
    }
}