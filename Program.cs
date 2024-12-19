using Api.Filmes.Infra;
using Api.Filmes.Interfaces;
using Api.Filmes.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MoviesContext>(options => options.UseInMemoryDatabase("MoviesDb"));
builder.Services.AddSingleton<ICsvReaderService, CsvReaderService>();
builder.Services.AddScoped<IMovieService, MovieService>();
var app = builder.Build();

StartProcessSaveData(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapGet("/producers", async (IMovieService service) =>
{
    var result = await service.GetRangewAwardsAsync();
    return Results.Ok(result);
})
    .WithName("GetProducers").WithDescription("Intervalo de prêmios.")
    .Produces(StatusCodes.Status200OK);

app.Run();

static void StartProcessSaveData(WebApplication app)
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