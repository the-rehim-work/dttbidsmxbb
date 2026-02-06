using dttbidsmxbb.Models;
using dttbidsmxbb.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace dttbidsmxbb.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ILogService logService, UserManager<AppUser> userManager)
        {
            await next(context);

            if (context.Request.Headers.XRequestedWith == "XMLHttpRequest")
                return;

            if (context.Request.Path.StartsWithSegments("/lib") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/assets"))
                return;

            if (context.Request.Method != "GET")
                return;

            if (context.User?.IsInRole("Admin") == true)
                return;

            int? userId = null;
            string? userFullName = null;

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var idClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idClaim, out var uid))
                {
                    userId = uid;
                    var user = await userManager.FindByIdAsync(uid.ToString());
                    userFullName = user?.FullName ?? context.User.Identity?.Name;
                }
            }

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            await logService.LogEventAsync(
                userId,
                userFullName,
                context.Request.Method,
                context.Request.Path.ToString(),
                context.Response.StatusCode,
                ip);
        }
    }
}