using Api.Filmes.Model;
using Microsoft.EntityFrameworkCore;

namespace Api.Filmes.Infra;

public class MoviesContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Movie> Movies { get; set; }
}
