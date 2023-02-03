MATCH (p:Person {name: 'Keanu Reeves'})-[actedIn:ACTED_IN]->(m:Movie)
OPTIONAL MATCH (m)<-[:ACTED_IN]-(p2:Person)
OPTIONAL MATCH (m)<-[:DIRECTED]-(d:Person)
WITH 
  p AS anchor, 
  actedIn, 
  m AS movie, 
  { 
    gKey: m.title,  
    actors: collect(DISTINCT p2{.*}), 
    directors: collect(DISTINCT d{.*}), 
    nested: { example: "data" }
  } AS staff
RETURN 
  anchor, 
  collect(actedIn) AS actingRels, 
  collect(movie) AS movies, 
  collect(staff) AS staff, 
  { gkey: anchor.name, feels: "so many"} AS feels, 
  1 AS surprise, 
  { one: "two", three: "four" } AS dictionary,
  { one: 1, two: 2.0, three: "four" } as objectionableDictionary