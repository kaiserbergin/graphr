using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Graphr.Neo4j.QueryExecution
{
    public sealed class QueryExecutorWithConfig : IQueryExecutor
    {
        private readonly IQueryExecutor _queryExecutor;

        public QueryExecutorWithConfig(IQueryExecutor queryExecutor, Action<SessionConfigBuilder> sessionConfigBuilder)
        {
            _queryExecutor = queryExecutor;
            queryExecutor.SessionConfigBuilder = sessionConfigBuilder;
        }

        public IQueryExecutor Clone() =>
            new QueryExecutorWithConfig(_queryExecutor, SessionConfigBuilder);

        public Action<SessionConfigBuilder> SessionConfigBuilder
        {
            get => _queryExecutor.SessionConfigBuilder;
            set => _queryExecutor.SessionConfigBuilder = value;
        }

        public async Task<List<IRecord>> ReadAsync(string query, CancellationToken cancellationToken = default) =>
            await _queryExecutor.ReadAsync(query, cancellationToken);


        public async Task<List<IRecord>> ReadAsync(string query, object parameters, CancellationToken cancellationToken = default) =>
            await _queryExecutor.ReadAsync(query, parameters, cancellationToken);

        public async Task<List<IRecord>> ReadAsync(string query, IDictionary<string, object> parameters, CancellationToken cancellationToken = default) =>
            await _queryExecutor.ReadAsync(query, parameters, cancellationToken);

        public async Task<List<IRecord>> ReadAsync(Query query, CancellationToken cancellationToken = default) =>
            await _queryExecutor.ReadAsync(query, cancellationToken);

        public async Task<List<IRecord>> WriteAsync(string query, CancellationToken cancellationToken = default) =>
            await _queryExecutor.WriteAsync(query, cancellationToken);

        public async Task<List<IRecord>> WriteAsync(string query, object parameters, CancellationToken cancellationToken = default) =>
            await _queryExecutor.WriteAsync(query, parameters, cancellationToken);

        public async Task<List<IRecord>> WriteAsync(string query, IDictionary<string, object> parameters, CancellationToken cancellationToken = default) =>
            await _queryExecutor.WriteAsync(query, parameters, cancellationToken);

        public async Task<List<IRecord>> WriteAsync(Query query, CancellationToken cancellationToken = default) =>
            await _queryExecutor.WriteAsync(query, cancellationToken);
    }
}