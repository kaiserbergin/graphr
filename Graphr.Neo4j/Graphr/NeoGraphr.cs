using System;
using System.Collections.Generic;
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

        public async Task<List<T>> ReadAsAsync<T>(string query) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(string query, object parameters) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, parameters);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(string query, IDictionary<string, object> parameters) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, parameters);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(Query query) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query, object parameters) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, parameters);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query, IDictionary<string, object> parameters) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, parameters);

            return TranslatorService.Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(Query query) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query);

            return TranslatorService.Translate<T>(records);
        }

        public async Task WriteAsync(string query) =>
            await _queryExecutor.WriteAsync(query);

        public async Task WriteAsync(string query, object parameters) =>
            await _queryExecutor.WriteAsync(query, parameters);

        public async Task WriteAsync(string query, Dictionary<string, object> parameters) =>
            await _queryExecutor.WriteAsync(query, parameters);

        public async Task WriteAsync(Query query) =>
            await _queryExecutor.WriteAsync(query);

    }
}