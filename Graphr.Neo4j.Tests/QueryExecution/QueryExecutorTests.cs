using System;
using System.Collections.Generic;
using System.IO;
using Graphr.Neo4j.QueryExecution;
using Graphr.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Xunit;

namespace Graphr.Tests.QueryExecution
{
    [Collection("ServiceProvider")]
    public class QueryExecutorTests
    {
        private readonly IQueryExecutor _queryExecutor;
        private readonly string _nonParameterizedQuery;
        private readonly string _parameterizedQuery;

        public QueryExecutorTests(ServiceProviderFixture serviceProviderFixture)
        {
            _queryExecutor = serviceProviderFixture
                .ServiceProvider
                .GetRequiredService<IQueryExecutor>();

            _nonParameterizedQuery = File.ReadAllText(@"Queries\one-to-one.cypher");
            _parameterizedQuery = File.ReadAllText(@"Queries\parameterized-query.cypher");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsync_StringQuery_ReturnsIRecords()
        {
            // Arrange
            const int expectedRecordCount = 5;

            // Act
            var records = await _queryExecutor.ReadAsync(_nonParameterizedQuery);

            // Assert
            Assert.Equal(expectedRecordCount, records.Count);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsync_QueryWithParamsAsObject_ReturnsIRecords()
        {
            // Arrange
            const int expectedRecordCount = 5;
            var parameters = new { name = "Tom Hanks" };

            // Act
            var records = await _queryExecutor.ReadAsync(_parameterizedQuery, parameters);

            // Assert
            Assert.Equal(expectedRecordCount, records.Count);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsync_QueryWithParamsAsIDictionary_ReturnsIRecords()
        {
            // Arrange
            const int expectedRecordCount = 5;
            var parameters = new Dictionary<string, object> { { "name", "Tom Hanks" } };

            // Act
            var records = await _queryExecutor.ReadAsync(_parameterizedQuery, parameters);

            // Assert
            Assert.Equal(expectedRecordCount, records.Count);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsync_Query_ReturnsIRecords()
        {
            // Arrange
            const int expectedRecordCount = 5;
            var parameters = new { name = "Tom Hanks" };
            var query = new Query(_parameterizedQuery, parameters);

            // Act
            var records = await _queryExecutor.ReadAsync(query);

            // Assert
            Assert.Equal(expectedRecordCount, records.Count);
        }
    }
}