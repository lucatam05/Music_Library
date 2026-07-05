namespace Music.Library.Shared.Events;

public class SongAddedEvent
{
    public int UserId { get; set; }
    public required string SpotifyId { get; set; }
}

public class SongRemovedEvent
{
    public int UserId { get; set; }
    public required string SpotifyId { get; set; }
}