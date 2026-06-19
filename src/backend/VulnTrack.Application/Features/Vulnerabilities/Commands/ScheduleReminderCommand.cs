using MediatR;
using VulnTrack.Application.Common.Models;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

public sealed record ScheduleReminderCommand(
    Guid VulnerabilityId,
    string RecipientEmail,
    DateTimeOffset ScheduledFor,
    string? RecipientUserId,
    string? Message) : IRequest<Result<Guid>>;
