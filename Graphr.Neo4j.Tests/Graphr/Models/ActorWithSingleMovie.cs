using Graphr.Neo4j.Attributes;
using TrialsOfNeo;

namespace Graphr.Tests.Graphr.Models
{
    public class ActorWithSingleMovie : Actor
    {
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
        public Movie Movie { get; set; }
    }
}