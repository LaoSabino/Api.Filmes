using Api.Filmes.Model;

namespace Api.Filmes.Interfaces;

public interface ICsvReaderService
{
     List<Movie> ReadCsvFile(string filePath);
}
