using Microsoft.AspNetCore.Mvc;
using Music.Library.Business.Abstractions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Music.Catalogue.Shared.Exceptions;

namespace MusicLibrary.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class LibraryController(IBusiness business) : ControllerBase
{
    [HttpGet(Name = "GetLibrary")]
    public async Task<ActionResult> GetLibraryPerIdAsync(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
            return Unauthorized();
    
        int userId = int.Parse(userIdClaim);

        try
        {
            var canzoni = await business.GetCanzoniByLibreriaAsync(userId, cancellationToken);
            return Ok(canzoni);
        }
        catch (ModelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost(Name = "AddSong")]
    public async Task<ActionResult> AddSongToLibraryAsync(string songId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
            return Unauthorized();
    
        int userId = int.Parse(userIdClaim);
        try
        {
            await business.AddSongToLibraryAsync(userId, songId, cancellationToken);
            return Ok();
        }
        catch (ModelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    [HttpDelete(Name = "DeleteSong")]
    public async Task<ActionResult> RemoveSongFromLibraryAsync(string songId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
            return Unauthorized();
    
        int userId = int.Parse(userIdClaim);
        try
        {
            await business.RemoveSongFromLibraryAsync(userId, songId, cancellationToken);
            return Ok();
        }
        catch (ModelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    [HttpPost(Name = "CreateLibrary")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateLibraryAsync(int userId, CancellationToken cancellationToken)
    {
        await business.CreateLibraryAsync(userId, cancellationToken);
        return Ok();
    }
}