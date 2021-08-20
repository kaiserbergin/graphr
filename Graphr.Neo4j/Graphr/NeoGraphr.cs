using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Graphr.Neo4j.Attributes;
using Graphr.Neo4j.QueryExecution;
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

            return Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(string query, object parameters) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, parameters);

            return Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(string query, IDictionary<string, object> parameters) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query, parameters);

            return Translate<T>(records);
        }

        public async Task<List<T>> ReadAsAsync<T>(Query query) where T : class, new()
        {
            var records = await _queryExecutor.ReadAsync(query);

            return Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query);

            return Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query, object parameters) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, parameters);

            return Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(string query, IDictionary<string, object> parameters) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query, parameters);

            return Translate<T>(records);
        }

        public async Task<List<T>> WriteAsAsync<T>(Query query) where T : class, new()
        {
            var records = await _queryExecutor.WriteAsync(query);

            return Translate<T>(records);
        }

        public async Task WriteAsync(string query) =>
            await _queryExecutor.WriteAsync(query);

        public async Task WriteAsync(string query, object parameters) =>
            await _queryExecutor.WriteAsync(query, parameters);

        public async Task WriteAsync(string query, Dictionary<string, object> parameters) =>
            await _queryExecutor.WriteAsync(query, parameters);

        public async Task WriteAsync(Query query) =>
            await _queryExecutor.WriteAsync(query);

        public List<T> Translate<T>(List<IRecord> records) where T : class, new()
        {
            var result = new List<object>();

            var targetType = typeof(T);

            foreach (var record in records)
            {
                var neoLookups = new NeoLookups(record);
                var rootNode = GetRootNode(record, targetType);

                var translatedNode = TranslateNode(rootNode, targetType, new HashSet<long>(), neoLookups);

                result.Add(translatedNode);
            }

            return result.Select(obj => (T) obj).ToList();
        }

        #region Translation Initilization

        private INode GetRootNode(IRecord record, Type targetType)
        {
            var attributes = Attribute.GetCustomAttributes(targetType);
            var labels = GetNodeLabels(attributes);

            foreach (var (_, recordValue) in record.Values)
            {
                if (recordValue is INode node)
                {
                    foreach (var label in node.Labels)
                    {
                        if (labels.Contains(label))
                        {
                            return node;
                        }
                    }
                }
            }

            throw new Exception("Anchor node not found sucka");
        }

        private HashSet<string> GetNodeLabels(Attribute[] attributes)
        {
            var labels = new HashSet<string>();

            foreach (var attribute in attributes)
            {
                if (attribute is NeoNodeAttribute nodeAttribute)
                {
                    labels.Add(nodeAttribute.Label);
                }
            }

            return labels;
        }

        #endregion

        #region Node translation

        private object TranslateNode(INode neoNode, Type targetType, HashSet<long> traversedIds, NeoLookups neoLookups)
        {
            if (targetType.GetConstructor(Type.EmptyTypes) == null)
                throw new Exception($"You need a paramless ctor bro. Class: {targetType.Name}");

            traversedIds = new HashSet<long>(traversedIds);
            var isFirstTraversal = traversedIds.Add(neoNode.Id);

            var target = Activator.CreateInstance(targetType) ?? throw new NullReferenceException($@"Could not create an instance of {targetType}");

            var targetProperties = targetType.GetProperties();

            foreach (var propertyInfo in targetProperties)
            {
                NeoPropertyAttribute? neoPropertyAttribute = null;
                NeoRelationshipAttribute? neoRelationshipAttribute = null;

                foreach (var customAttribute in Attribute.GetCustomAttributes(propertyInfo))
                {
                    if (customAttribute is NeoPropertyAttribute propertyAttribute)
                    {
                        neoPropertyAttribute = propertyAttribute;
                        break;
                    }

                    if (customAttribute is NeoRelationshipAttribute relationshipAttribute)
                    {
                        neoRelationshipAttribute = relationshipAttribute;
                        break;
                    }
                }

                if (neoPropertyAttribute?.Name != null && neoNode.Properties.TryGetValue(neoPropertyAttribute.Name, out var neoProp))
                {
                    SetPropertyValue(propertyInfo, target, neoProp);
                    continue;
                }

                if (isFirstTraversal && neoRelationshipAttribute?.Type != null)
                {
                    // need to determine target type when wrapped in IEnumerable
                    // Also we populated ALL the relationships and nodes instead of the ones in our current record.
                    var relationshipTargetType = propertyInfo.PropertyType;

                    if (typeof(string) != relationshipTargetType && typeof(IEnumerable).IsAssignableFrom(relationshipTargetType))
                    {
                        var relationshipTargetGenericType = relationshipTargetType.GetGenericArguments()[0];
                        var targetNodes = TranslateRelatedNodes(neoNode, neoRelationshipAttribute, relationshipTargetGenericType, traversedIds, neoLookups);

                        propertyInfo.SetValue(target, targetNodes);
                    }
                    else
                    {
                        var targetNode = TranslateRelatedNode(neoNode, neoRelationshipAttribute, relationshipTargetType, traversedIds, neoLookups);

                        propertyInfo.SetValue(target, targetNode);
                    }
                }
            }

            return target;
        }

        private static void SetPropertyValue(PropertyInfo propertyInfo, object target, object neoProp)
        {
            if (propertyInfo.PropertyType == typeof(LocalDate))
                propertyInfo.SetValue(target, neoProp.As<LocalDate>());
            else if (propertyInfo.PropertyType == typeof(LocalTime))
                propertyInfo.SetValue(target, neoProp.As<LocalTime>());
            else if (propertyInfo.PropertyType == typeof(ZonedDateTime))
                propertyInfo.SetValue(target, neoProp.As<ZonedDateTime>());
            else if (propertyInfo.PropertyType == typeof(LocalDateTime))
                propertyInfo.SetValue(target, neoProp.As<LocalDateTime>());
            else
                propertyInfo.SetValue(target, neoProp);
        }

        private IList TranslateRelatedNodes(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type relationshipTargetType,
            HashSet<long> traversedIds,
            NeoLookups neoLookups)
        {
            var targetTypeLabels = GetTargetTypeLabels(relationshipTargetType);

            var genericListType = typeof(List<>).MakeGenericType(relationshipTargetType) ?? throw new NullReferenceException($@"Could not create generic {relationshipTargetType} typed list from.");
            var targetNodes = (IList) Activator.CreateInstance(genericListType)!;

            var relationshipsOfTargetType = neoLookups.RelationshipLookup[neoRelationshipAttribute.Type];

            foreach (var relationship in relationshipsOfTargetType)
            {
                if (IsTranslatableRelationship(sourceNode, neoRelationshipAttribute, relationship, targetTypeLabels, neoLookups, out var targetNode))
                {
                    var translatedNode = TranslateNode(targetNode!, relationshipTargetType, traversedIds, neoLookups);
                    targetNodes!.Add(translatedNode);
                }
            }

            return targetNodes;
        }

        private HashSet<string> GetTargetTypeLabels(Type targetNodeType)
        {
            var targetTypeCustomAttributes = Attribute.GetCustomAttributes(targetNodeType);
            var targetTypeLabels = GetNodeLabels(targetTypeCustomAttributes);
            return targetTypeLabels;
        }

        private bool IsTranslatableRelationship(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            IRelationship relationship,
            HashSet<string> targetTypeLabels,
            NeoLookups neoLookups,
            out INode? targetNode)
        {
            targetNode = null;

            var (relationshipSourceId, relationshipTargetId) = neoRelationshipAttribute.Direction switch
            {
                RelationshipDirection.Incoming => (relationship.EndNodeId, relationship.StartNodeId),
                RelationshipDirection.Outgoing => (relationship.StartNodeId, relationship.EndNodeId),
                _ => throw new Exception("Invalid Direction")
            };

            if (sourceNode.Id != relationshipSourceId) return false;

            if (neoLookups.NodesById.TryGetValue(relationshipTargetId, out var candidateTargetNode))
            {
                if (candidateTargetNode.Labels.Any(targetTypeLabels.Contains))
                {
                    targetNode = candidateTargetNode;
                    return true;
                }
            }

            return false;
        }

        private object TranslateRelatedNode(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type targetNodeType,
            HashSet<long> traversedIds,
            NeoLookups neoLookups)
        {
            var targetTypeLabels = GetTargetTypeLabels(targetNodeType);
            INode? nodeToTranslate = null;

            var relationshipsOfTargetType = neoLookups.RelationshipLookup[neoRelationshipAttribute.Type];

            foreach (var relationship in relationshipsOfTargetType)
            {
                if (IsTranslatableRelationship(sourceNode, neoRelationshipAttribute, relationship, targetTypeLabels, neoLookups, out var targetNode))
                {
                    if (nodeToTranslate != null)
                        throw new Exception("You have multiple relationship matches for a 1 to 1 mapping.");

                    nodeToTranslate = targetNode;
                }
            }

            if (nodeToTranslate == null)
                throw new ArgumentNullException($"No translatable realtionship found for relationship type: {neoRelationshipAttribute.Type}");

            return TranslateNode(nodeToTranslate, targetNodeType, traversedIds, neoLookups);
        }

        #endregion
    }
}