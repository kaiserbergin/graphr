using System.Collections.Generic;
using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    public class ActorWithMovieRelationshipEntities : Actor
    {
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
        public IEnumerable<ActorToMovieRelationship> ActorToMovieRelationships { get; set; }
    }
}