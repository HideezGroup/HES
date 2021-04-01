using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace HES.Web.Providers
{
    public class RegisterSecurityKeyTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public RegisterSecurityKeyTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<RegisterSecurityKeyTokenProviderOptions> options, ILogger<RegisterSecurityKeyTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {

        }
    }

    public class RegisterSecurityKeyTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public RegisterSecurityKeyTokenProviderOptions()
        {
            Name = "SecurityKeyDataProtectorTokenProvider";
            TokenLifespan = TimeSpan.FromDays(3);
        }
    }

    public class RegisterSecurityKeyTokenConstants
    {
        public const string TokenName = "RegisterSecurityKey";
        public const string TokenPurpose = "RegisterSecurityKeyPurpose";
    }
}