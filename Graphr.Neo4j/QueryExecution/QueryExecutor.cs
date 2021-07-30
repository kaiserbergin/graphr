using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Graphr.Neo4j.Driver;
using Neo4j.Driver;

namespace Graphr.Neo4j.QueryExecution
{
    public class QueryExecutor : IQueryExecutor
    {
        private readonly IDriver _driver;

        public QueryExecutor(IDriverProvider driverProvider)
        {
            _driver = driverProvider.Driver;
        }

        private async Task<List<IRecord>> ReadAsync(Func<IAsyncTransaction, Task<IResultCursor>> runAsyncCommand)
        {
            var records = new List<IRecord>();
            await using var session = _driver.AsyncSession();
            
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var reader = await runAsyncCommand(tx);

                    while (await reader.FetchAsync())
                    {
                        records.Add(reader.Current);
                    }
                });
            }
            finally
            {
                await session.CloseAsync();
            }
            
            return records;
        }

        private async Task<List<IRecord>> WriteAsync(Func<IAsyncTransaction, Task<IResultCursor>> runAsyncCommand)
        {
            var records = new List<IRecord>();
            await using var session = _driver.AsyncSession();
            
            try
            {
                await session.WriteTransactionAsync(async tx =>
                {
                    var reader = await runAsyncCommand(tx);

                    while (await reader.FetchAsync())
                    {
                        records.Add(reader.Current);
                    }
                });
            }
            finally
            {
                await session.CloseAsync();
            }
            
            return records;
        }

        public async Task<List<IRecord>> ReadAsync(string query)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await ReadAsync(RunAsyncCommand);
        }

        public async Task<List<IRecord>> ReadAsync(string query, object parameters)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await ReadAsync(RunAsyncCommand);
        }

        public async Task<List<IRecord>> ReadAsync(string query, IDictionary<string, object> parameters)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await ReadAsync(RunAsyncCommand);
        }

        public async Task<List<IRecord>> ReadAsync(Query query)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await ReadAsync(RunAsyncCommand);
        }
        
        public async Task<List<IRecord>> WriteAsync(string query)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await WriteAsync(RunAsyncCommand);
        }

        public async Task<List<IRecord>> WriteAsync(string query, object parameters)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await WriteAsync(RunAsyncCommand);
        }

        public async Task<List<IRecord>> WriteAsync(string query, IDictionary<string, object> parameters)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query, parameters);

            return await WriteAsync(RunAsyncCommand);
        }

        public async Task<List<IRecord>> WriteAsync(Query query)
        {
            async Task<IResultCursor> RunAsyncCommand(IAsyncTransaction tx) => await tx.RunAsync(query);

            return await WriteAsync(RunAsyncCommand);
        }
    }
}