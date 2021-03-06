﻿using System;
using System.Reflection;

namespace HES.Core.Constants
{
    public class ServerConstants
    {
        // Server Version
        public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public static string Copyright => $"© {DateTime.Today.Year} Hideez Group Inc.";

        public const string ServerName = "Hideez Enterprise Server";

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

        // Cookie
        public const string SetCookieHeader = "Set-Cookie";
    }
}