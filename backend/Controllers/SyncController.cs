using LilyWhiteMap.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LilyWhiteMap.Api.Controllers;

[ApiController]
[Route("api/admin/sync")]
public sealed class SyncController(SpursRosterService rosterService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var count = await rosterService.SyncAsync(cancellationToken);
        return Ok(new { records = count });
    }
}
