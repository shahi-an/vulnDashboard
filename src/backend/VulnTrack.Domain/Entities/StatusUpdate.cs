using VulnTrack.Domain.Common;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Domain.Entities;

/// <summary>
/// Immutable audit record of a single status transition on a Vulnerability.
/// Created inside Vulnerability.UpdateStatus — never created directly.
/// </summary>
public sealed class StatusUpdate : BaseEntity
{
    public Guid VulnerabilityId { get; private set; }
    public VulnerabilityStatus PreviousStatus { get; private set; }
    public VulnerabilityStatus NewStatus { get; private set; }
    public string? Comment { get; private set; }

    private StatusUpdate() { }

    internal static StatusUpdate Create(
        Guid vulnerabilityId,
        VulnerabilityStatus previousStatus,
        VulnerabilityStatus newStatus,
        string changedBy,
        string? comment = null)
    {
        return new StatusUpdate
        {
            VulnerabilityId = vulnerabilityId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Comment = comment,
            CreatedBy = changedBy
        };
    }
}
