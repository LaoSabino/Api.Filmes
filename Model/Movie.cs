namespace Api.Filmes.Model;

public record Movie(int Id,
    int Year,
    string Title,
    string Studio,
    string Producers,
    bool Winner);
