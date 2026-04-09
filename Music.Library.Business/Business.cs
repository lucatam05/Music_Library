using Microsoft.Extensions.Configuration;
using Music.Library.Business.Abstractions;
using Music.Library.Shared;
using Music.User.Repository.Abstractions;

namespace Music.Library.Business;

public class Business(IRepository repository, IClient Http clienthttp) : IBusiness
{
    public async Task<LibraryDTO?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AddSongToLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveSongToLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task CreateLibraryAsync(int userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<List<LibrarySongDTO>?> GetCanzoniByLibreriaAsync(int libraryId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}