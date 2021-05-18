using Microsoft.Extensions.Configuration;

namespace HES.Core.Helpers
{
    public class SamlHelper
    {
        public static bool IsEnabled(IConfiguration configuration)
        {
            if (!string.IsNullOrWhiteSpace(configuration.GetValue<string>("Saml2:SigningCertificatePassword")))
            {
                return true;
            }
            return false;
        }
    }
}