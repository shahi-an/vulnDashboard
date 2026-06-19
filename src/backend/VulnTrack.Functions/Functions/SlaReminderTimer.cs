using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace VulnTrack.Functions.Functions;

public sealed class SlaReminderTimer(ILogger<SlaReminderTimer> logger)
{
    // Runs daily at 08:00 UTC — checks for vulnerabilities approaching SLA breach.
    [Function(nameof(SlaReminderTimer))]
    public async Task Run(
        [TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("SLA reminder check triggered at {Now}", DateTimeOffset.UtcNow);
        await Task.CompletedTask;
    }
}
