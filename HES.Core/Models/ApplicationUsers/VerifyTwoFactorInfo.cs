using System.Collections.Generic;

namespace HES.Core.Models.ApplicationUsers
{
    public class VerifyTwoFactorInfo
    {
        public bool IsTwoFactorTokenValid { get; set; }
        public List<string> RecoveryCodes { get; set; }
    }
}