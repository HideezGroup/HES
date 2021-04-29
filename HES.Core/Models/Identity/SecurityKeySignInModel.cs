using Fido2NetLib;

namespace HES.Core.Models.Identity
{
    public class SecurityKeySignInModel
    {
        public AuthenticatorAssertionRawResponse AuthenticatorAssertionRawResponse { get; set; }
        public bool RememberMe { get; set; }
    }
}