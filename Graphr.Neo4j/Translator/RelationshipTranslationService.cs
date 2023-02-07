using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphr.Neo4j.Attributes;
using Graphr.Neo4j.Graphr;
using Neo4j.Driver;

namespace Graphr.Neo4j.Translator
{
    internal static class RelationshipTranslationService
    {
        internal static IList TranslateRelatedTargets(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type relationshipTargetType,
            HashSet<long> traversedIds,
            NeoLookups neoLookups,
            Dictionary<string, object> projections)
        {
            return IsRelationshipEntity(relationshipTargetType)
                ? TranslateRelationshipEntityBasedTargets(
                    sourceNode, 
                    neoRelationshipAttribute, 
                    relationshipTargetType, 
                    traversedIds, 
                    neoLookups,
                    projections)
                : TranslateTargetNodes(
                    sourceNode, 
                    neoRelationshipAttribute, 
                    relationshipTargetType, 
                    traversedIds, 
                    neoLookups,
                    projections);
        }

        private static bool IsRelationshipEntity(Type relationshipTargetType) =>
            Attribute.GetCustomAttributes(relationshipTargetType).Any(attribute => attribute is NeoRelationshipEntityAttribute);

        private static IList TranslateRelationshipEntityBasedTargets(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type relationshipEntityType,
            HashSet<long> traversedIds,
            NeoLookups neoLookups,
            Dictionary<string, object> projections)
        {
            var genericListType = typeof(List<>).MakeGenericType(relationshipEntityType) ?? throw new NullReferenceException($@"Could not create generic {relationshipEntityType} typed list from.");
            var genericRelationshipEntityList = (IList) Activator.CreateInstance(genericListType)!;

            var targetNodePropertyInfo = GetTargetNodePropertyInfoFromRelationshipEntity(relationshipEntityType);
            var targetNodeType = targetNodePropertyInfo.PropertyType;

            var enumerator = GetTranslatedTargetNodeAndRelationship(
                sourceNode, 
                neoRelationshipAttribute, 
                targetNodeType, 
                traversedIds, 
                neoLookups,
                projections);

            foreach (var (targetNode, neoRelationship) in enumerator)
            {
                var relationshipEntity = Activator.CreateInstance(relationshipEntityType) ?? throw new NullReferenceException($@"Could not create an instance of {relationshipEntityType}");

                foreach (var propertyInfo in relationshipEntityType.GetProperties())
                {
                    foreach (var customAttribute in propertyInfo.GetCustomAttributes())
                    {
                        if (customAttribute is NeoPropertyAttribute neoPropertyAttribute)
                        {
                            if (neoPropertyAttribute.Name != null && neoRelationship.Properties.TryGetValue(neoPropertyAttribute.Name, out var neoProp))
                            {
                                PropertySetterService.SetPropertyValue(propertyInfo, relationshipEntity, neoProp);
                                break;
                            }
                        }

                        if (customAttribute is NeoTargetNodeAttribute)
                        {
                            PropertySetterService.SetPropertyValue(propertyInfo, relationshipEntity, targetNode);
                        }
                    }
                }

                genericRelationshipEntityList.Add(relationshipEntity);
            }

            return genericRelationshipEntityList;
        }

        private static PropertyInfo GetTargetNodePropertyInfoFromRelationshipEntity(Type relationshipEntityType) =>
            relationshipEntityType
                .GetProperties()
                .Single(info => Attribute.GetCustomAttributes(info).Count(attribute => attribute is NeoTargetNodeAttribute) == 1);

        private static IEnumerable<(object, IRelationship)> GetTranslatedTargetNodeAndRelationship(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type targetNodeType,
            HashSet<long> traversedIds,
            NeoLookups neoLookups,
            Dictionary<string, object> projections)
        {
            var targetTypeLabels = GetTargetTypeLabels(targetNodeType);
            var relationshipsOfTargetType = neoLookups.RelationshipLookup[neoRelationshipAttribute.Type];

            foreach (var relationship in relationshipsOfTargetType)
            {
                if (IsTranslatableRelationship(sourceNode, neoRelationshipAttribute, relationship, targetTypeLabels, neoLookups, out var targetNode))
                {
                    yield return (NodesService.TranslateNode(targetNode!, targetNodeType, traversedIds, neoLookups, projections), relationship);
                }
            }
        }

        private static IList TranslateTargetNodes(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type targetNodeType,
            HashSet<long> traversedIds,
            NeoLookups neoLookups,
            Dictionary<string, object> projections)
        {
            var genericListType = typeof(List<>).MakeGenericType(targetNodeType) ?? throw new NullReferenceException($@"Could not create generic {targetNodeType} typed list from.");
            var targetNodes = (IList) Activator.CreateInstance(genericListType)!;

            var enumerator = GetTranslatedTargetNodeAndRelationship(
                sourceNode, 
                neoRelationshipAttribute, 
                targetNodeType, 
                traversedIds, 
                neoLookups,
                projections);

            foreach (var (node, _) in enumerator)
            {
                targetNodes.Add(node);
            }

            return targetNodes;
        }

        private static HashSet<string> GetTargetTypeLabels(Type targetNodeType)
        {
            var targetTypeCustomAttributes = Attribute.GetCustomAttributes(targetNodeType);
            var targetTypeLabels = NodesService.GetNodeLabels(targetTypeCustomAttributes);
            return targetTypeLabels;
        }

        private static bool IsTranslatableRelationship(
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

        internal static object TranslateRelatedNode(
            INode sourceNode,
            NeoRelationshipAttribute neoRelationshipAttribute,
            Type targetNodeType,
            HashSet<long> traversedIds,
            NeoLookups neoLookups,
            Dictionary<string, object> projections)
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

            return NodesService.TranslateNode(
                nodeToTranslate, 
                targetNodeType, 
                traversedIds, 
                neoLookups,
                projections);
        }
    }
}