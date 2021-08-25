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
            if (propertyInfo.PropertyType == typeof(DateTime))
                propertyInfo.SetValue(target, neoProp.As<DateTime>());
            else if (propertyInfo.PropertyType == typeof(DateTimeOffset))
                propertyInfo.SetValue(target, neoProp.As<DateTimeOffset>());
            else if (propertyInfo.PropertyType == typeof(TimeSpan))
                propertyInfo.SetValue(target, neoProp.As<TimeSpan>());
            else if (IsGenericIEnumerable(propertyInfo.PropertyType))
                SetIEnumerableProperty(propertyInfo, target, neoProp);
            else
                propertyInfo.SetValue(target, neoProp);
        }

        private static void SetIEnumerableProperty(PropertyInfo propertyInfo, object target, object neoProp)
        {
            var list = CreateListFromNeo(propertyInfo.PropertyType, neoProp);
            
            propertyInfo.SetValue(target, list);
        }

        private static object CreateListFromNeo(Type type, object neoProp)
        {
            var genericType = type.GetGenericArguments()[0];
            
            var genericListType = typeof(List<>).MakeGenericType(genericType) ?? throw new NullReferenceException($@"Could not create generic {genericType} typed list from.");
            var instance = (IList) Activator.CreateInstance(genericListType)!;

            if (neoProp == null) return instance;
            
            foreach (var item in (IList) neoProp)
            {
                instance.Add(Convert.ChangeType(item, genericType));
            }

            return instance;
        }

        private static bool IsGenericIEnumerable(Type type) =>
            type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType;
    }
}