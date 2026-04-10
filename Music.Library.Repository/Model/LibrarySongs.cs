namespace Music.Library.Repository.Model;

public class LibrarySongs
{
    public int Id { get; set; }
    public int LibraryId { get; set; }
    public required string SongId { get; set; }
    public DateTime DataAggiunta { get; set; }
}