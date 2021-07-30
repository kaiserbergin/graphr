using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Graphr.Neo4j.QueryExecution
{
    public interface IQueryExecutor
    {
        Task<List<IRecord>> ReadAsync(string query);
        Task<List<IRecord>> ReadAsync(string query, object parameters);
        Task<List<IRecord>> ReadAsync(string query, IDictionary<string, object> parameters);
        Task<List<IRecord>> ReadAsync(Query query);
        Task<List<IRecord>> WriteAsync(string query);
        Task<List<IRecord>> WriteAsync(string query, object parameters);
        Task<List<IRecord>> WriteAsync(string query, IDictionary<string, object> parameters);
        Task<List<IRecord>> WriteAsync(Query query);
    }
}