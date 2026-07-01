using System.Net.Http.Headers;
using System.Net.Http.Json;
using Music.Library.ClientHttp.Abstractions;
using Music.Library.Shared;

namespace Music.Library.ClientHttp;

public class ClientHttp(HttpClient httpClient) : IClientHttp
{
    public async Task CreateLibraryAsync(int userId, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync(
            $"/Library/CreateLibrary?userId={userId}", 
            null, 
            cancellationToken);
        
        if (!response.IsSuccessStatusCode)
            throw new Exception("Errore nella creazione della libreria");
    }

    public async Task<List<LibrarySongDTO>?> GetCanzoniUtenteAsync(string token, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Token: {token}");

        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var response = await httpClient.GetAsync("/Library/GetLibrary", cancellationToken);     
        Console.WriteLine($"Response: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
            return null;
        
        return await response.Content.ReadFromJsonAsync<List<LibrarySongDTO>>(cancellationToken: cancellationToken);
    }
}