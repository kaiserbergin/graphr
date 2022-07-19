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
        private readonly IDriver _driver;
        private readonly NeoDriverConfigurationSettings _settings;
        private readonly TimeSpan _timeout;

        public QueryExecutor(IDriverProvider driverProvider, NeoDriverConfigurationSettings neoDriverConfigurationSettings)
        {
            _driver = driverProvider.Driver ?? throw new ArgumentNullException(nameof(driverProvider));
            _settings = neoDriverConfigurationSettings ?? throw new ArgumentNullException(nameof(neoDriverConfigurationSettings));
            _timeout = TimeSpan.FromMilliseconds(neoDriverConfigurationSettings.QueryTimeoutInMs!.Value);
        }

        /// <summary>
        /// Reads records from neo4j asynchronously
        /// </summary>
        /// <param name="runAsyncCommand"></param>
        /// <param name="sessionConfigurationAction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<List<IRecord>> ReadAsync(
            Func<IAsyncTransaction, Task<IResultCursor>> runAsyncCommand,
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            if (_settings.VerifyConnectivity)
                await _driver.VerifyConnectivityAsync();

            await using var session = _driver.AsyncSession(sessionConfigurationAction ?? GetSessionConfiguration());

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
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            if (_settings.VerifyConnectivity)
                await _driver.VerifyConnectivityAsync();

            await using var session = _driver.AsyncSession(sessionConfigurationAction ?? GetSessionConfiguration());

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

        private Action<SessionConfigBuilder> GetSessionConfiguration()
        {
            if (_settings.DatabaseName is not null)
                return builder => builder.WithDatabase(_settings.DatabaseName);

            return _ => { };
        }

        public async Task<List<IRecord>> ReadAsync(
            string query,
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await ReadAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }

        public async Task<List<IRecord>> ReadAsync(
            string query, 
            object parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await ReadAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }

        public async Task<List<IRecord>> ReadAsync(
            string query, 
            IDictionary<string, object> parameters,
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await ReadAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }

        public async Task<List<IRecord>> ReadAsync(
            Query query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await ReadAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            string query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await WriteAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            string query, 
            object parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await WriteAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            string query, 
            IDictionary<string, object> parameters,
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await WriteAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }

        public async Task<List<IRecord>> WriteAsync(
            Query query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default
        )
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await WriteAsync(RunAsyncCommand, sessionConfigurationAction, cancellationToken);
        }
    }
}