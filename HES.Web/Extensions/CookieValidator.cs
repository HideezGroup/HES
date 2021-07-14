using HES.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HES.Web.Extensions
{
    public static class CookieValidator
    {
        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (!await ValidateCookieAsync(context))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            }
        }

        private static async Task<bool> ValidateCookieAsync(CookieValidatePrincipalContext context)
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var optionsAccessor = context.HttpContext.RequestServices.GetRequiredService<IOptions<IdentityOptions>>();

            var user = await userManager.GetUserAsync(context.Principal);
            if (user == null)
            {
                return false;
            }

            var principalStamp = context.Principal.FindFirstValue(optionsAccessor.Value.ClaimsIdentity.SecurityStampClaimType);
            var userStamp = await userManager.GetSecurityStampAsync(user);
            return principalStamp == userStamp;
        }
    }
}