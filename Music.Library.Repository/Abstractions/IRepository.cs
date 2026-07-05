using Music.Library.Repository.Model;

namespace Music.Library.Repository.Abstractions;

public interface IRepository
{
    public Task<Libraries?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken);
    public Task AddSongToLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken);
    public Task RemoveSongFromLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken);
    public Task CreateLibraryAsync(int userId, CancellationToken cancellationToken);
    public Task RenameLibraryAsync(int userId, string nome, CancellationToken cancellationToken);
    public Task<List<LibrarySongs>> GetCanzoniByLibreriaAsync(int libraryId, CancellationToken cancellationToken);
}