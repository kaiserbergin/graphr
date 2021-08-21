using System;
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
    public class GraphrTests : IDisposable
    {
        private readonly INeoGraphr _neoGraphr;
        private readonly string _nonParameterizedQuery;
        private readonly string _parameterizedQuery;
        private readonly string _oneToManyQuery;
        private readonly string _actorToMovieToReviewerToFollowerQuery;
        private readonly string _createMovieQuery;
        private readonly string _createMovieParameterizedQuery;
        private readonly string _deleteCreatedMovieQuery;
        private readonly string _getCreatedMovieQuery;

        public GraphrTests(ServiceProviderFixture serviceProviderFixture)
        {
            _neoGraphr = serviceProviderFixture
                .ServiceProvider
                .GetRequiredService<INeoGraphr>();

            _nonParameterizedQuery = File.ReadAllText(@"Queries/one-to-one.cypher");
            _parameterizedQuery = File.ReadAllText(@"Queries/parameterized-query.cypher");
            _oneToManyQuery = File.ReadAllText(@"Queries/one-to-many.cypher");
            _actorToMovieToReviewerToFollowerQuery = File.ReadAllText(@"Queries/actor-movie-reviewer-follower.cypher");
            _createMovieQuery = File.ReadAllText(@"Queries/create-movie.cypher");
            _createMovieParameterizedQuery = File.ReadAllText(@"Queries/create-movie-parameterized.cypher");
            _deleteCreatedMovieQuery = File.ReadAllText(@"Queries/delete-movie.cypher");
            _getCreatedMovieQuery = File.ReadAllText(@"Queries/get-created-movie.cypher");
        }

        public void Dispose()
        {
            _neoGraphr.WriteAsync(_deleteCreatedMovieQuery).GetAwaiter().GetResult();
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
            const int expectedMovieCount = 12;
            var query = new Query(_oneToManyQuery);

            // Act
            var actorWithMovies = await _neoGraphr.ReadAsAsync<ActorWithMovies>(query);

            // Assert
            Assert.Single(actorWithMovies);
            Assert.Equal(expectedMovieCount, actorWithMovies.SelectMany(actor => actor.Movie).Count());
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_NodeWithRelationshipEnitityList_ReturnsActorWithMultipleRelationshipsLinkedToSingularMovies()
        {
            // Arrange
            const int expectedMovieCount = 12;
            var query = new Query(_oneToManyQuery);

            // Act
            var actorWithMovies = await _neoGraphr.ReadAsAsync<ActorWithMovieRelationshipEntities>(query);

            // Assert
            Assert.Single(actorWithMovies);
            Assert.Equal(expectedMovieCount, actorWithMovies.SelectMany(actor => actor.ActorToMovieRelationships).Count());
            Assert.True(actorWithMovies.All(actor => actor.ActorToMovieRelationships.All(actorToMovieRelationship => actorToMovieRelationship.Movie != null)));
            Assert.True(actorWithMovies.All(actor => actor.ActorToMovieRelationships.All(actorToMovieRelationship => actorToMovieRelationship.Roles != null)));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsAsync_CircularReferences_ReturnsRepeatedNodesWithoutInfiniteRelationshipLoops()
        {
            // Arrange
            var query = new Query(_actorToMovieToReviewerToFollowerQuery);

            // Act
            var actorWithMovies = await _neoGraphr.ReadAsAsync<Person>(query);

            // Assert
            Assert.Single(actorWithMovies);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsAsync_CreateMovieQueryAsString_ReturnsSingleMovie()
        {
            // Act
            var movies = await _neoGraphr.WriteAsAsync<Movie>(_createMovieQuery);

            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsAsync_CreateMovieQueryWithAnonymousParameters_ReturnsSingleMovie()
        {
            // Arrange
            var parameters = new { title = "Test Movie" };
            
            // Act
            var movies = await _neoGraphr.WriteAsAsync<Movie>(_createMovieParameterizedQuery, parameters);

            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsAsync_CreateMovieQueryWithDictionaryParameters_ReturnsSingleMovie()
        {
            // Arrange
            var parameters = new Dictionary<string, object> { { "title", "Test Movie" } };
            
            // Act
            var movies = await _neoGraphr.WriteAsAsync<Movie>(_createMovieParameterizedQuery, parameters);

            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsAsync_CreateMovieQueryWithQueryClass_ReturnsSingleMovie()
        {
            // Arrange
            var parameters = new { title = "Test Movie" };
            var query = new Query(_createMovieParameterizedQuery, parameters);
            
            // Act
            var movies = await _neoGraphr.WriteAsAsync<Movie>(query);

            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsync_CreateMovieQueryAsString_ReturnsSingleMovie()
        {
            // Act
            await _neoGraphr.WriteAsync(_createMovieQuery);
            var movies = await _neoGraphr.ReadAsAsync<Movie>(_getCreatedMovieQuery);

            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsync_CreateMovieQueryWithAnonymousParameters_ReturnsSingleMovie()
        {
            // Arrange
            var parameters = new { title = "Test Movie" };
            
            // Act
            await _neoGraphr.WriteAsync(_createMovieParameterizedQuery, parameters);
            var movies = await _neoGraphr.ReadAsAsync<Movie>(_getCreatedMovieQuery);
            
            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsync_CreateMovieQueryWithDictionaryParameters_ReturnsSingleMovie()
        {
            // Arrange
            var parameters = new Dictionary<string, object> { { "title", "Test Movie" } };
            
            // Act
            await _neoGraphr.WriteAsync(_createMovieParameterizedQuery, parameters);
            var movies = await _neoGraphr.ReadAsAsync<Movie>(_getCreatedMovieQuery);

            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void WriteAsync_CreateMovieQueryWithQueryClass_ReturnsSingleMovie()
        {
            // Arrange
            var parameters = new { title = "Test Movie" };
            var query = new Query(_createMovieParameterizedQuery, parameters);
            
            // Act
            await _neoGraphr.WriteAsync(query);
            var movies = await _neoGraphr.ReadAsAsync<Movie>(_getCreatedMovieQuery);

            // Assert
            Assert.Single(movies);
            Assert.Equal("Test Movie", movies.Single().Title);
        }
    }
}