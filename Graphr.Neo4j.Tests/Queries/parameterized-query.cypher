MATCH (a:Person)-[r:ACTED_IN]->(m:Movie)
WHERE a.name = $name
RETURN a, r, m LIMIT 5