using System;

namespace Graphr.Neo4j.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NeoRelationshipAttribute : Attribute
    {
        public string Type { get; set; }
        public RelationshipDirection Direction { get; set; }

        public NeoRelationshipAttribute(string type, RelationshipDirection direction)
        {
            Type = type;
            Direction = direction;
        }
    }
}