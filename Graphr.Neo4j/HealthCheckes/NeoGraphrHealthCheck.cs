using System;
using System.Threading;
using System.Threading.Tasks;
using Graphr.Neo4j.Driver;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Graphr.Neo4j.HealthCheckes
{
    public class NeoGraphrHealthCheck : IHealthCheck
    {
            private readonly IDriverProvider _driverProvider;
            private readonly ILogger<NeoGraphrHealthCheck> _logger;

            public NeoGraphrHealthCheck(IDriverProvider driverProvider, ILogger<NeoGraphrHealthCheck> logger)
            {
                _driverProvider = driverProvider ?? throw new ArgumentNullException(nameof(driverProvider));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
            {
                try
                {
                    await _driverProvider.Driver.VerifyConnectivityAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Neo4j connection not established");
                
                    return HealthCheckResult.Degraded();
                }
                return HealthCheckResult.Healthy();
            }
        }
}