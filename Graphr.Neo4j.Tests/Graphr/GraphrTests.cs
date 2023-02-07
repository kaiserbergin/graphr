using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Graphr.Neo4j.Configuration;
using Graphr.Neo4j.Driver;
using Graphr.Neo4j.Graphr;
using Graphr.Neo4j.QueryExecution;
using Graphr.Tests.Fixtures;
using Graphr.Tests.Graphr.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using VerifyXunit;
using Xunit;

namespace Graphr.Tests.Graphr
{
    [UsesVerify]
    [Collection("ServiceProvider")]
    public class GraphrTests : IAsyncLifetime
    {
        private readonly ServiceProviderFixture _serviceProviderFixture;
        private readonly INeoGraphr _neoGraphr;
        
        private readonly string _nonParameterizedQuery;
        private readonly string _parameterizedQuery;
        private readonly string _oneToManyQuery;
        private readonly string _actorToMovieToReviewerToFollowerQuery;
        private readonly string _createMovieQuery;
        private readonly string _createMovieParameterizedQuery;
        private readonly string _deleteCreatedMovieQuery;
        private readonly string _getCreatedMovieQuery;
        private readonly string _createNodeForTypeTestingQuery;
        private readonly string _retrieveNodeForTypeTestingQuery;
        private readonly string _deleteNodeForTypeTestingQuery;
        private readonly string _initiateGraphQuery;
        private readonly string _cleanupGraphQuery;
        private readonly string _actorToMovieWithProjectionsQuery;

        private const string DATABASE_NAME = "neo4j";
        private const string TEST_MOVIE_TITLE = "Test Movie";

        public GraphrTests(ServiceProviderFixture serviceProviderFixture)
        {
            _serviceProviderFixture = serviceProviderFixture;
            
            _neoGraphr = serviceProviderFixture
                .ServiceProvider
                .GetRequiredService<INeoGraphr>();
            
            _nonParameterizedQuery = File.ReadAllText(@"Queries/one-to-one.cypher");
            _parameterizedQuery = File.ReadAllText(@"Queries/parameterized-query.cypher");
            _oneToManyQuery = File.ReadAllText(@"Queries/one-to-many.cypher");
            _actorToMovieToReviewerToFollowerQuery = File.ReadAllText(@"Queries/actor-movie-reviewer-follower.cypher");
            _actorToMovieWithProjectionsQuery = File.ReadAllText(@"Queries/map-projection.cypher");
            _createMovieQuery = File.ReadAllText(@"Queries/create-movie.cypher");
            _createMovieParameterizedQuery = File.ReadAllText(@"Queries/create-movie-parameterized.cypher");
            _deleteCreatedMovieQuery = File.ReadAllText(@"Queries/delete-movie.cypher");
            _getCreatedMovieQuery = File.ReadAllText(@"Queries/get-created-movie.cypher");
            _createNodeForTypeTestingQuery = File.ReadAllText(@"Queries/create-node-for-type-testing.cypher");
            _retrieveNodeForTypeTestingQuery = File.ReadAllText(@"Queries/retrieve-type-test-node.cypher");
            _deleteNodeForTypeTestingQuery = File.ReadAllText(@"Queries/delete-node-for-type-testing.cypher");
            _initiateGraphQuery = File.ReadAllText(@"Queries/play-movies.cypher");
            _cleanupGraphQuery = File.ReadAllText(@"Queries/cleanup.cypher");
        }

        public async Task InitializeAsync()
        {
            await _neoGraphr.WriteAsync(_cleanupGraphQuery);
            await _neoGraphr.WriteAsync(_initiateGraphQuery);
        }

        public async Task DisposeAsync() => await _neoGraphr.WriteAsync(_cleanupGraphQuery);

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
        public async void ReadAsync_NodeWithAllSupportedTypes_ReturnsProperlyMappedClass()
        {
            // Arrange
            await _neoGraphr.WriteAsync(_deleteNodeForTypeTestingQuery);
            await _neoGraphr.WriteAsync(_createNodeForTypeTestingQuery);
            
            var query = new Query(_retrieveNodeForTypeTestingQuery);

            // Actssert
            var idonttrustme = await _neoGraphr.ReadAsAsync<ValueTypesNode>(query);
            
            await _neoGraphr.WriteAsync(_deleteNodeForTypeTestingQuery);
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

        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAndWriteAsync_WithDatabaseNameFromSettings_ExecutesProperly()
        {
            // Arrange
            var serviceProvider = _serviceProviderFixture.ServiceProvider;

            var driverProvider = serviceProvider.GetRequiredService<IDriverProvider>();
            var settings = serviceProvider.GetRequiredService<IOptions<NeoDriverConfigurationSettings>>();

            settings.Value.DatabaseName = DATABASE_NAME;

            var queryExecutor = new QueryExecutor(driverProvider, settings.Value);
            var neoGraphr = new NeoGraphr(queryExecutor);

            var parameters = new { title = TEST_MOVIE_TITLE };
            var query = new Query(_createMovieParameterizedQuery, parameters);
            
            // Act
            await neoGraphr.WriteAsync(query);
            var movies = await neoGraphr.ReadAsAsync<Movie>(_getCreatedMovieQuery);

            // Assert
            Assert.Single(movies);
            Assert.Equal(TEST_MOVIE_TITLE, movies.Single().Title); 
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAndWriteAsync_WithSessionConfigBuilder_ExecutesProperly()
        {
            // Arrange
            var parameters = new { title = TEST_MOVIE_TITLE };
            var query = new Query(_createMovieParameterizedQuery, parameters);

            var configurationAction = new Action<SessionConfigBuilder>(config => config.WithDatabase(DATABASE_NAME));
            
            // Act
            await _neoGraphr
                .WithSessionConfig(configurationAction)
                .WriteAsync(query);
            var movies = await _neoGraphr
                .WithSessionConfig(configurationAction)
                .ReadAsAsync<Movie>(_getCreatedMovieQuery);

            // Assert
            Assert.Single(movies);
            Assert.Equal(TEST_MOVIE_TITLE, movies.Single().Title); 
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async void ReadAsync_WithImproperSessionConfigBuilder_ThrowsException()
        {
            // Arrange
            var configurationAction = new Action<SessionConfigBuilder>(config => config.WithDatabase("break"));
            
            // Act
            Func<Task<List<IRecord>>> act = async () => await _neoGraphr
                .WithSessionConfig(configurationAction)
                .WriteAsync(_createMovieQuery);

            // Assert
            await act.Should().ThrowAsync<FatalDiscoveryException>();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ReadAsync_WithMapProjections_SerializesSuccessfully()
        {
            // Act
            var actorWithMovieProjection = await _neoGraphr.ReadAsAsync<ActorWithMoviesAndProjection>(_actorToMovieWithProjectionsQuery);
            
            // Assert
            var keanu = actorWithMovieProjection.First();

            keanu.Name.Should().Be("Keanu Reeves");
            keanu.Born.Should().Be(1964);
            keanu.Feels.Feels.Should().Be("so many");
            keanu.Labels.Single().Should().Be("Person");
            keanu.Surprise.Should().Be(1);
            
            keanu.Dictionary.Count().Should().Be(2);
            keanu.Dictionary["one"].Should().Be("two");
            
            keanu.ObjectionableDictionary.Count.Should().Be(3);
            keanu.ObjectionableDictionary["two"].Should().Be(2);
            
            keanu.Movie.Count().Should().Be(7);

            var movie = keanu.Movie.First(m => m.Title == "Something's Gotta Give");

            movie.Description.Should().BeNull();
            movie.Released.Should().Be(2003);
            movie.Staff.Actors.Count().Should().Be(3);
            movie.Staff.Directors.Single().Name.Should().Be("Nancy Meyers");
            movie.Staff.Directors.Single().Born.Should().Be(1949);
            movie.Staff.Nested.Example.Should().Be("data");
        }
    }
}