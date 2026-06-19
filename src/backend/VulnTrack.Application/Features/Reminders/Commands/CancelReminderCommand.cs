using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Reminders.Commands;

public sealed record CancelReminderCommand(Guid ReminderId) : IRequest<Result>;

internal sealed class CancelReminderCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CancelReminderCommand, Result>
{
    public async Task<Result> Handle(CancelReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await dbContext.ScheduledReminders.FindAsync([request.ReminderId], cancellationToken)
            ?? throw new NotFoundException(nameof(ScheduledReminder), request.ReminderId);

        reminder.Cancel();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
