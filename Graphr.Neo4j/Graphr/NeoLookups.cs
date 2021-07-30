using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver;

namespace Graphr.Neo4j.Graphr
{
    public class NeoLookups
    {
        public readonly Dictionary<long, INode> NodesById;
        public readonly ILookup<string, IRelationship> RelationshipLookup;

        public NeoLookups(IRecord record)
        {
            NodesById = GetDistinctNodesById(record);
            RelationshipLookup = GetDistinctRelationshipsByType(record);
        }

        private Dictionary<long, INode> GetDistinctNodesById(IRecord record)
        {
            var nodesById = new Dictionary<long, INode>();

            foreach (var (_, recordValue) in record.Values)
            {
                if (recordValue is INode node)
                {
                    nodesById.TryAdd(node.Id, node);
                }
                else if (recordValue is List<object> objectList && objectList.FirstOrDefault() != null &&
                         objectList.FirstOrDefault() is INode)
                {
                    foreach (var obj in objectList)
                    {
                        var nodeFromObj = obj as INode;
                        if (nodeFromObj?.Id != null)
                            nodesById.TryAdd(nodeFromObj.Id, nodeFromObj);
                    }
                }
            }

            return nodesById;
        }

        private ILookup<string, IRelationship> GetDistinctRelationshipsByType(IRecord record)
        {
            var distinctRelationships = GetDistinctRelationships(record);

            return distinctRelationships.ToLookup(rel => rel.Type, rel => rel);
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
    }
}