using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VulnTrack.Application.Common.Models;
using VulnTrack.Application.Features.Vulnerabilities.Commands;
using VulnTrack.Application.Features.Vulnerabilities.Queries;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class VulnerabilitiesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VulnerabilityListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Severity? severity = null,
        [FromQuery] VulnerabilityStatus? status = null,
        [FromQuery] Guid? assetId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetVulnerabilitiesQuery(pageNumber, pageSize, severity, status, assetId, search),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVulnerabilityCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        // Placeholder — GetVulnerabilityByIdQuery to be added in subsequent iteration
        await Task.CompletedTask;
        return Ok(new { id });
    }
}
