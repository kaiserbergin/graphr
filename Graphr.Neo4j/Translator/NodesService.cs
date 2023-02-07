using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graphr.Neo4j.Attributes;
using Graphr.Neo4j.Graphr;
using Neo4j.Driver;

namespace Graphr.Neo4j.Translator
{
    internal static class NodesService
    {
        internal static INode GetRootNode(IRecord record, Type targetType)
        {
            var attributes = Attribute.GetCustomAttributes(targetType);
            var labels = NodesService.GetNodeLabels(attributes);

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

        internal static HashSet<string> GetNodeLabels(Attribute[] attributes)
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

        internal static object TranslateNode(
            INode neoNode, 
            Type targetType, 
            HashSet<long> traversedIds, 
            NeoLookups neoLookups, 
            Dictionary<string, object> projections)
        {
            if (targetType.GetConstructor(Type.EmptyTypes) == null)
                throw new Exception($"You need a paramless ctor bro. Class: {targetType.Name}");

            traversedIds = new HashSet<long>(traversedIds);
            var isFirstTraversal = traversedIds.Add(neoNode.Id);

            var target = Activator.CreateInstance(targetType) ?? throw new ArgumentException($"Could not create an instance of {targetType}");

            var targetProperties = targetType.GetProperties();

            foreach (var propertyInfo in targetProperties)
            {
                NeoPropertyAttribute? neoPropertyAttribute = null;
                NeoRelationshipAttribute? neoRelationshipAttribute = null;
                NeoLabelsAttribute? neoLabelsAttribute = null;

                foreach (var customAttribute in Attribute.GetCustomAttributes(propertyInfo))
                {
                    switch (customAttribute)
                    {
                        case NeoLabelsAttribute labelsAttribute:
                            neoLabelsAttribute = labelsAttribute;
                            break;
                        case NeoPropertyAttribute propertyAttribute:
                            neoPropertyAttribute = propertyAttribute;
                            break;
                        case NeoRelationshipAttribute relationshipAttribute:
                            neoRelationshipAttribute = relationshipAttribute;
                            break;
                    }
                }

                if (neoLabelsAttribute != null)
                {
                    if (propertyInfo.PropertyType.IsArray || EnumerableService.IsGenericIEnumerable(propertyInfo.PropertyType) && EnumerableService.CanAssignToIEnumerable(neoNode.Labels))
                        PropertySetterService.SetPropertyValue(propertyInfo, target, neoNode.Labels);
                    else
                        throw new Exception($"You done messed up, Labels needs to be generic type but you used {propertyInfo.PropertyType} for {propertyInfo.Name}");
                    
                    continue;
                }

                if (neoPropertyAttribute?.Name != null && neoNode.Properties.TryGetValue(neoPropertyAttribute.Name, out var neoProp))
                {
                    PropertySetterService.SetPropertyValue(propertyInfo, target, neoProp);
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
                        var targetClasses = RelationshipTranslationService.TranslateRelatedTargets(
                            neoNode, 
                            neoRelationshipAttribute, 
                            relationshipTargetGenericType, 
                            traversedIds, 
                            neoLookups,
                            projections);

                        propertyInfo.SetValue(target, targetClasses);
                    }
                    else
                    {
                        var targetNode = RelationshipTranslationService.TranslateRelatedNode(
                            neoNode, 
                            neoRelationshipAttribute, 
                            relationshipTargetType, 
                            traversedIds, 
                            neoLookups,
                            projections);

                        propertyInfo.SetValue(target, targetNode);
                    }
                }
            }

            foreach (var propertyInfo in targetProperties)
            {
                if (Attribute.GetCustomAttributes(propertyInfo)?.FirstOrDefault(a => a.GetType() == typeof(NeoProjectionAttribute)) is NeoProjectionAttribute neoProjectionAttribute)
                {
                    object? projection = ProjectionRetrievalService.GetTargetProjection(neoProjectionAttribute, projections, target);
                    ProjectionSetterService.SetProjection(propertyInfo, target, projection, projections);
                }
            }
            
            return target;
        }
    }
}