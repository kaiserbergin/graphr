using System.Collections.Generic;
using Graphr.Neo4j.Attributes;
using Graphr.Neo4j.Translator;

namespace Graphr.Tests.Graphr.Models
{
    public class ActorWithMoviesAndProjection : Actor
    {
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
        public IEnumerable<MovieWithProjections> Movie { get; set; }
        
        [NeoProjection(projectionName: "feels")]
        public FeelsProjection Feels { get; set; }
        
        [NeoProjection(projectionName: "surprise")]
        public long Surprise { get; set; }
        
        [NeoProjection(projectionName: "dictionary")]
        public Dictionary<string, string> Dictionary { get; set; }
        
        [NeoProjection(projectionName: "objectionableDictionary")]
        public Dictionary<string, object> ObjectionableDictionary { get; set; }
    }
}