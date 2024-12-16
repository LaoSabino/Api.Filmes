using Api.Filmes.Infra;
using Api.Filmes.Interfaces;
using Api.Filmes.Model;
using Microsoft.EntityFrameworkCore;

namespace Api.Filmes.Service;

public class MovieService(MoviesContext moviesContext) : IMovieService
{
    public async Task GetRangewAwardsAsync()
    {
        var winners = await moviesContext.Movies.Where(m => m.Winner).ToListAsync();

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

        //var result = new
        //{
        //    Min = producerAwards.Where(p => p.Interval == minInterval?.Interval).Select(p => new { p.Producer, p.Interval, p.PreviousWin, p.FollowingWin }).ToList(),
        //    Max = producerAwards.Where(p => p.Interval == maxInterval?.Interval).Select(p => new { p.Producer, p.Interval, p.PreviousWin, p.FollowingWin }).ToList()
        //};

        //return result;
    }
}
