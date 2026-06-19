using VulnTrack.Domain.Common;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Domain.Entities;

/// <summary>
/// A time-based notification to be dispatched by the Azure Functions SLA timer.
/// Created via Vulnerability.ScheduleReminder — never constructed directly.
/// </summary>
public sealed class ScheduledReminder : BaseEntity
{
    public Guid VulnerabilityId { get; private set; }
    public Vulnerability Vulnerability { get; private set; } = null!;
    public string? RecipientUserId { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public DateTimeOffset ScheduledFor { get; private set; }
    public string? Message { get; private set; }
    public ReminderStatus Status { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? FailureReason { get; private set; }

    private ScheduledReminder() { }

    internal static ScheduledReminder Create(
        Guid vulnerabilityId,
        string recipientEmail,
        DateTimeOffset scheduledFor,
        string createdBy,
        string? recipientUserId = null,
        string? message = null)
    {
        return new ScheduledReminder
        {
            VulnerabilityId = vulnerabilityId,
            RecipientUserId = recipientUserId,
            RecipientEmail = recipientEmail.Trim().ToLowerInvariant(),
            ScheduledFor = scheduledFor,
            Message = message,
            Status = ReminderStatus.Pending,
            CreatedBy = createdBy
        };
    }

    public void MarkSent()
    {
        Status = ReminderStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = ReminderStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = ReminderStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Skip()
    {
        Status = ReminderStatus.Skipped;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
