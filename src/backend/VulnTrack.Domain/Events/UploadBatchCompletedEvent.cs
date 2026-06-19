using VulnTrack.Domain.Common;

namespace VulnTrack.Domain.Events;

public sealed record UploadBatchCompletedEvent(
    Guid BatchId,
    Guid SourceId,
    int TotalRecords,
    int SuccessCount,
    int FailureCount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
