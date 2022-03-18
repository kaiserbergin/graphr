using System;
using System.Collections;
using System.Collections.Generic;

namespace Graphr.Neo4j.Translator
{
    internal static class EnumerableService
    {
        internal static object? CreateGenericIEnumerableFromNeoProp(Type type, object? neoProp)
        {
            var arrayFromNeoProp = CreateArrayFromNeoProp(neoProp, type.GetGenericArguments()[0]);
            
            return type.IsInterface ? arrayFromNeoProp : Activator.CreateInstance(type, arrayFromNeoProp);
        }

        internal static object? CreateArrayFromNeoProp(object? neoProp, Type targetElementType)
        {
            var genericListType = typeof(List<>).MakeGenericType(targetElementType)
                                  ?? throw new ArgumentException($"Could not create generic {targetElementType.FullName} typed list from.");
            var instance = Activator.CreateInstance(genericListType)!;
            var addMethod = genericListType.GetMethod("Add")
                            ?? throw new ArgumentException($"No Add method supported for {genericListType.FullName}");
            var toArrayMethod = genericListType.GetMethod("ToArray")
                                ?? throw new ArgumentException($"No ToArray method for {genericListType.FullName}");

            if (neoProp is null)
                return toArrayMethod.Invoke(instance, null);

            foreach (var item in (IList)neoProp)
            {
                var convertedItem = PropertySetterService.ConvertToClrType(targetElementType, item);
                addMethod!.Invoke(instance, new[] { Convert.ChangeType(convertedItem, targetElementType) });
            }

            return toArrayMethod.Invoke(instance, null);
        }

        internal static bool IsGenericIEnumerable(Type type) =>
            type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType && !type.IsArray;

        internal static bool CanAssignToIEnumerable(object? obj) =>
            obj is null ||
            obj.GetType() != typeof(string) &&
            obj.GetType().IsAssignableTo(typeof(IEnumerable));
    }
}