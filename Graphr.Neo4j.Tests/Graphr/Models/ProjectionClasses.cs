using System.Collections.Generic;
using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    
    [NeoProjectedEntity]
    public class FeelsProjection
    {
        [NeoProperty("feels")]
        public string Feels { get; set; }
    }
    
    [NeoProjectedEntity]
    public class StaffProjection
    {
        [NeoProjection("actors")]
        public IEnumerable<ProjectedPerson> Actors { get; set; } 
        
        [NeoProjection("directors")]
        public ProjectedPerson[] Directors { get; set; }
        
        [NeoProjection("missing")]
        public IEnumerable<ProjectedPerson> MissingPersons { get; set; }
        
        [NeoProjection("null")]
        public IEnumerable<ProjectedPerson> NullPersons { get; set; }
        
        [NeoProjection("nested")]
        public ProjectedNested Nested { get; set; }
    }

    [NeoProjectedEntity]
    public class ProjectedNested
    {
        [NeoProperty("example")]
        public string Example { get; set; }
    }

    [NeoProjectedEntity]
    public class ProjectedPerson
    {
        [NeoProperty("born")]
        public long Born { get; set; }
        
        [NeoProperty("name")]
        public string Name { get; set; }
    }
}