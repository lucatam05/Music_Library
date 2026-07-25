namespace Music.Library.Shared.Events;

public class SongAddedEvent
{
    public int UserId { get; set; }
    public required string SpotifyId { get; set; }

    /// <summary>
    /// CorrelationId della richiesta HTTP che ha originato l'evento, propagato
    /// così i log del consumer (UserService) restano correlabili a quelli del producer.
    /// </summary>
    public string? CorrelationId { get; set; }
}

public class SongRemovedEvent
{
    public int UserId { get; set; }
    public required string SpotifyId { get; set; }

    /// <summary>
    /// CorrelationId della richiesta HTTP che ha originato l'evento, propagato
    /// così i log del consumer (UserService) restano correlabili a quelli del producer.
    /// </summary>
    public string? CorrelationId { get; set; }
}