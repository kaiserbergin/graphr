using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.Driver;
using Graphr.Neo4j.Graphr;
using Graphr.Neo4j.HealthCheckes;
using Graphr.Neo4j.Logging;
using Graphr.Neo4j.QueryExecution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using INeoLogger = Neo4j.Driver.ILogger;

namespace Graphr.Neo4j.DependencyInjection
{
    public static class NeoDependencyInjectionExtensions
    {
        public static IServiceCollection AddNeoGraphr(this IServiceCollection services, IConfiguration configuration)
        {
            var neoDriverConfigurationSettings = new NeoDriverConfigurationSettings();
            configuration.Bind(nameof(NeoDriverConfigurationSettings), neoDriverConfigurationSettings);

            services.TryAddSingleton(neoDriverConfigurationSettings);
            services.TryAddSingleton<INeoLogger, NeoLogger>();
            services.TryAddSingleton<IDriverProvider, DriverProvider>();
            services.TryAddScoped<IQueryExecutor, QueryExecutor>();
            services.TryAddScoped<INeoGraphr, NeoGraphr>();

            if (neoDriverConfigurationSettings.IncludeHealthChecks)
            {
                services.AddHealthChecks().AddCheck<NeoGraphrHealthCheck>(neoDriverConfigurationSettings.HealthCheckName);
            }

            return services;
        }
    }
}