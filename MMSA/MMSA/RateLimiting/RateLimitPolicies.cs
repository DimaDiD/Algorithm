using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace MMSA.RateLimiting;

public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string ExerciseCheck = "exercise-check";
    public const string ClientIdHeader = "X-Client-Id";

    public static string GetClientKey(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        if (Guid.TryParse(httpContext.Request.Headers[ClientIdHeader], out var clientId) && clientId != Guid.Empty)
            return $"client:{clientId:N}";

        return GetIpKey(httpContext);
    }

    public static string GetIpKey(HttpContext httpContext) =>
        $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}
