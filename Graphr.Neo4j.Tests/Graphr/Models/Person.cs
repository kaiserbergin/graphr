using System.Collections.Generic;
using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    [NeoNode("Person")]
    public class Person
    {
        [NeoProperty("name")] public string Name { get; set; }
        [NeoProperty("born")] public long Born { get; set; }
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
        public IEnumerable<MovieWithRelationships> Movies { get; set; }
        [NeoRelationship(type: "FOLLOWS", direction: RelationshipDirection.Outgoing)]
        public IEnumerable<Person> Followings { get; set; }
        [NeoRelationship(type: "FOLLOWS", direction: RelationshipDirection.Incoming)]
        public IEnumerable<Person> Followers { get; set; }
        [NeoRelationship(type: "REVIEWED", direction: RelationshipDirection.Outgoing)]
        public IEnumerable<MovieWithRelationships> Reviews { get; set; }
    }
}