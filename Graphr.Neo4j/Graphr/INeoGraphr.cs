using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Graphr.Neo4j.Graphr
{
    public interface INeoGraphr
    {
        INeoGraphr WithSessionConfig(Action<SessionConfigBuilder> builder);
        
        Task<List<T>> ReadAsAsync<T>(
            string query, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> ReadAsAsync<T>(
            string query, 
            object parameters, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> ReadAsAsync<T>(
            string query, 
            IDictionary<string, object> parameters, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> ReadAsAsync<T>(
            Query query, 
            CancellationToken cancellationToken = default) where T : class, new();
        Task<List<T>> WriteAsAsync<T>(
            string query, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> WriteAsAsync<T>(
            string query, 
            object parameters, 
            CancellationToken cancellationToken = default)
            where T : class, new();

        Task<List<T>> WriteAsAsync<T>(
            string query, 
            IDictionary<string, object> parameters, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> WriteAsAsync<T>(
            Query query, 
            CancellationToken cancellationToken = default) where T : class, new();
        Task<List<IRecord>> ReadAsync(
            string query, 
            CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(
            string query, 
            object parameters, 
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> ReadAsync(
            string query, 
            IDictionary<string, object> parameters, 
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> ReadAsync(
            Query query, 
            CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(
            string query, 
            CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(
            string query, 
            object parameters, 
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> WriteAsync(
            string query, 
            IDictionary<string, object> parameters, 
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> WriteAsync(
            Query query, 
            CancellationToken cancellationToken = default);
    }
}