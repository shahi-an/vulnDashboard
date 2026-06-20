using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTrack.Application.Features.Users.Queries;

namespace VulnTrack.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<UserSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(Array.Empty<UserSearchResultDto>());

        var result = await mediator.Send(new SearchUsersQuery(q), cancellationToken);
        return Ok(result);
    }
}
