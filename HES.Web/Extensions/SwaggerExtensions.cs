using HES.Web.Middleware;
using Microsoft.AspNetCore.Builder;

namespace HES.Web.Extensions
{
    public static class SwaggerExtensions
    {
        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerAuthorizedMiddleware>();
        }
    }
}