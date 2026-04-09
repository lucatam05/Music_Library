using Microsoft.EntityFrameworkCore;
using Music.User.Repository.Abstractions;
using Music.User.Repository.Model;

namespace Music.User.Repository;

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

    public async Task RemoveSongToLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken)
    {
        LibrarySongs? song = await libraryDbContext.LibrarySongsEnumerable.
                FirstOrDefaultAsync(l => l.LibraryId == libraryId && l.SongId == songId, cancellationToken);
        if (song is not null)
        {
            libraryDbContext.Remove(song);
            await libraryDbContext.SaveChangesAsync(cancellationToken);
        }
        //TODO implementare eccezione, SongNotFoundException;
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

    public async Task<List<LibrarySongs>> GetCanzoniByLibreriaAsync(int libraryId, CancellationToken cancellationToken)
    {
        return await libraryDbContext.LibrarySongsEnumerable.Where(l => l.LibraryId == libraryId)
            .ToListAsync(cancellationToken);
    }
}