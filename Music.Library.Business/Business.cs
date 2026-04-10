using Microsoft.Extensions.Configuration;
using Music.Catalogue.ClientHttp.Abstractions;
using Music.Catalogue.Shared;
using Music.Catalogue.Shared.Exceptions;
using Music.Library.Business.Abstractions;
using Music.Library.Shared;


namespace Music.Library.Business;

public class Business(IRepository repository, IClientHttp clientHttp) : IBusiness
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

        SongDTO? song = await clientHttp.SearchCanzoniByIDSpotify(songId, cancellationToken);
        if (song is null)
            throw new ModelNotFoundException("Canzone non trovata");
        
        await repository.AddSongToLibraryAsync(library.Id, songId, cancellationToken);

        // 4. TODO: pubblica evento Kafka
    }

    public async Task RemoveSongToLibraryAsync(int userId, string songId, CancellationToken cancellationToken)
    {
        Libraries? library = await repository.GetLibraryByUserIdAsync(userId, cancellationToken);
        if (library is null)
            throw new ModelNotFoundException("Libreria non trovata");
        
        await repository.RemoveSongFromLibraryAsync(library.Id, songId, cancellationToken);
    }

    public async Task CreateLibraryAsync(int userId, CancellationToken cancellationToken)
    {
        await repository.CreateLibraryAsync(userId, cancellationToken);
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
            SongDTO? song = await clientHttp.SearchCanzoniByIDSpotify(canzone.SongId, cancellationToken);
            if (song is not null)
            {
                result.Add(new LibrarySongDTO
                {
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