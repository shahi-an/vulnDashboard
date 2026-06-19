namespace VulnTrack.Domain.Common;

// Plain marker — Application layer wraps these into MediatR INotification.
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
