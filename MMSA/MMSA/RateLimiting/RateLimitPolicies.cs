namespace MMSA.RateLimiting;

public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string ExerciseCheck = "exercise-check";

    public static string GetClientKey(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

}