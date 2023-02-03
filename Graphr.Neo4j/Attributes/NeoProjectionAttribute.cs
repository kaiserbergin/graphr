using System;

namespace Graphr.Neo4j.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NeoProjectionAttribute : Attribute
    {
        public string ProjectionName { get; set; }
        public string ProjectionKey { get; set; }
        public string MatchOn { get; set; }

        public NeoProjectionAttribute(string projectionName, string projectionKey = null, string matchOn = null)
        {
            ProjectionName = projectionName;
            ProjectionKey = projectionKey;
            MatchOn = matchOn;
        }
    }
}