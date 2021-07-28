using System;
using Graphr.Neo4j.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Graphr.Tests.Fixtures
{
    public class ServiceProviderFixture
    {
        public IServiceProvider ServiceProvider;

        public ServiceProviderFixture()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.integrationtests.json", false)
                .AddEnvironmentVariables()
                .Build();

            serviceCollection
                .AddLogging()
                .AddNeoGraphr(configuration);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}