using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.Driver;
using Graphr.Neo4j.Graphr;
using Graphr.Neo4j.Logging;
using Graphr.Neo4j.QueryExecution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using INeoLogger = Neo4j.Driver.ILogger;

namespace Graphr.Neo4j.DependencyInjection
{
    public static class NeoDependencyInjectionExtensions
    {
        public static IServiceCollection AddNeoGraphr(this IServiceCollection services, IConfiguration configuration)
        {
            var neoDriverConfigurationSettings = new NeoDriverConfigurationSettings();
            configuration.Bind(nameof(NeoDriverConfigurationSettings), neoDriverConfigurationSettings);
            
            services
                .AddSingleton(neoDriverConfigurationSettings)
                .AddSingleton<INeoLogger, NeoLogger>()
                .AddSingleton<IDriverProvider, DriverProvider>()
                .AddScoped<IQueryExecutor, QueryExecutor>()
                .AddScoped<INeoGraphr, NeoGraphr>();

            return services;
        }
    }
}