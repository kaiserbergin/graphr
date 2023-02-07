using System;

namespace Graphr.Neo4j.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NeoProjectedEntity : Attribute
    {
        public string? Name { get; set; }

        public NeoProjectedEntity(string name = null)
        {
            Name = name;
        }
    }
}