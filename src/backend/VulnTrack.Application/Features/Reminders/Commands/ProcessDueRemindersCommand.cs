using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.Reminders.Commands;

/// <summary>
/// Invoked by the Azure Functions SlaReminderTimer (daily 08:00 UTC).
/// Returns the count of reminders processed (sent + skipped).
/// </summary>
public sealed record ProcessDueRemindersCommand : IRequest<int>;

internal sealed class ProcessDueRemindersCommandHandler(
    IApplicationDbContext dbContext,
    IServiceBusPublisher publisher,
    ILogger<ProcessDueRemindersCommandHandler> logger)
    : IRequestHandler<ProcessDueRemindersCommand, int>
{
    public async Task<int> Handle(ProcessDueRemindersCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var dueReminders = await dbContext.ScheduledReminders
            .Include(r => r.Vulnerability)
            .Where(r => r.Status == ReminderStatus.Pending && r.ScheduledFor <= now)
            .ToListAsync(cancellationToken);

        var processed = 0;

        foreach (var reminder in dueReminders)
        {
            try
            {
                if (reminder.Vulnerability.Status == VulnerabilityStatus.Remediated)
                {
                    reminder.Skip();
                }
                else
                {
                    await publisher.PublishAsync("notifications", new
                    {
                        EventType = "ReminderDue",
                        reminder.VulnerabilityId,
                        reminder.RecipientEmail,
                        reminder.RecipientUserId,
                        reminder.Message,
                        reminder.ScheduledFor
                    }, cancellationToken);

                    reminder.MarkSent();
                }

                processed++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process reminder {ReminderId}", reminder.Id);
                reminder.MarkFailed(ex.Message);
            }
        }

        if (processed > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return processed;
    }
}
