using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    [NeoNode("Movie")]
    public class Movie
    {
        [NeoProperty("tagline")]
        public string Description { get; set; }
        
        [NeoProperty("title")]
        public string Title { get; set; }

        [NeoProperty("released")]
        public long Released { get; set; }
    }
}