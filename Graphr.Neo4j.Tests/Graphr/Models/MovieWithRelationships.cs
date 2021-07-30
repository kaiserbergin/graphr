using System.Collections.Generic;
using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    public class MovieWithRelationships : Movie
    {
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Incoming)]
        public IEnumerable<Person> Actors { get; set; }
        [NeoRelationship(type: "REVIEWED", direction: RelationshipDirection.Incoming)]
        public IEnumerable<Person> Reviewers { get; set; }
    }
}