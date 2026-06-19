namespace VulnTrack.Domain.Enums;

public enum ReminderStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3,
    Skipped = 4   // e.g. vulnerability was remediated before reminder fired
}
