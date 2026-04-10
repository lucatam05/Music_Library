using Music.Library.Shared;
using Music.User.Repository.Model;

namespace Music.Library.Business.Abstractions;

public interface IBusiness
{
    public Task<LibraryDTO?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken);
    public Task AddSongToLibraryAsync(int userId, string songId, CancellationToken cancellationToken);
    public Task RemoveSongToLibraryAsync(int userId, string songId, CancellationToken cancellationToken);
    public Task CreateLibraryAsync(int userId, CancellationToken cancellationToken);
    public Task<List<LibrarySongDTO>?> GetCanzoniByLibreriaAsync(int userId, CancellationToken cancellationToken);
}