# Graphr.Neo4j

Graphr.Neo4j is a lightweight ORM for Neo4j that focuses on easy set up and result
mapping.

Cypher queries are incredibly flexible, and this project makes _no attempt_
to write queries for you, as that would be a huge undertaking and probably just rob
you of understanding what the heck you're doing.

## Installation

Simply install via nuget in your favorite IDE (it's Rider), or use the command line.

```powershell
Install-Package Graphr.Neo4j -Version 0.0.2
```

## Usage

### Dependency Injection Setup (if you're into that)
Add a section to your appsettings, environment variables, or whatever else (I'm not
your dad) so you end up with something like this:
```json
{
  "NeoDriverConfigurationSettings": {
    "url": "YOUR_URL_PROBALBY_BOLT:SOMETHING",
    "username": "YOUR_USERNAME",
    "password": "YOUR_PASSWORD",
    "isDebugLoggingEnabled": true,
    "isTraceLoggingEnabled": true
  }
}
```
If you don't know where this goes, you shouldn't be trying to use this yet...
```c#
services.AddNeoGraphr(configuration);
```

### Setting Up Your Entites
Now add some attributes to your beautiful entities!
```c#
[NeoNode("Person")]
public class Actor 
{
    [NeoProperty("name")]
    public string Name { get; set; }
    
    [NeoProperty("born")]
    public long Born { get; set; }
    
    [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
    public IEnumerable<Movie> Movie { get; set; }
}

[NeoNode("Movie")]
public class Movie
{
    [NeoProperty("tagline")]
    public string Description { get; set; }
    
    [NeoProperty("title")]
    public string Title { get; set; }

    [NeoProperty("released")]
    public long Released { get; set; }
}
```
If you're familiar with Neo4j, then these attributes are pretty self explanatory. If
not, I encourage you to check out Neo4j's documentation or their Graph Academy and
get learnt and then get back here.

### Using Graphr to Query
Now that you've done all the setup, it's time to have some fun! Check out how easy it
is! 

```c#
// It's late, and I'm not testing this example in any way.
public class Neo4jRocks
{
    private readonly INeoGraphr _neoGraphr;
    
    public Neo4jRocks(INeoGraphr neoGraphr)
    {
        _neoGraphr = neoGraphr;
    }
    
    public async Task ReadSomeStuff()
    {
        // You'll need to return the nodes and relationships between them for those
        //  fancy attributes to work.
        var query = "MATCH (a:Person {name:'Tom Hanks'})-[r:ACTED_IN]->(m:Movie) RETURN a, collect(r), collect(m)";
        var actorsWithSingleMovie = await _neoGraphr.ReadAsAsync<Actor>(query);
        
        ... Do Stuff ...
    }
}
```

### Notes on Usage

- You must return relationships between nodes in your queries if your models have 
  relationship attributes
  
- Relationship attributes can be placed on classes that are decorated with the 
  `[NeoNode]` attribute or an `IEnumerable` of a decorated class
  
- Since you're mapping to a class, return the single node that represents that starting
  point first in your query
  
- This library maps relationships for a node only once per DFS chain to avoid circular
  references. The node will still be mapped with it's properties each time, but
  relationships mapped _only once_.
  
## Road Map
Here's where my heads at and where I want to take this
- [x] ~~Simple node mapping (one to one and one to many)~~
- [x] ~~Circular Reference Issues~~
- [x] ~~Basic Dependency Injection Extension~~
- [ ] Mapping of `[NeoRelationshipEntity]` attribute
- [ ] Add `[NeoResult]` attribute to map non-node record responses
- [ ] More configuration options
- [ ] Fix all the bugs I haven't found yet... Or at least the worst ones

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)