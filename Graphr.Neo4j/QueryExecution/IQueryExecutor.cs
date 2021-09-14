using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Graphr.Neo4j.QueryExecution
{
    public interface IQueryExecutor
    {
        Task<List<IRecord>> ReadAsync(string query, CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(string query, object parameters, CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(string query, IDictionary<string, object> parameters, CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(Query query, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(string query, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(string query, object parameters, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(string query, IDictionary<string, object> parameters, CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(Query query, CancellationToken cancellationToken = default);
    }
}