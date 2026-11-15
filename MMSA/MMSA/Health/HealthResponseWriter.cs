﻿using Microsoft.Extensions.Diagnostics.HealthChecks;



namespace MMSA.Health;



public static class HealthResponseWriter

{

    public static Task WriteAsync(HttpContext context, HealthReport report)

    {

        context.Response.Headers.CacheControl = "no-store";



        return context.Response.WriteAsJsonAsync(new

        {

            status = report.Status.ToString(),

            version = AppVersion.Value,

            commit = AppVersion.Commit,

            durationMs = (int)report.TotalDuration.TotalMilliseconds,

            checks = report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString())

        });

    }

}

