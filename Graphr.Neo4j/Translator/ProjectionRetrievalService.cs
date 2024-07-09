using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Graphr.Neo4j.Attributes;
using Neo4j.Driver;

namespace Graphr.Neo4j.Translator
{
    public static class ProjectionRetrievalService
    {
        public static Dictionary<string, object> GetProjections(IRecord record)
        {
            var projections = new Dictionary<string, object>();

            foreach (var dataSet in record.Values)
            {
                if (IsNeoType(dataSet.Value.GetType()))
                    continue;

                if (IsProjectedList(dataSet.Value, out var genericListType))
                {
                    if (!IsNeoType(genericListType))
                        TryAddToProjections(dataSet, projections);

                    continue;
                }

                TryAddToProjections(dataSet, projections);
            }

            return projections;
        }

        private static void TryAddToProjections(KeyValuePair<string, object> dataSet, Dictionary<string, object> projections)
        {
            if (!projections.TryAdd(dataSet.Key, dataSet.Value))
                throw new Exception("Yo dawg, you named multiple projections by the same name and I can't deal with that yo.");
        }

        private static bool IsNeoType(Type type) =>
            type.IsAssignableTo(typeof(INode)) || type.IsAssignableTo(typeof(IRelationship));

        private static bool IsProjectedList(object obj, [NotNullWhen(true)] out Type? genericType)
        {
            genericType = null;

            var type = obj.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ICollection<>))
                return false;

            genericType = type.GenericTypeArguments[0];

            return true;
        }

        public static object? GetTargetProjection(NeoProjectionAttribute projectionAttribute, Dictionary<string, object> projections, object target = null)
        {
            if (projectionAttribute is { ProjectionName: not null }
                && projections.TryGetValue(projectionAttribute.ProjectionName, out var projectionObject))
            {
                if (projectionAttribute is { ProjectionKey: null, MatchOn: null })
                    return projectionObject;

                if (projectionAttribute.ProjectionKey is null || projectionAttribute.MatchOn is null)
                    throw new Exception(
                        $"Either leave both projectionKey ({projectionAttribute.ProjectionKey}) and matchOn ({projectionAttribute.MatchOn}) null, or specify both for projectionName: {projectionAttribute.ProjectionName}");

                if (projectionObject is not List<object> objList)
                    throw new Exception($"Your query needs to return a list of json like objects for {projectionAttribute.ProjectionName} to work.");
                
                var dictList = objList
                    .OfType<Dictionary<string, object>>()
                    .ToList();
                
                if (dictList.Count != objList.Count)
                    throw new Exception($"Your query needs to return a list of json like objects for {projectionAttribute.ProjectionName} to work.");

                var matchOnProperty = target.GetType()
                    .GetProperties()
                    .FirstOrDefault(p => p.Name == projectionAttribute.MatchOn);

                if (matchOnProperty is null)
                    throw new Exception($"No MatchOn property matches {projectionAttribute.MatchOn} for {projectionAttribute.ProjectionName}");

                var matchOnValue = matchOnProperty.GetValue(target)?.ToString() ?? string.Empty;

                return dictList?
                    .FirstOrDefault(dict =>
                        dict.TryGetValue(projectionAttribute.ProjectionKey, out var pValue)
                        && matchOnValue == pValue.ToString());
            }

            return null;
        }
    }
}