using LilyWhiteMap.Api.Models;
using LilyWhiteMap.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LilyWhiteMap.Api.Controllers;

[ApiController]
[Route("api/players")]
public sealed class PlayersController(SpursRosterService rosterService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Player>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Player>>> GetPlayers(
        [FromQuery] bool includeBirthplaces = false,
        CancellationToken cancellationToken = default)
    {
        var players = await rosterService.GetPlayersAsync(cancellationToken);
        return Ok(players);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<Player>> GetPlayer(
        string slug,
        CancellationToken cancellationToken)
    {
        var player = await rosterService.GetPlayerAsync(slug, cancellationToken);
        return player is null ? NotFound() : Ok(player);
    }
}
