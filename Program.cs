using Api.Filmes.Infra;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MoviesContext>(options => options.UseInMemoryDatabase("MoviesDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var moviesParsed = CsvReaderService.ReadCsvFile(@"C:\\Users\laerc\source\repos\Api.Filmes\File\movielist.csv");
    var dbContext = scope.ServiceProvider.GetRequiredService<MoviesContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Movies.AddRange(moviesParsed);
    dbContext.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/producers", async (MoviesContext db) =>
{
    // var movies = await db.Movies.ToListAsync();
    var winners = await db.Movies.Where(m => m.Winner).ToListAsync();

    var producerAwards = winners.SelectMany(m => m.Producers.Split(',').Select(p =>
    new { Producer = p.Trim(), m.Year })).GroupBy(pa => pa.Producer).Where(g => g.Count() > 1).Select(g => new
    {
        Producer = g.Key,
        Intervals = g.OrderBy(pa => pa.Year).Select(pa => pa.Year).Zip(g.OrderBy(pa => pa.Year).Select(pa => pa.Year).Skip(1), (a, b) => new { Interval = b - a, PreviousWin = a, FollowingWin = b })
    }).SelectMany(p => p.Intervals.Select(i => new
    { p.Producer, i.Interval, i.PreviousWin, i.FollowingWin })).ToList();

    var minInterval = producerAwards.Where(p => p.Interval > 0).OrderBy(p => p.Interval).FirstOrDefault();

    var maxInterval = producerAwards.OrderByDescending(p => p.Interval).FirstOrDefault(); var result = new
    {
        Min = producerAwards.Where(p => p.Interval == minInterval?.Interval).Select(p => new { p.Producer, p.Interval, p.PreviousWin, p.FollowingWin }).ToList(),
        Max = producerAwards.Where(p => p.Interval == maxInterval?.Interval).Select(p => new { p.Producer, p.Interval, p.PreviousWin, p.FollowingWin }).ToList()
    };

    return Results.Ok(result);
})
    .WithName("GetProducers")
    .Produces(StatusCodes.Status200OK);

app.Run();


