using System;
using System.Linq;
using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.DependencyInjection;
using Graphr.Neo4j.Driver;
using Graphr.Neo4j.Graphr;
using Graphr.Neo4j.Queries;
using Graphr.Tests.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using INeoLogger = Neo4j.Driver.ILogger;

namespace Graphr.Tests.DependencyInjection
{
    [Collection("ServiceProvider")]
    public class NeoDependencyInjectionExtensionsTests
    {
        private readonly IServiceProvider _serviceProvider;

        public NeoDependencyInjectionExtensionsTests(ServiceProviderFixture serviceProviderFixture)
        {
            _serviceProvider = serviceProviderFixture.ServiceProvider;
        }

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
            // Assert
            _serviceProvider.GetRequiredService<NeoDriverConfigurationSettings>();
            _serviceProvider.GetRequiredService<INeoLogger>();
            _serviceProvider.GetRequiredService<IDriverProvider>();
            _serviceProvider.GetRequiredService<IQueryExecutor>();
            _serviceProvider.GetRequiredService<INeoGraphr>();
        }
    }
}