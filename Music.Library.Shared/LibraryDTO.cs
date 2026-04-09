namespace Music.Library.Shared;

public class LibraryDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Nome { get; set; }
}