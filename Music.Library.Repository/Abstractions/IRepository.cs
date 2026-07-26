using Music.Library.Repository.Model;

namespace Music.Library.Repository.Abstractions;

public interface IRepository
{
    public Task<Libraries?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Aggiunge la canzone alla libreria e il relativo evento all'outbox nella STESSA transazione DB,
    /// così scrittura di dominio e pubblicazione dell'evento sono atomiche per costruzione.
    /// </summary>
    public Task AddSongToLibraryAsync(int libraryId, string songId, OutboxMessage outboxMessage, CancellationToken cancellationToken);

    /// <summary>
    /// Rimuove la canzone dalla libreria e aggiunge il relativo evento all'outbox nella STESSA transazione DB.
    /// </summary>
    public Task RemoveSongFromLibraryAsync(int libraryId, string songId, OutboxMessage outboxMessage, CancellationToken cancellationToken);

    public Task CreateLibraryAsync(int userId, CancellationToken cancellationToken);
    public Task RenameLibraryAsync(int userId, string nome, CancellationToken cancellationToken);
    public Task<List<LibrarySongs>> GetCanzoniByLibreriaAsync(int libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Seleziona un batch di messaggi outbox non ancora pubblicati (con lock a livello di riga
    /// tramite FOR UPDATE SKIP LOCKED, per essere sicuri anche con più istanze del servizio),
    /// e per ciascuno invoca <paramref name="publishAsync"/>. In base al risultato marca il messaggio
    /// come Processed, oppure incrementa i tentativi e lo marca Failed se supera <paramref name="maxAttempts"/>.
    /// </summary>
    /// <returns>Numero di messaggi presi in carico in questo batch</returns>
    public Task<int> ProcessPendingOutboxMessagesAsync(
        int batchSize,
        int maxAttempts,
        Func<OutboxMessage, CancellationToken, Task<bool>> publishAsync,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cancella i messaggi outbox già pubblicati con successo prima di <paramref name="olderThanUtc"/> (retention/cleanup).
    /// </summary>
    /// <returns>Numero di righe cancellate</returns>
    public Task<int> DeleteProcessedOutboxMessagesOlderThanAsync(DateTime olderThanUtc, CancellationToken cancellationToken);
}