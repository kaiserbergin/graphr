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
            var convertedNeoProp = ConvertToClrType(propertyInfo.PropertyType, neoProp);
            propertyInfo.SetValue(target, convertedNeoProp);
        }

        private static object? ConvertToClrType(Type type, object? neoProp)
        {
            if (IsGenericIEnumerable(type))
                return CreateListFromNeo(type, neoProp);

            if (type == typeof(DateTime))
                return neoProp.As<DateTime>();
            if (type == typeof(DateTimeOffset))
                return neoProp.As<DateTimeOffset>();
            if (type == typeof(TimeSpan))
                return neoProp.As<TimeSpan>();
            if (type == typeof(string))
                return neoProp.As<string>();

            return neoProp;
        }

        private static object CreateListFromNeo(Type type, object? neoProp)
        {
            var genericType = type.GetGenericArguments()[0];

            var genericListType = typeof(List<>).MakeGenericType(genericType) ?? throw new NullReferenceException($@"Could not create generic {genericType} typed list from.");
            var instance = (IList) Activator.CreateInstance(genericListType)!;

            if (neoProp == null) return instance;

            foreach (var item in (IList) neoProp)
            {
                var convertedItem = ConvertToClrType(genericType, item);
                instance.Add(Convert.ChangeType(convertedItem, genericType));
            }

            return instance;
        }

        private static bool IsGenericIEnumerable(Type type) =>
            type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType;
    }
}