MATCH (a:Person {name: 'Tom Hanks'})-[r:ACTED_IN]->(m:Movie)
RETURN a, r, m LIMIT 5