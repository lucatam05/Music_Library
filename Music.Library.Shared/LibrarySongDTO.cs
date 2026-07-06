namespace Music.Library.Shared;

public class LibrarySongDTO
{
    public int Id { get; set; }
    public int LibraryId { get; set; }
    public required string SongId { get; set; }
    public required string NomeLibreria { get; set; }
    public DateTime DataAggiunta { get; set; }
    public string? Titolo { get; set; }
    public string? Artista { get; set; }
}