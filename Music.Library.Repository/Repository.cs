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

    public async Task AddSongToLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken)
    {
        LibrarySongs song = new LibrarySongs
        {
            LibraryId = libraryId,
            SongId = songId,
            DataAggiunta = DateTime.Now
        };

        libraryDbContext.Add(song);
        await libraryDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveSongFromLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken)
    {
        LibrarySongs? song = await libraryDbContext.LibrarySongsEnumerable.
                FirstOrDefaultAsync(l => l.LibraryId == libraryId && l.SongId == songId, cancellationToken);
        if (song is null)
            throw new ModelNotFoundException("Canzone non presente nella libreria!");
        
        libraryDbContext.Remove(song);
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
}