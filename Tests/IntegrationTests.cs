using Api.Filmes.Infra;
using Api.Filmes.Interfaces;
using Api.Filmes.Model;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Api.Filmes.Tests;

//public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
//{
//    private readonly HttpClient _client;
//    public IntegrationTests(WebApplicationFactory<Program> factory)
//    {
//        _client = factory.CreateClient();
//    }

//    [Fact]
//    public async Task TestGetProducers()
//    {
//        var response = await _client.GetAsync("/producers"); response.EnsureSuccessStatusCode();
//        var result = await response.Content.ReadFromJsonAsync<object>();
//        Assert.NotNull(result);
//    }
//}
public class IntegrationTests
{
    [Fact]
    public void StartProcessSaveData_ShouldPopulateDatabase_WhenCsvIsValid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MoviesContext>()
            .UseInMemoryDatabase(databaseName: "MoviesTestDb")
            .Options;

        var mockCsvReaderService = new Mock<ICsvReaderService>();

        // Simular os dados lidos do CSV
        var mockMovies = new List<Movie>
        {
            new(1,1982,"Filme1","Studio1","Producer1",true),
            new(1,1982,"Filme2","Studio2","Producer2",true),
            new(1,1982,"Filme3","Studio3","Producer3",true),
            new(1,1982,"Filme4","Studio4","Producer4",true)
        };

        mockCsvReaderService.Setup(service => service.ReadCsvFile(It.IsAny<string>()))
            .Returns(mockMovies);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICsvReaderService)))
            .Returns(mockCsvReaderService.Object);

        using var dbContext = new MoviesContext(options);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(MoviesContext)))
            .Returns(dbContext);

        var appMock = new Mock<WebApplication>();
        appMock.Setup(app => app.Services.CreateScope())
            .Returns(new Mock<IServiceScope>().Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        // Act
        StartProcessSaveData(appMock.Object);

        // Assert
        var moviesInDb = dbContext.Movies.ToList();
        Assert.NotEmpty(moviesInDb);
        Assert.Equal(3, moviesInDb.Count);
        Assert.Contains(moviesInDb, m => m.Title == "Movie 1" && m.Winner == true);
    }

    private void StartProcessSaveData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "File", "movielist.csv");
        var readService = scope.ServiceProvider.GetRequiredService<ICsvReaderService>();
        var moviesParsed = readService.ReadCsvFile(csvFilePath);
        var dbContext = scope.ServiceProvider.GetRequiredService<MoviesContext>();
        dbContext.Database.EnsureCreated();
        dbContext.Movies.AddRange(moviesParsed);
        dbContext.SaveChanges();
    }
}