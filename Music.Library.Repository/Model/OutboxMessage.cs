namespace Music.Library.Repository.Model;

public class OutboxMessage
{
    public int Id { get; set; }

    /// <summary>
    /// Topic Kafka di destinazione (es. "song-added-to-library")
    /// </summary>
    public required string Topic { get; set; }

    /// <summary>
    /// Chiave del messaggio Kafka (oggi: UserId come stringa)
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Evento serializzato in JSON (es. SongAddedEvent, con CorrelationId già incluso)
    /// </summary>
    public required string Payload { get; set; }

    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;

    /// <summary>
    /// Numero di tentativi di pubblicazione falliti finora
    /// </summary>
    public int Attempts { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Valorizzato quando il messaggio viene pubblicato con successo su Kafka
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Ultimo errore riscontrato durante un tentativo di pubblicazione, per debug
    /// </summary>
    public string? LastError { get; set; }
}
