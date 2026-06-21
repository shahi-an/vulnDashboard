using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

internal sealed class ScheduleReminderCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<ScheduleReminderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ScheduleReminderCommand request, CancellationToken cancellationToken)
    {
        var vulnerability = await dbContext.Vulnerabilities.FindAsync([request.VulnerabilityId], cancellationToken)
            ?? throw new NotFoundException(nameof(Vulnerability), request.VulnerabilityId);

        var reminder = vulnerability.ScheduleReminder(
            request.RecipientEmail,
            request.ScheduledFor,
            currentUser.UserName,
            request.RecipientUserId,
            request.Message);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(reminder.Id);
    }
}
