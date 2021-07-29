MATCH (a:Person {name:'Tom Hanks'})-[r:ACTED_IN]->(m:Movie) 
RETURN a, collect(r), collect(m)