using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HES.Web.Middleware
{
    public class HeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public HeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                context.Response.Headers.Add("X-Frame-Options", "DENY");
            }

            if (!context.Response.Headers.ContainsKey("X-Xss-Protection"))
            {
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
            }

            if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            }

            if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
            {
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");
            }

            if (!context.Response.Headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
            {
                context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
            }

            if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
            {
                context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'none'");
            }

            if (!context.Response.Headers.ContainsKey("Permissions-Policy"))
            {
                context.Response.Headers.Add("Permissions-Policy", "clipboard-write=*");
            }

            await _next.Invoke(context);
        }
    }
}