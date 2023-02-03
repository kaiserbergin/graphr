using System;
using System.Collections;
using System.Collections.Generic;

namespace Graphr.Neo4j.Translator
{
    public static class DictionaryService
    {
        public static IDictionary GetGenericDictionary(object neoProp, Type type)
        {
            if (neoProp is not IDictionary dict)
                throw new Exception("Error converting to clr type. Specified class property does not match returned object from database.");
                
            Type[] genericTypeArguments = type.GenericTypeArguments;
            IDictionary instance = (IDictionary) Activator.CreateInstance(typeof (Dictionary<,>).MakeGenericType(genericTypeArguments));
                
            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext())
                instance.Add(enumerator.Key, enumerator.Value);

            return instance;
        }
    
    }
}