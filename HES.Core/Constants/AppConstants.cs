using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace HES.Core.Constants
{
    public class AppConstants
    {
        // Server
        public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public static string Copyright => $"© {DateTime.Today.Year} {CompanyName}";
        public static string ServerUrl { get; private set; }
        public static string ServerFullName { get; private set; }
        public static string ServerShortName { get; private set; }
        public static string CompanyName { get; private set; }

        public static void SetServerSettings(IConfiguration configuration)
        {
            ServerUrl = configuration["ServerSettings:ServerUrl"];
            ServerFullName = configuration["ServerSettings:ServerFullName"];
            ServerShortName = configuration["ServerSettings:ServerShortName"];
            CompanyName = configuration["ServerSettings:CompanyName"];
        }

        // Hardware Vault default profile id
        public const string DefaulHardwareVaultProfileId = "default";

        // App Settings keys
        public const string Licensing = "licensing";
        public const string Alarm = "alarm";
        public const string Ldap = "domain";
        public const string Email = "email";
        public const string Server = "server";
        public const string Splunk = "splunk";
        public const string Saml2Sp = "saml2sp";

        // License
        public const string LicenseAddress = "https://hls.hideez.com";
    }
}