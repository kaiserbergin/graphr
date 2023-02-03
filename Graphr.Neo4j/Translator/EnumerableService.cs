using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphr.Neo4j.Attributes;

namespace Graphr.Neo4j.Translator
{
    internal static class EnumerableService
    {
        internal static object? CreateGenericIEnumerableFromObject(Type type, object? obj)
        {
            var arrayFromNeoProp = CreateArrayFromObject(obj, type.GetGenericArguments()[0]);
            
            return type.IsInterface ? arrayFromNeoProp : Activator.CreateInstance(type, arrayFromNeoProp);
        }

        internal static object? CreateArrayFromObject(object? obj, Type targetElementType)
        {
            var genericListType = typeof(List<>).MakeGenericType(targetElementType)
                                  ?? throw new ArgumentException($"Could not create generic {targetElementType.FullName} typed list from.");
            var instance = Activator.CreateInstance(genericListType)!;
            var addMethod = genericListType.GetMethod("Add")
                            ?? throw new ArgumentException($"No Add method supported for {genericListType.FullName}");
            var toArrayMethod = genericListType.GetMethod("ToArray")
                                ?? throw new ArgumentException($"No ToArray method for {genericListType.FullName}");

            if (obj is null)
                return toArrayMethod.Invoke(instance, null);

            foreach (var item in (IList)obj)
            {
                var convertedItem = PropertySetterService.ConvertToClrType(targetElementType, item);
                addMethod!.Invoke(instance, new[] { Convert.ChangeType(convertedItem, targetElementType) });
            }

            return toArrayMethod.Invoke(instance, null);
        }
        
        internal static bool IsGenericIEnumerable(Type type) =>
            type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type) && type is { IsGenericType: true, IsArray: false };

        internal static bool CanAssignToIEnumerable(object? obj) =>
            obj is null ||
            obj.GetType() != typeof(string) &&
            obj.GetType().IsAssignableTo(typeof(IEnumerable));

        internal static bool IsArrayOfNeoProjectedEntities(Type type) =>
            type.IsArray &&
            type.GetElementType()
                .GetCustomAttributes()
                ?.Any(a => a.GetType() == typeof(NeoProjectedEntity)) == true;

        internal static bool IsGenericArgumentNeoProjectedEntity(Type type) =>
            type.GetGenericArguments()?[0]
                ?.GetCustomAttributes()
                ?.Any(a => a.GetType() == typeof(NeoProjectedEntity)) == true;
    }
}