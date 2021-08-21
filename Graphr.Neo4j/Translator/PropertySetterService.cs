using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Neo4j.Driver;

namespace Graphr.Neo4j.Translator
{
    internal static class PropertySetterService
    {
        internal static void SetPropertyValue(PropertyInfo propertyInfo, object target, object neoProp)
        {
            if (propertyInfo.PropertyType == typeof(LocalDate))
                propertyInfo.SetValue(target, neoProp.As<LocalDate>());
            else if (propertyInfo.PropertyType == typeof(LocalTime))
                propertyInfo.SetValue(target, neoProp.As<LocalTime>());
            else if (propertyInfo.PropertyType == typeof(ZonedDateTime))
                propertyInfo.SetValue(target, neoProp.As<ZonedDateTime>());
            else if (propertyInfo.PropertyType == typeof(LocalDateTime))
                propertyInfo.SetValue(target, neoProp.As<LocalDateTime>());
            else if (propertyInfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                SetIEnumerableProperty(propertyInfo, target, neoProp);
            else
                propertyInfo.SetValue(target, neoProp);
        }

        private static void SetIEnumerableProperty(PropertyInfo propertyInfo, object target, object neoProp)
        {
            var genericType = propertyInfo.PropertyType.GetGenericArguments()[0];
            var genericListType = typeof(List<>).MakeGenericType(genericType) ?? throw new NullReferenceException($@"Could not create generic {genericType} typed list from.");
            var genericRelationshipEntityList = (IList) Activator.CreateInstance(genericListType)!;

            foreach (var item in (IList) neoProp)
            {
                genericRelationshipEntityList.Add(item);
            }

            propertyInfo.SetValue(target, genericRelationshipEntityList);
        }
    }
}