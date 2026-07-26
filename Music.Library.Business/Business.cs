using System.Text.Json;
using Music.Catalogue.ClientHttp.Abstractions;
using Music.Catalogue.Shared;
using Music.Library.Shared.Exceptions;
using Music.Library.Business.Abstractions;
using Music.Library.Repository.Abstractions;
using Music.Library.Repository.Model;
using Music.Library.Shared;
using Music.Library.Shared.Events;

namespace Music.Library.Business;

public class Business(
    IRepository repository,
    IClientHttp clientHttp,
    ICorrelationIdProvider correlationIdProvider) : IBusiness
{
    public async Task<LibraryDTO?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        Libraries? library = await repository.GetLibraryByUserIdAsync(userId, cancellationToken);
        if (library is null)
            throw new ModelNotFoundException("Libreria non trovata!");
        
        return new LibraryDTO
        {
            Id = library.Id,
            UserId = library.UserId,
            Nome = library.Nome
        };
    }
    
    public async Task AddSongToLibraryAsync(int userId, string songId, CancellationToken cancellationToken) 
    {
        Libraries? library = await repository.GetLibraryByUserIdAsync(userId, cancellationToken);
        if (library is null)
            throw new ModelNotFoundException("Libreria non trovata");

        SongDTO? song = await clientHttp.SearchCanzoniByIdSpotify(songId, cancellationToken);
        if (song is null)
            throw new ModelNotFoundException("Canzone non trovata");
        
        var songAddedEvent = new SongAddedEvent
        {
            UserId = userId,
            SpotifyId = songId,
            CorrelationId = correlationIdProvider.CorrelationId
        };

        var outboxMessage = new OutboxMessage
        {
            Topic = "song-added-to-library",
            Key = userId.ToString(),
            Payload = JsonSerializer.Serialize(songAddedEvent),
            CreatedAt = DateTime.UtcNow
        };

        // Canzone e messaggio outbox vengono scritti nella STESSA transazione DB:
        // la pubblicazione effettiva su Kafka avviene poi in modo asincrono dal poller (LibraryProducerService).
        await repository.AddSongToLibraryAsync(library.Id, songId, outboxMessage, cancellationToken);
    }

    public async Task RemoveSongFromLibraryAsync(int userId, string songId, CancellationToken cancellationToken)
    {
        Libraries? library = await repository.GetLibraryByUserIdAsync(userId, cancellationToken);
        if (library is null)
            throw new ModelNotFoundException("Libreria non trovata");
        
        var songRemovedEvent = new SongRemovedEvent
        {
            UserId = userId,
            SpotifyId = songId,
            CorrelationId = correlationIdProvider.CorrelationId
        };

        var outboxMessage = new OutboxMessage
        {
            Topic = "song-removed-from-library",
            Key = userId.ToString(),
            Payload = JsonSerializer.Serialize(songRemovedEvent),
            CreatedAt = DateTime.UtcNow
        };

        await repository.RemoveSongFromLibraryAsync(library.Id, songId, outboxMessage, cancellationToken);
    }

    public async Task CreateLibraryAsync(int userId, CancellationToken cancellationToken)
    {
        await repository.CreateLibraryAsync(userId, cancellationToken);
    }

    public async Task RenameLibraryAsync(int userId, string nome, CancellationToken cancellationToken)
    {
        await repository.RenameLibraryAsync(userId, nome, cancellationToken);
    }

    public async Task<List<LibrarySongDTO>?> GetCanzoniByLibreriaAsync(int userId, CancellationToken cancellationToken)
    {
        Libraries? library = await repository.GetLibraryByUserIdAsync(userId, cancellationToken);
        if (library is null)
            throw new ModelNotFoundException("Libreria non trovata");

        List<LibrarySongs> canzoni = await repository.GetCanzoniByLibreriaAsync(library.Id, cancellationToken);
    
        List<LibrarySongDTO> result = new List<LibrarySongDTO>();
        foreach (var canzone in canzoni)
        {
            SongDTO? song = await clientHttp.SearchCanzoniByIdSpotify(canzone.SongId, cancellationToken);
            if (song is not null)
            {
                result.Add(new LibrarySongDTO
                {
                    Id = canzone.Id,
                    LibraryId = library.Id,
                    NomeLibreria = library.Nome,
                    SongId = song.SpotifyId,
                    Titolo = song.Titolo,
                    Artista = song.Artista,
                    DataAggiunta = canzone.DataAggiunta
                });
            }
        }
        return result;
    }
}