using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    [NeoNode("Person")]
    public class Actor 
    {
        [NeoProperty("name")] public string Name { get; set; }
        [NeoProperty("born")] public long Born { get; set; }
    }
}