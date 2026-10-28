namespace MMSA.RateLimiting;

public static class RateLimitPolicies
{
    public const string Auth = "auth";

    public static string GetClientKey(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

}