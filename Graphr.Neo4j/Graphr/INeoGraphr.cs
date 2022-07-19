using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Graphr.Neo4j.Graphr
{
    public interface INeoGraphr
    {
        Task<List<T>> ReadAsAsync<T>(
            string query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> ReadAsAsync<T>(
            string query, 
            object parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> ReadAsAsync<T>(
            string query, 
            IDictionary<string, object> parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> ReadAsAsync<T>(
            Query query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default) where T : class, new();
        Task<List<T>> WriteAsAsync<T>(
            string query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> WriteAsAsync<T>(
            string query, 
            object parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default)
            where T : class, new();

        Task<List<T>> WriteAsAsync<T>(
            string query, 
            IDictionary<string, object> parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default) where T : class, new();

        Task<List<T>> WriteAsAsync<T>(
            Query query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default) where T : class, new();
        Task<List<IRecord>> ReadAsync(
            string query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default);
        Task<List<IRecord>> ReadAsync(
            string query, 
            object parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> ReadAsync(
            string query, 
            IDictionary<string, object> parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> ReadAsync(
            Query query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(
            string query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default);
        Task<List<IRecord>> WriteAsync(
            string query, 
            object parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> WriteAsync(
            string query, 
            IDictionary<string, object> parameters, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null,
            CancellationToken cancellationToken = default);

        Task<List<IRecord>> WriteAsync(
            Query query, 
            Action<SessionConfigBuilder>? sessionConfigurationAction = null, 
            CancellationToken cancellationToken = default);
    }
}