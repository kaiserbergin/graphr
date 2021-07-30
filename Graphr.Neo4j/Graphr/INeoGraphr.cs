using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Graphr.Neo4j.Graphr
{
    public interface INeoGraphr
    {
        Task<List<T>> ReadAsAsync<T>(string query) where T : class, new();
        Task<List<T>> ReadAsAsync<T>(string query, object parameters) where T : class, new();
        Task<List<T>> ReadAsAsync<T>(string query, IDictionary<string, object> parameters) where T : class, new();
        Task<List<T>> ReadAsAsync<T>(Query query) where T : class, new();
        List<T> Translate<T>(List<IRecord> records) where T : class, new();
        Task<List<T>> WriteAsAsync<T>(string query) where T : class, new();
        Task<List<T>> WriteAsAsync<T>(string query, object parameters) where T : class, new();
        Task<List<T>> WriteAsAsync<T>(string query, IDictionary<string, object> parameters) where T : class, new();
        Task<List<T>> WriteAsAsync<T>(Query query) where T : class, new();
        Task WriteAsync(Query query);
        Task WriteAsync(string query);
        Task WriteAsync(string query, object parameters);
        Task WriteAsync(string query, Dictionary<string, object> parameters);
    }
}