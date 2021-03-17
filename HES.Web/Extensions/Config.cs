using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using Rsk.Saml;
using Rsk.Saml.Models;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace HES.Web.Extensions
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("api1", "My API #1")
            };
        }

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            var ClientId = configuration.GetValue<string>("SAML2P:ServiceProvider:EntityId");
            var ClientName = configuration.GetValue<string>("SAML2P:ClientName");

            return new[]
            {
                new Client
                {
                    ClientId = ClientId,
                    ClientName = ClientName,
                    ProtocolType = IdentityServerConstants.ProtocolTypes.Saml2p,
                    AllowedScopes = {"openid", "profile"}
                },
            };
        }
        public static X509Certificate2 GetCertificate(IConfiguration configuration)
        {
            var path = configuration.GetValue<string>("SAML2P:SigningCredential");
            var password = configuration.GetValue<string>("SAML2P:SigningCredentialPassword");

            return new X509Certificate2(path, password);
        }

        public static IEnumerable<ServiceProvider> GetServiceProviders(IConfiguration configuration)
        {
            var EntityId = configuration.GetValue<string>("SAML2P:ServiceProvider:EntityId");
            var AssertionConsumerServices = configuration.GetValue<string>("SAML2P:ServiceProvider:AssertionConsumerServices");
            var SingleLogoutServices = configuration.GetValue<string>("SAML2P:ServiceProvider:SingleLogoutServices");
            var SigningCertificates = configuration.GetValue<string>("SAML2P:ServiceProvider:SigningCertificates");

            return new[]
            {
                new ServiceProvider
                {
                    EntityId = EntityId,
                    AssertionConsumerServices = {new Service(SamlConstants.BindingTypes.HttpPost, AssertionConsumerServices) },
                    SingleLogoutServices = {new Service(SamlConstants.BindingTypes.HttpRedirect, SingleLogoutServices) },
                    SigningCertificates = { new X509Certificate2(Encoding.ASCII.GetBytes(SigningCertificates)) }
                },
            };
        }
    }
}
