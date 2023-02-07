using System.Collections.Generic;
using Graphr.Neo4j.Attributes;

namespace Graphr.Tests.Graphr.Models
{
    [NeoNode("Movie")]
    public class MovieWithProjections : Movie
    {
        [NeoProjection(projectionName: "staff", projectionKey: "gKey", matchOn: nameof(Movie.Title))]
        public StaffProjection Staff { get; set; }
    }
}