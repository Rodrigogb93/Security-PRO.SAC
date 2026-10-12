using System.Security.Claims;

namespace SecurityPRO.ERP.Web.Middleware;

public class AuditoriaMiddleware
{
    private readonly RequestDelegate _next;
    public AuditoriaMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Items["IpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        context.Items["IdUsuario"] = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _next(context);
    }
}