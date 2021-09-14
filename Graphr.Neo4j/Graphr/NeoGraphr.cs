using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Graphr.Neo4j.QueryExecution;
using Graphr.Neo4j.Translator;
using Neo4j.Driver;

namespace Graphr.Neo4j.Graphr
{
    public class NeoGraphr : INeoGraphr
    {
        private readonly IQueryExecutor _queryExecutor;

        public NeoGraphr(IQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        }

        public async Task<List<T>> ReadAsAsync<T>(string query, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(string query, object parameters, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, parameters, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(string query, IDictionary<string, object> parameters, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, parameters, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(Query query, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query, object parameters, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, parameters, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query, IDictionary<string, object> parameters, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, parameters, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(Query query, CancellationToken cancellationToken) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, cancellationToken);

            return TranslatorService.Translate<T>(records);
        }

        public async Task WriteAsync(string query, CancellationToken cancellationToken) =>
            await _queryExecutor.WriteAsync(query, cancellationToken);

        public async Task WriteAsync(string query, object parameters, CancellationToken cancellationToken) =>
            await _queryExecutor.WriteAsync(query, parameters, cancellationToken);

        public async Task WriteAsync(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken) =>
            await _queryExecutor.WriteAsync(query, parameters, cancellationToken);

        public async Task WriteAsync(Query query, CancellationToken cancellationToken) =>
            await _queryExecutor.WriteAsync(query, cancellationToken);

    }
}