using System.Collections.Generic;
using Graphr.Neo4j.Attributes;
using TrialsOfNeo;

namespace Graphr.Tests.Graphr.Models
{
    public class ActorWithMovies : Actor
    {
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
        public IEnumerable<Movie> Movie { get; set; }
    }
}