using Music.Library.Shared;

namespace Music.Library.Business.Abstractions;

public interface IBusiness
{
    public Task<LibraryDTO?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken);
    public Task AddSongToLibraryAsync(int userId, string songId, CancellationToken cancellationToken);
    public Task RemoveSongFromLibraryAsync(int userId, string songId, CancellationToken cancellationToken);
    public Task CreateLibraryAsync(int userId, CancellationToken cancellationToken);
    public Task RenameLibraryAsync(int userId, string nome, CancellationToken cancellationToken);
    public Task<List<LibrarySongDTO>?> GetCanzoniByLibreriaAsync(int userId, CancellationToken cancellationToken);
}