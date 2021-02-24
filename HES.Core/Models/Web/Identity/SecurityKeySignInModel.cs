using Fido2NetLib;

namespace HES.Core.Models.Web.Identity
{
    public class SecurityKeySignInModel
    {
        public AuthenticatorAssertionRawResponse AuthenticatorAssertionRawResponse { get; set; }
        public bool RememberMe { get; set; }
    }
}