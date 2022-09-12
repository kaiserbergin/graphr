using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Graphr.Tests.Fixtures
{
    public sealed class ServiceProviderFixture : IAsyncLifetime
    {
        internal readonly TestcontainersContainer _neo4j =
            new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("neo4j:latest")
                .WithEnvironment("NEO4J_USERNAME", "neo4j")
                .WithEnvironment("NEO4J_PASSWORD", "connect")
                .WithEnvironment("NEO4J_AUTH", "neo4j/connect")
                .WithEnvironment("NEO4J_ACCEPT_LICENSE_AGREEMENT", "yes")
                .WithEnvironment("NEO4JLABS_PLUGINS", "[\"apoc\"]")
                .WithPortBinding(7474, true)
                .WithPortBinding(7473, true)
                .WithPortBinding(7687, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(7687))
                .Build();

        internal IServiceProvider ServiceProvider;

        public async Task InitializeAsync()
        {
            await _neo4j.StartAsync();
            
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.integrationtests.json", false)
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(
                        $"{nameof(NeoDriverConfigurationSettings)}:{nameof(NeoDriverConfigurationSettings.Url)}",
                        $"bolt://localhost:{_neo4j.GetMappedPublicPort(7687)}")
                })
                .AddEnvironmentVariables()
                .Build();

            serviceCollection
                .AddLogging()
                .AddNeoGraphr(configuration);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public async Task DisposeAsync() =>
            await _neo4j.StopAsync();
    }
}