using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.Reminders.Queries;

public sealed record PendingReminderDto(
    Guid Id,
    Guid VulnerabilityId,
    string VulnerabilityNumber,
    string RecipientEmail,
    string? Message,
    DateTimeOffset ScheduledFor);

public sealed record GetPendingRemindersQuery(DateTimeOffset? DueBefore = null) : IRequest<IReadOnlyList<PendingReminderDto>>;

internal sealed class GetPendingRemindersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPendingRemindersQuery, IReadOnlyList<PendingReminderDto>>
{
    public async Task<IReadOnlyList<PendingReminderDto>> Handle(
        GetPendingRemindersQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ScheduledReminders
            .AsNoTracking()
            .Include(r => r.Vulnerability)
            .Where(r => r.Status == ReminderStatus.Pending);

        if (request.DueBefore.HasValue)
            query = query.Where(r => r.ScheduledFor <= request.DueBefore.Value);

        return await query
            .OrderBy(r => r.ScheduledFor)
            .Select(r => new PendingReminderDto(
                r.Id,
                r.VulnerabilityId,
                r.Vulnerability.VulnerabilityNumber,
                r.RecipientEmail,
                r.Message,
                r.ScheduledFor))
            .ToListAsync(cancellationToken);
    }
}
