namespace Music.Library.Shared;

public class LibrarySongDTO
{
    public int Id { get; set; }
    public int LibraryId { get; set; }
    public required string SongId { get; set; }
    public DateTime DataAggiunta { get; set; }
}