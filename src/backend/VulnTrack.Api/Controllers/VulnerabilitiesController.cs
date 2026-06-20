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
        [FromQuery] VulnerabilityType? vulnerabilityType = null,
        [FromQuery] Guid? teamId = null,
        [FromQuery] Guid? sourceId = null,
        [FromQuery] string? assignedToEmail = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTimeOffset? createdAfter = null,
        [FromQuery] DateTimeOffset? createdBefore = null,
        [FromQuery] DateTimeOffset? followUpDueBefore = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetVulnerabilitiesQuery(
                pageNumber, pageSize,
                severity, status, vulnerabilityType,
                teamId, sourceId, assignedToEmail, search,
                createdAfter, createdBefore, followUpDueBefore),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VulnerabilityDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVulnerabilityByIdQuery(id), cancellationToken);
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVulnerabilityCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteVulnerabilityCommand(id), cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateVulnerabilityStatusCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignVulnerabilityCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPatch("{id:guid}/ecd")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetEcd(Guid id, [FromBody] SetVulnerabilityEcdCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPatch("{id:guid}/follow-up-due")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFollowUpDue(Guid id, [FromBody] SetFollowUpDueCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    // ── Status history ────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/status-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetVulnerabilityStatusHistoryQuery(id, pageNumber, pageSize),
            cancellationToken);

        return Ok(result);
    }

    // ── Comments ──────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(
        Guid id,
        [FromBody] AddVulnerabilityCommentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AddVulnerabilityCommentCommand(id, request.Body, request.IsInternal),
            cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    // ── Reminders ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/reminders")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleReminder(
        Guid id,
        [FromBody] ScheduleReminderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ScheduleReminderCommand(id, request.RecipientEmail, request.ScheduledFor, request.RecipientUserId, request.Message),
            cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    // ── Attachments ───────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/attachments")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAttachment(
        Guid id,
        IFormFile file,
        [FromQuery] Guid? uploadBatchId = null,
        CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(
            new UploadAttachmentCommand(id, file.FileName, file.ContentType, stream, uploadBatchId),
            cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download-url")]
    [ProducesResponseType(typeof(Uri), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttachmentDownloadUrl(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var uri = await mediator.Send(new GetAttachmentDownloadUrlQuery(attachmentId), cancellationToken);
        return Ok(new { url = uri.ToString() });
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteAttachmentCommand(attachmentId), cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}

public sealed record AddVulnerabilityCommentRequest(string Body, bool IsInternal = false);
public sealed record ScheduleReminderRequest(string RecipientEmail, DateTimeOffset ScheduledFor, string? RecipientUserId = null, string? Message = null);
