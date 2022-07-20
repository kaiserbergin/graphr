using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.Driver;
using Neo4j.Driver;

namespace Graphr.Neo4j.QueryExecution
{
    internal class QueryExecutor : IQueryExecutor
    {
        private readonly IDriverProvider _driverProvider;
        private readonly IDriver _driver;
        private readonly NeoDriverConfigurationSettings _settings;
        private readonly TimeSpan _timeout;
        
        public Action<SessionConfigBuilder> SessionConfigBuilder { get; set; }

        public QueryExecutor(IDriverProvider driverProvider, NeoDriverConfigurationSettings neoDriverConfigurationSettings)
        {
            _driverProvider = driverProvider ?? throw new ArgumentNullException(nameof(driverProvider));
            _driver = driverProvider.Driver ?? throw new ArgumentNullException(nameof(driverProvider.Driver));
            _settings = neoDriverConfigurationSettings ?? throw new ArgumentNullException(nameof(neoDriverConfigurationSettings));
            _timeout = TimeSpan.FromMilliseconds(neoDriverConfigurationSettings.QueryTimeoutInMs!.Value);

            if (neoDriverConfigurationSettings.DatabaseName is not null)
                SessionConfigBuilder = config => config.WithDatabase(neoDriverConfigurationSettings.DatabaseName);
            else
                SessionConfigBuilder = _ => { };
        }

        public IQueryExecutor Clone() => new QueryExecutor(_driverProvider, _settings);

        private async Task<List<IRecord>> ReadAsync(
            Func<IAsyncTransaction, Task<IResultCursor>> runAsyncCommand,
            CancellationToken cancellationToken = default
        )
        {
            if (_settings.VerifyConnectivity)
                await _driver.VerifyConnectivityAsync();

            await using var session = _driver.AsyncSession(SessionConfigBuilder);

            var records = new List<IRecord>();

            try
            {
                await session.ReadTransactionAsync(async tx =>
                    {
                        var reader = await runAsyncCommand(tx);

                        while (await reader.FetchAsync())
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            records.Add(reader.Current);
                        }
                    },
                    x => x.WithTimeout(_timeout));
            }
            finally
            {
                await session.CloseAsync();
            }

            return records;
        }

        private async Task<List<IRecord>> WriteAsync(
            Func<IAsyncTransaction, Task<IResultCursor>> runAsyncCommand,
            CancellationToken cancellationToken = default
        )
        {
            if (_settings.VerifyConnectivity)
                await _driver.VerifyConnectivityAsync();

            await using var session = _driver.AsyncSession(SessionConfigBuilder);

            var records = new List<IRecord>();

            try
            {
                await session.WriteTransactionAsync(async tx =>
                    {
                        var reader = await runAsyncCommand(tx);

                        while (await reader.FetchAsync())
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            records.Add(reader.Current);
                        }
                    },
                    x => x.WithTimeout(_timeout));
            }
            finally
            {
                await session.CloseAsync();
            }

            return records;
        }

        public async Task<List<IRecord>> ReadAsync(
            string query,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await ReadAsync(RunAsyncCommand, cancellationToken);
        }

        public async Task<List<IRecord>> ReadAsync(
            string query, 
            object parameters, 
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await ReadAsync(RunAsyncCommand, cancellationToken);
        }

        public async Task<List<IRecord>> ReadAsync(
            string query, 
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await ReadAsync(RunAsyncCommand, cancellationToken);
        }

        public async Task<List<IRecord>> ReadAsync(
            Query query, 
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await ReadAsync(RunAsyncCommand, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            string query, 
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await WriteAsync(RunAsyncCommand, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            string query, 
            object parameters, 
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await WriteAsync(RunAsyncCommand, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            string query, 
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await WriteAsync(RunAsyncCommand, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            Query query, 
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await WriteAsync(RunAsyncCommand, cancellationToken);
        }
    }
}