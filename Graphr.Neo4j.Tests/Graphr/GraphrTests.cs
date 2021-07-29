using System.Collections.Generic;
using System.IO;
using System.Linq;
using Graphr.Neo4j.Graphr;
using Graphr.Tests.Fixtures;
using Graphr.Tests.Graphr.Models;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Xunit;

namespace Graphr.Tests.Graphr
{
    [Collection("ServiceProvider")]
    public class GraphrTests
    {
        private readonly INeoGraphr _neoGraphr;
        private readonly string _nonParameterizedQuery;
        private readonly string _parameterizedQuery;
        private readonly string _oneToManyQuery;
        private readonly string _actorToMovieToReviewerToFollowerQuery;

        public GraphrTests(ServiceProviderFixture serviceProviderFixture)
        {
            _neoGraphr = serviceProviderFixture
                .ServiceProvider
                .GetRequiredService<INeoGraphr>();

            _nonParameterizedQuery = File.ReadAllText(@"Queries\one-to-one.cypher");
            _parameterizedQuery = File.ReadAllText(@"Queries\parameterized-query.cypher");
            _oneToManyQuery = File.ReadAllText(@"Queries\one-to-many.cypher");
            _actorToMovieToReviewerToFollowerQuery = File.ReadAllText(@"Queries\actor-movie-reviewer-follower.cypher");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_StringQuery_ReturnsActorsWithSingleMovie()
        {
            // Arrange
            const int expectedActorCount = 5;
            const int expectedMovieCount = 5;

            // Act
            var actorsWithSingleMovie = await _neoGraphr.ReadAsAsync<ActorWithSingleMovie>(_nonParameterizedQuery);

            // Assert
            Assert.Equal(expectedActorCount, actorsWithSingleMovie.Count);
            Assert.Equal(expectedMovieCount, actorsWithSingleMovie.Select(x => x.Movie).Count());
            Assert.All(
                actorsWithSingleMovie,
                actor => Assert.True(
                    actor.Name != null
                    && actor.Movie?.Title != null 
                    && actor.Movie.Description != null));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_QueryWithParamsAsObject_ReturnsActorsWithSingleMovie()
        {
            // Arrange
            const int expectedActorCount = 5;
            const int expectedMovieCount = 5;
            var parameters = new { name = "Tom Hanks" };

            // Act
            var actorsWithSingleMovie = await _neoGraphr.ReadAsAsync<ActorWithSingleMovie>(_parameterizedQuery, parameters);

            // Assert
            Assert.Equal(expectedActorCount, actorsWithSingleMovie.Count);
            Assert.Equal(expectedMovieCount, actorsWithSingleMovie.Select(x => x.Movie).Count());
            Assert.All(
                actorsWithSingleMovie,
                actor => Assert.True(
                    actor.Name != null
                    && actor.Movie?.Title != null 
                    && actor.Movie.Description != null));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_QueryWithParamsAsIDictionary_ReturnsActorsWithSingleMovie()
        {
            // Arrange
            const int expectedActorCount = 5;
            const int expectedMovieCount = 5;
            var parameters = new Dictionary<string, object> { { "name", "Tom Hanks" } };

            // Act
            var actorsWithSingleMovie = await _neoGraphr.ReadAsAsync<ActorWithSingleMovie>(_parameterizedQuery, parameters);

            // Assert
            Assert.Equal(expectedActorCount, actorsWithSingleMovie.Count);
            Assert.Equal(expectedMovieCount, actorsWithSingleMovie.Select(x => x.Movie).Count());
            Assert.All(
                actorsWithSingleMovie,
                actor => Assert.True(
                    actor.Name != null
                    && actor.Movie?.Title != null 
                    && actor.Movie.Description != null));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_Query_ReturnsActorsWithSingleMovie()
        {
            // Arrange
            const int expectedActorCount = 5;
            const int expectedMovieCount = 5;
            var parameters = new { name = "Tom Hanks" };
            var query = new Query(_parameterizedQuery, parameters);

            // Act
            var actorsWithSingleMovie = await _neoGraphr.ReadAsAsync<ActorWithSingleMovie>(query);

            // Assert
            Assert.Equal(expectedActorCount, actorsWithSingleMovie.Count);
            Assert.Equal(expectedMovieCount, actorsWithSingleMovie.Select(x => x.Movie).Count());
            Assert.All(
                actorsWithSingleMovie,
                actor => Assert.True(
                    actor.Name != null
                    && actor.Movie?.Title != null 
                    && actor.Movie.Description != null));
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_CollectedRelatedNodes_ReturnsActorWithMultipleMovies()
        {
            // Arrange
            const int expectedMovieCount = 13;
            var query = new Query(_oneToManyQuery);

            // Act
            var actorWithMovies = await _neoGraphr.ReadAsAsync<ActorWithMovies>(query);

            // Assert
            Assert.Single(actorWithMovies);
            Assert.Equal(expectedMovieCount, actorWithMovies.SelectMany(actor => actor.Movie).Count());
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_CircularReferences_ReturnsRepeatedNodesWithoutInfiniteRelationshipLoops()
        {
            // Arrange
            const int expectedMovieCount = 13;
            var query = new Query(_actorToMovieToReviewerToFollowerQuery);

            // Act
            var actorWithMovies = await _neoGraphr.ReadAsAsync<Person>(query);

            // Assert
            Assert.Single(actorWithMovies);
        }
    }
}