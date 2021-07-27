using System;

namespace Graphr.Neo4j.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NeoNodeAttribute : Attribute
    {
        public string Label { get; }

        public NeoNodeAttribute(string label)
        {
            Label = label;
        }
    }
}