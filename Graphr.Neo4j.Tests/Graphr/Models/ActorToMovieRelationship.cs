using System.Collections.Generic;
using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    [NeoRelationshipEntity]
    public class ActorToMovieRelationship
    {
        [NeoProperty("roles")]
        public IEnumerable<string> Roles { get; set; }

        [NeoTargetNode]
        public Movie Movie { get; set; }
    }
}