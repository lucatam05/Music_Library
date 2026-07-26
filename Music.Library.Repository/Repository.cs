using Microsoft.EntityFrameworkCore;
using Music.Library.Shared.Exceptions;
using Music.Library.Repository.Abstractions;
using Music.Library.Repository.Model;

namespace Music.Library.Repository;

public class Repository(LibraryDbContext libraryDbContext) : IRepository
{
    public async Task<Libraries?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await libraryDbContext.LibrariesEnumerable.FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);
    }

    public async Task AddSongToLibraryAsync(int libraryId, string songId, OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        LibrarySongs song = new LibrarySongs
        {
            LibraryId = libraryId,
            SongId = songId,
            DataAggiunta = DateTime.Now
        };

        libraryDbContext.Add(song);
        libraryDbContext.Add(outboxMessage);
        // Un'unica SaveChangesAsync = un'unica transazione: la canzone e l'evento outbox
        // vengono scritti insieme, oppure nessuno dei due (nessun disallineamento possibile).
        await libraryDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveSongFromLibraryAsync(int libraryId, string songId, OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        LibrarySongs? song = await libraryDbContext.LibrarySongsEnumerable.
                FirstOrDefaultAsync(l => l.LibraryId == libraryId && l.SongId == songId, cancellationToken);
        if (song is null)
            throw new ModelNotFoundException("Canzone non presente nella libreria!");
        
        libraryDbContext.Remove(song);
        libraryDbContext.Add(outboxMessage);
        await libraryDbContext.SaveChangesAsync(cancellationToken);
        
    }

    public async Task CreateLibraryAsync(int userId, CancellationToken cancellationToken)
    {
        Libraries library = new Libraries
        {
            UserId = userId,
            Nome = "TEMPORARY NAME"
        };

        libraryDbContext.LibrariesEnumerable.Add(library);
        await libraryDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RenameLibraryAsync(int userId, string nome, CancellationToken cancellationToken)
    {
        Libraries? library = await GetLibraryByUserIdAsync(userId, cancellationToken);
        if (library is null)
            throw new ModelNotFoundException("Libreria non trovata!");
        
        library.Nome = nome;
        
        await libraryDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<LibrarySongs>> GetCanzoniByLibreriaAsync(int libraryId, CancellationToken cancellationToken)
    {
        return await libraryDbContext.LibrarySongsEnumerable.Where(l => l.LibraryId == libraryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ProcessPendingOutboxMessagesAsync(
        int batchSize,
        int maxAttempts,
        Func<OutboxMessage, CancellationToken, Task<bool>> publishAsync,
        CancellationToken cancellationToken)
    {
        // La transazione resta aperta per tutta la durata del batch: il lock di riga (FOR UPDATE SKIP LOCKED)
        // garantisce che, con più istanze del servizio in esecuzione, nessun messaggio venga preso in carico
        // da due poller contemporaneamente.
        await using var transaction = await libraryDbContext.Database.BeginTransactionAsync(cancellationToken);

        List<OutboxMessage> messages = await libraryDbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT * FROM "OutboxMessages"
                WHERE "Status" = {(int)OutboxMessageStatus.Pending}
                ORDER BY "CreatedAt" ASC
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (OutboxMessage message in messages)
        {
            bool published;
            try
            {
                published = await publishAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                published = false;
                message.LastError = ex.Message;
            }

            if (published)
            {
                message.Status = OutboxMessageStatus.Processed;
                message.ProcessedAt = DateTime.UtcNow;
            }
            else
            {
                message.Attempts++;
                // Superato il numero massimo di tentativi: il messaggio diventa "Failed" (dead-letter)
                // ed esce dal ciclo di retry automatico, evitando che blocchi indefinitamente il poller.
                if (message.Attempts >= maxAttempts)
                {
                    message.Status = OutboxMessageStatus.Failed;
                }
            }
        }

        await libraryDbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages.Count;
    }

    public async Task<int> DeleteProcessedOutboxMessagesOlderThanAsync(DateTime olderThanUtc, CancellationToken cancellationToken)
    {
        return await libraryDbContext.OutboxMessages
            .Where(o => o.Status == OutboxMessageStatus.Processed && o.ProcessedAt != null && o.ProcessedAt < olderThanUtc)
            .ExecuteDeleteAsync(cancellationToken);
    }
}