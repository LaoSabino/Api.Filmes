using System.Globalization;
using Api.Filmes.Interfaces;
using Api.Filmes.Model;

namespace Api.Filmes.Infra;

public class CsvReaderService : ICsvReaderService
{
    public List<Movie> ReadCsvFile(string filePath)
    {
        var movies = new List<Movie>();
        using var reader = new StreamReader(filePath);
        string line;
        reader.ReadLine();
        while ((line = reader.ReadLine()) != null)
        {
            var values = line.Split(';');
            var movieParsed = new Movie(0,
               int.Parse(values[0]),
                values[1],
                 values[2],
                  values[3],
                 IsWinner(values));
            movies.Add(movieParsed);
        }
        return movies;
    }

    private static bool IsWinner(string[] values)
    {
        var winner = values[4];
        return !string.IsNullOrWhiteSpace(winner) && winner.Equals("yes");
    }
}