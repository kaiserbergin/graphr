using System.Diagnostics;
using Graphr.Neo4j.Graphr;
using Graphr.Tests.Fixtures;
using Graphr.Tests.Graphr.Models;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;

var spf = new ServiceProviderFixture();
var _neoGraphr = spf
    .ServiceProvider
    .GetRequiredService<INeoGraphr>();

var sw = new Stopwatch();
            
sw.Start();

var taskList = new List<Task>();

for (var i = 0; i < 50; i++)
{
    var queryStr = "CREATE (a:Movie {title: $title, released: 1999}) RETURN (a)";
    var parameters = new { title = i + "th Movie" };
    var query = new Query(queryStr, parameters);
            
    taskList.Add(_neoGraphr.WriteAsAsync<Movie>(query)); 
}

await Task.WhenAll(taskList);
            
sw.Stop();

Console.WriteLine($"Complete after: {sw.ElapsedMilliseconds}");