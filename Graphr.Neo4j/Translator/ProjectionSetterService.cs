using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphr.Neo4j.Attributes;

namespace Graphr.Neo4j.Translator
{
    public static class ProjectionSetterService
    {
        public static void SetProjection(PropertyInfo propertyInfo, object target, object sourceProjection, Dictionary<string, object> projections)
        {
            // TODO: IF we get a generic IEnumerable and the Generic argument is a NeoProjected Entity, run the below on each item in the list.
            if (EnumerableService.IsArrayOfNeoProjectedEntities(propertyInfo.PropertyType))
            {
                PopulateAndSetArrayToTarget(propertyInfo, target, sourceProjection, projections);
                return;
            }

            if (EnumerableService.IsGenericIEnumerable(propertyInfo.PropertyType) && EnumerableService.IsGenericArgumentNeoProjectedEntity(propertyInfo.PropertyType))
            {
                PopulateAndSetIEnumerableToTarget(propertyInfo, target, sourceProjection, projections);
                return;
            }

            if (IsNeoProjectedEntity(propertyInfo))
            {
                PopulateAndSetProjectedEntityToTarget(propertyInfo, target, sourceProjection, projections);
                return;
            }

            PropertySetterService.SetPropertyValue(propertyInfo, target, sourceProjection);
        }

        private static void PopulateAndSetArrayToTarget(
            PropertyInfo propertyInfo, 
            object target, 
            object sourceProjection, 
            Dictionary<string, object> projections)
        {
            object? entitiesArray = CreateArrayOfNeoProjectionEntities(
                propertyInfo,
                propertyInfo.PropertyType.GetElementType(),
                sourceProjection,
                projections);

            propertyInfo.SetValue(target, entitiesArray);
        }
        
        private static void PopulateAndSetIEnumerableToTarget(
            PropertyInfo propertyInfo, 
            object target, 
            object sourceProjection, 
            Dictionary<string, object> projections)
        {
            object? entitiesArray = CreateGenericIEnumerableFromObject(
                propertyInfo,
                propertyInfo.PropertyType.GetGenericArguments()[0],
                sourceProjection,
                projections);

            propertyInfo.SetValue(target, entitiesArray);
        }

        private static void PopulateAndSetProjectedEntityToTarget(
            PropertyInfo propertyInfo,
            object target,
            object sourceProjection,
            Dictionary<string, object> projections)
        {
            if (sourceProjection is not Dictionary<string, object> sourceProjectionDict)
                throw new Exception("Hey man, I can't make a class out of a not dictionary mkay?");

            var projectedTarget = Activator.CreateInstance(propertyInfo.PropertyType) ?? throw new ArgumentException($"Could not create an instance of {propertyInfo.PropertyType}");
            PopulateProjectedEntity(projectedTarget, sourceProjectionDict, projections);
            propertyInfo.SetValue(target, projectedTarget);
        }

        private static bool IsNeoProjectedEntity(PropertyInfo propertyInfo) =>
            propertyInfo
                .PropertyType
                .GetCustomAttributes()?
                .Any(a => a.GetType() == typeof(NeoProjectedEntity)) == true;

        private static void PopulateProjectedEntity(
            object target,
            Dictionary<string, object> source,
            Dictionary<string, object> projections)
        {
            foreach (var propertyInfo in target.GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttributes()?.FirstOrDefault(a => a.GetType() == typeof(NeoPropertyAttribute)) is NeoPropertyAttribute neoProperty)
                {
                    PropertySetterService.SetPropertyValue(propertyInfo, target, source[neoProperty.Name]);
                    continue;
                }

                if (propertyInfo.GetCustomAttributes()?.First(a => a.GetType() == typeof(NeoProjectionAttribute)) is NeoProjectionAttribute projectionAttribute)
                {
                    object? targetProjection = ProjectionRetrievalService.GetTargetProjection(projectionAttribute, source);

                    if (targetProjection is not null)
                        SetProjection(propertyInfo, target, targetProjection, source);
                }
            }
        }

        internal static object? CreateGenericIEnumerableFromObject(
            PropertyInfo propertyInfo,
            Type targetElementType,
            object sourceProjection,
            Dictionary<string, object> projections)
        {
            var projectionEntites = CreateArrayOfNeoProjectionEntities(
                propertyInfo, 
                targetElementType,
                sourceProjection,
                projections);

            return propertyInfo.PropertyType.IsInterface ? projectionEntites : Activator.CreateInstance(propertyInfo.PropertyType, projectionEntites);
        }

        internal static object? CreateArrayOfNeoProjectionEntities(
            PropertyInfo propertyInfo,
            Type targetElementType,
            object sourceProjection,
            Dictionary<string, object> projections)
        {
            if (sourceProjection is not List<object> sourceList)
                throw new Exception($"Hey man, I can't make a class out of a not dictionary mkay? Double check {propertyInfo.Name}: {propertyInfo.PropertyType}");

            var sourceProjectionList = sourceList
                .OfType<Dictionary<string, object>>()
                .ToList();

            if (sourceList.Count != sourceProjectionList.Count())
                throw new Exception($"Woah, funky projection found for {propertyInfo.Name}: {propertyInfo.PropertyType}");

            var genericListType = typeof(List<>).MakeGenericType(targetElementType)
                                  ?? throw new ArgumentException($"Could not create generic {targetElementType.FullName} typed list from.");
            var instance = Activator.CreateInstance(genericListType)!;
            var addMethod = genericListType.GetMethod("Add")
                            ?? throw new ArgumentException($"No Add method supported for {genericListType.FullName}");
            var toArrayMethod = genericListType.GetMethod("ToArray")
                                ?? throw new ArgumentException($"No ToArray method for {genericListType.FullName}");

            if (sourceProjectionList is null)
                return toArrayMethod.Invoke(instance, null);

            foreach (var projection in sourceProjectionList)
            {
                var projectedTarget = Activator.CreateInstance(targetElementType) ??
                                      throw new ArgumentException($"Could not create an instance of {targetElementType}");
                PopulateProjectedEntity(projectedTarget, projection, projections);
                addMethod!.Invoke(instance, new[] { projectedTarget });
            }

            return toArrayMethod.Invoke(instance, null);
        }
    }
}