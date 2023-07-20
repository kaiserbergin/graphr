using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Neo4j;
using Xunit;

namespace Graphr.Tests.Fixtures
{
    public sealed class ServiceProviderFixture : IAsyncLifetime
    {
        internal readonly Neo4jContainer _neo4jContainer =
            new Neo4jBuilder()
                .WithImage("neo4j:enterprise")
                .WithEnvironment("NEO4JLABS_PLUGINS", "[\"apoc\"]")
                .WithEnvironment("NEO4J_ACCEPT_LICENSE_AGREEMENT", "yes")
                .WithEnvironment("NEO4J_AUTH", $"neo4j/connect")
                .Build();

        internal IServiceProvider ServiceProvider;

        public async Task InitializeAsync()
        {
            if (!IsLocalNeoInstance())
                await _neo4jContainer.StartAsync();

            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.integrationtests.json", false)
                .AddEnvironmentVariables();

            if (!IsLocalNeoInstance())
                configuration
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>(
                            $"{nameof(NeoDriverConfigurationSettings)}:{nameof(NeoDriverConfigurationSettings.Url)}",
                            _neo4jContainer.GetConnectionString())
                    });

            serviceCollection
                .AddLogging()
                .AddNeoGraphr(configuration.Build());

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private bool IsLocalNeoInstance()
        {
            using var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect("127.0.0.1", 7687);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task DisposeAsync()
        {
            if (!IsLocalNeoInstance())
                await _neo4jContainer.StopAsync();
        }
    }
}