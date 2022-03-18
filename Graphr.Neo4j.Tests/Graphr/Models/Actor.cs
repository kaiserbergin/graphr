using System.Collections.Generic;
using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    [NeoNode("Person")]
    public class Actor 
    {
        [NeoLabels] public IEnumerable<string> Labels { get; set; }
        [NeoProperty("name")] public string Name { get; set; }
        [NeoProperty("born")] public long Born { get; set; }
    }
}