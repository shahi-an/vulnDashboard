using VulnTrack.Domain.Common;

namespace VulnTrack.Domain.Events;

public sealed record ReminderScheduledEvent(
    Guid VulnerabilityId,
    Guid ReminderId,
    DateTimeOffset ScheduledFor) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
