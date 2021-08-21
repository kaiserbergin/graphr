using System.Collections.Generic;
using System.Linq;
using Graphr.Neo4j.Graphr;
using Neo4j.Driver;

namespace Graphr.Neo4j.Translator
{
    public static class TranslatorService
    {
        public static List<T> Translate<T>(List<IRecord> records) where T : class, new()
        {
            var result = new List<object>();

            var targetType = typeof(T);

            foreach (var record in records)
            {
                var neoLookups = new NeoLookups(record);
                var rootNode = NodesService.GetRootNode(record, targetType);

                var translatedNode = NodesService.TranslateNode(rootNode, targetType, new HashSet<long>(), neoLookups);

                result.Add(translatedNode);
            }

            return result.Select(obj => (T) obj).ToList();
        }
    }
}