using System.Linq;
using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.DependencyInjection;
using Graphr.Neo4j.Driver;
using Graphr.Neo4j.Graphr;
using Graphr.Neo4j.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using INeoLogger = Neo4j.Driver.ILogger;

namespace Graphr.Tests.DependencyInjection
{
    public class NeoDependencyInjectionExtensionsTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void AddNeoGraphr_NormalUsage_RegistersDependencies()
        {
            // Arrange
             var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();

            // Act
            serviceCollection.AddNeoGraphr(configuration);

            // Assert;
            Assert.Single(serviceCollection.Where(x => x.Lifetime == ServiceLifetime.Singleton && x.ServiceType == typeof(NeoDriverConfigurationSettings)));
            Assert.Single(serviceCollection.Where(x => x.Lifetime == ServiceLifetime.Singleton && x.ServiceType == typeof(INeoLogger)));
            Assert.Single(serviceCollection.Where(x => x.Lifetime == ServiceLifetime.Singleton && x.ServiceType == typeof(IDriverProvider)));
            Assert.Single(serviceCollection.Where(x => x.Lifetime == ServiceLifetime.Transient && x.ServiceType == typeof(IQueryExecutor)));
            Assert.Single(serviceCollection.Where(x => x.Lifetime == ServiceLifetime.Transient && x.ServiceType == typeof(INeoGraphr)));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AddNeoGraphr_NormalUsage_RegistersIntegratedDependencies()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.integrationtests.json", false)
                .AddEnvironmentVariables()
                .Build();

            // Act
            serviceCollection
                .AddLogging()
                .AddNeoGraphr(configuration);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            serviceProvider.GetRequiredService<NeoDriverConfigurationSettings>();
            serviceProvider.GetRequiredService<INeoLogger>();
            serviceProvider.GetRequiredService<IDriverProvider>();
            serviceProvider.GetRequiredService<IQueryExecutor>();
            serviceProvider.GetRequiredService<INeoGraphr>();
        }
    }
}