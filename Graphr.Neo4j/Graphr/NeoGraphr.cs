using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphr.Neo4j.Attributes;
using Graphr.Neo4j.QueryExecution;
using Neo4j.Driver;

namespace Graphr.Neo4j.Graphr
{
    public class NeoGraphr : INeoGraphr
    {
        private readonly IQueryExecutor _queryExecutor;
        private Dictionary<long, INode> _nodesById = new Dictionary<long, INode>();
        private ILookup<string, IRelationship> _relationshipLookup;

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

        public List<T> Translate<T>(List<IRecord> records) where T : class, new()
        {
            var result = new List<object>();

            var targetType = typeof(T);

            foreach (var record in records)
            {
                AssignNeoLookups(record);
                var rootNode = GetRootNode(record, targetType);

                var translatedNode = TranslateNode(rootNode, targetType, new HashSet<long>());

                result.Add(translatedNode);
            }

            return result.Select(obj => (T) obj).ToList();
        }

        #region Translation Initilization

        private void AssignNeoLookups(IRecord record)
        {
            AssignNodes(record);
            AssignRelationships(record);
        }

        private void AssignNodes(IRecord record)
        {
            foreach (var (_, recordValue) in record.Values)
            {
                if (recordValue is INode node)
                {
                    _nodesById.TryAdd(node.Id, node);
                }
                else if (recordValue is List<object> objectList && objectList.FirstOrDefault() != null &&
                         objectList.FirstOrDefault() is INode)
                {
                    foreach (var obj in objectList)
                    {
                        var nodeFromObj = obj as INode;
                        if (nodeFromObj?.Id != null)
                            _nodesById.TryAdd(nodeFromObj.Id, nodeFromObj);
                    }
                }
            }
        }

        private void AssignRelationships(IRecord record)
        {
            var distinctRelationships = GetDistinctRelationships(record);

            _relationshipLookup = distinctRelationships.ToLookup(rel => rel.Type, rel => rel);
        }

        private List<IRelationship> GetDistinctRelationships(IRecord record)
        {
            var relationships = new Dictionary<long, IRelationship>();

            foreach (var (_, recordValue) in record.Values)
            {
                if (recordValue is IRelationship relationship)
                {
                    relationships.TryAdd(relationship.Id, relationship);
                }
                else if (recordValue is List<object> objectList && objectList.FirstOrDefault() != null &&
                         objectList.FirstOrDefault() is IRelationship)
                {
                    foreach (var obj in objectList)
                    {
                        var relationshipFromObj = obj as IRelationship;
                        if (relationshipFromObj?.Id != null)
                            relationships.TryAdd(relationshipFromObj.Id, relationshipFromObj);
                    }
                }
            }

            return relationships.Values.ToList();
        }

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

        private object TranslateNode(INode neoNode, Type targetType, HashSet<long> traversedIds)
        {
            if (targetType.GetConstructor(Type.EmptyTypes) == null)
                throw new Exception($"You need a paramless ctor bro. Class: {targetType.Name}");

            traversedIds = new HashSet<long>(traversedIds);
            var isFirstTraversal = traversedIds.Add(neoNode.Id);

            var target = Activator.CreateInstance(targetType);

            var targetProperties = targetType.GetProperties();

            foreach (var propertyInfo in targetProperties)
            {
                NeoPropertyAttribute neoPropertyAttribute = null;
                NeoRelationshipAttribute neoRelationshipAttribute = null;

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
                    propertyInfo.SetValue(target, neoProp);
                    continue;
                }

                if (isFirstTraversal && neoRelationshipAttribute?.Type != null)
                {
                    // need to determine target type when wrapped in IEnumerable
                    // Also we populated ALL the relationships and nodes instead of the ones in our current record.
                    var relationshipTargetType = propertyInfo.PropertyType;

                    if (typeof(string) != relationshipTargetType && typeof(IEnumerable).IsAssignableFrom(relationshipTargetType))
                    {
                        var targetNodeType = relationshipTargetType.GetGenericArguments()[0];
                        var targetNodes = TranslateRelatedNodes(neoNode, neoRelationshipAttribute, targetNodeType, traversedIds);

                        propertyInfo.SetValue(target, targetNodes);
                    }
                    else
                    {
                        var targetNode = TranslateRelatedNode(neoNode, neoRelationshipAttribute, relationshipTargetType, traversedIds);

                        propertyInfo.SetValue(target, targetNode);
                    }
                }
            }

            return target;
        }

        private IList TranslateRelatedNodes(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type targetNodeType,
            HashSet<long> traversedIds)
        {
            var targetTypeLabels = GetTargetTypeLabels(targetNodeType);

            var genericListType = typeof(List<>).MakeGenericType(targetNodeType);
            var targetNodes = (IList) Activator.CreateInstance(genericListType);

            var relationshipsOfTargetType = _relationshipLookup[neoRelationshipAttribute.Type];

            foreach (var relationship in relationshipsOfTargetType)
            {
                if (IsTranslatableRelationship(sourceNode, neoRelationshipAttribute, relationship, targetTypeLabels, out var targetNode))
                {
                    var translatedNode = TranslateNode(targetNode, targetNodeType, traversedIds);
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
            out INode targetNode)
        {
            targetNode = null;

            var (relationshipSourceId, relationshipTargetId) = neoRelationshipAttribute.Direction switch
            {
                RelationshipDirection.Incoming => (relationship.EndNodeId, relationship.StartNodeId),
                RelationshipDirection.Outgoing => (relationship.StartNodeId, relationship.EndNodeId),
                _ => throw new Exception("Invalid Direction")
            };

            if (sourceNode.Id == relationshipSourceId)
            {
                if (_nodesById.TryGetValue(relationshipTargetId, out var candidateTargetNode))
                {
                    foreach (var label in candidateTargetNode.Labels)
                    {
                        if (targetTypeLabels.Contains(label))
                        {
                            targetNode = candidateTargetNode;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private object TranslateRelatedNode(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type targetNodeType,
            HashSet<long> traversedIds)
        {
            var targetTypeLabels = GetTargetTypeLabels(targetNodeType);
            INode nodeToTranslate = null;

            var relationshipsOfTargetType = _relationshipLookup[neoRelationshipAttribute.Type];

            foreach (var relationship in relationshipsOfTargetType)
            {
                if (IsTranslatableRelationship(sourceNode, neoRelationshipAttribute, relationship, targetTypeLabels, out var targetNode))
                {
                    if (nodeToTranslate != null)
                        throw new Exception("You have multiple relationship matches for a 1 to 1 mapping.");

                    nodeToTranslate = targetNode;
                }
            }

            return TranslateNode(nodeToTranslate, targetNodeType, traversedIds);
        }

        #endregion
    }
}