using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Graphr.Neo4j.QueryExecution
{
    public interface IQueryExecutor
    {
        Task<List<IRecord>> ReadAsync(string query, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(string query,  object parameters, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(string query, IDictionary<string, object> parameters, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(Query query, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(string query, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(string query, object parameters, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(string query, IDictionary<string, object> parameters, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(Query query, Action<SessionConfigBuilder>? sessionConfigurationAction = null, CancellationToken cancellationToken = default);
    }
}