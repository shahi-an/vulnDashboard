using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VulnTrack.Application.Features.Reminders.Commands;

namespace VulnTrack.Functions.Functions;

public sealed class SlaReminderTimer(IMediator mediator, ILogger<SlaReminderTimer> logger)
{
    // Runs daily at 08:00 UTC — dispatches pending reminders due today.
    [Function(nameof(SlaReminderTimer))]
    public async Task Run(
        [TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("SLA reminder check triggered at {Now}", DateTimeOffset.UtcNow);

        var processed = await mediator.Send(new ProcessDueRemindersCommand(), cancellationToken);

        logger.LogInformation("SLA reminder check complete — {Count} reminder(s) processed", processed);
    }
}
