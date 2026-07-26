namespace MusicLibrary.Outbox;

public class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>
    /// Numero massimo di messaggi outbox processati per ogni ciclo del poller
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Numero massimo di tentativi di pubblicazione prima di marcare il messaggio come Failed (dead-letter)
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Giorni di retention per i messaggi già pubblicati con successo, dopo i quali vengono cancellati
    /// </summary>
    public int RetentionDays { get; set; } = 7;

    /// <summary>
    /// Il cleanup (cancellazione dei messaggi Processed scaduti) viene eseguito una volta ogni N cicli
    /// del poller, per non fare una DELETE ad ogni singolo ciclo
    /// </summary>
    public int CleanupEveryNCycles { get; set; } = 60;
}
