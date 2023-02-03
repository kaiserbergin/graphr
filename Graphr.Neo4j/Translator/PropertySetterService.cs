using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neo4j.Driver;

namespace Graphr.Neo4j.Translator
{
    internal static class PropertySetterService
    {
        internal static void SetPropertyValue(PropertyInfo targetPropertyInfo, object targetClass, object source)
        {
            var convertedNeoProp = ConvertToClrType(targetPropertyInfo.PropertyType, source);
            targetPropertyInfo.SetValue(targetClass, convertedNeoProp);
        }

        internal static object? ConvertToClrType(Type type, object? neoProp)
        {
            if (type.IsArray)
                return EnumerableService.CreateArrayFromObject(neoProp, type.GetElementType()!);

            if (typeof(IDictionary).IsAssignableFrom(type))
                return DictionaryService.GetGenericDictionary(neoProp, type);
                
            if (EnumerableService.IsGenericIEnumerable(type) && EnumerableService.CanAssignToIEnumerable(neoProp))
                return EnumerableService.CreateGenericIEnumerableFromObject(type, neoProp);

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
    }
}