using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Auth;

public class UserContextMiddleware(RequestDelegate next, IUserContextSetter userContextSetter) {

    public async Task InvokeAsync(HttpContext context) {
        if(context.User == null) {
            await next(context);
            return;
        }

        var user = context.User;

        var tenantId = user.FindFirstValue("tenantId");
        string? userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var isAdmin = user.FindFirst("adm")?.Value.Equals("true") ?? false;
        var globalAdmin = user.FindFirst("gadm")?.Value.Equals("true") ?? false;

        bool hasTenant = Guid.TryParse(tenantId, out Guid tenantIdGuid);
        bool hasUserId = Guid.TryParse(userIdString, out Guid userIdGuid);
        if ( !globalAdmin && (!hasTenant || !hasUserId))
        {
            await next(context);
            return;
        }

        userContextSetter.SetUserContext(new UserContext(
            UserId: userIdGuid,
            TenantId: tenantIdGuid,
            IsAuthenticated: true,
            IsAdmin: isAdmin,
            IsGlobalAdmin: globalAdmin
        ));

        await next(context);
    }
}

public static class UserContextMiddlewareExtensions
{
    public static IApplicationBuilder UseUserContextProvider(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserContextMiddleware>();
    }
}