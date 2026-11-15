﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using MMSA.DAL.Entities;

namespace MMSA.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly AlgorithmDataContext _dataContext;

    public DatabaseHealthCheck(AlgorithmDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await _dataContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("Database is reachable.")
            : HealthCheckResult.Unhealthy("Database is not reachable.");
    }
}
