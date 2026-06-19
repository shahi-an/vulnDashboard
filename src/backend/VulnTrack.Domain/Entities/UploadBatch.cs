using VulnTrack.Domain.Common;
using VulnTrack.Domain.Enums;
using VulnTrack.Domain.Events;

namespace VulnTrack.Domain.Entities;

/// <summary>
/// Represents a bulk import of vulnerability data from a scanner export file
/// (e.g., Nessus .nessus, Qualys XML, CSV).
/// </summary>
public sealed class UploadBatch : AuditableEntity
{
    public Guid SourceId { get; private set; }
    public VulnerabilitySource Source { get; private set; } = null!;

    public string OriginalFileName { get; private set; } = string.Empty;

    /// <summary>Blob URI of the raw uploaded file, retained for audit purposes.</summary>
    public string? RawFileBlobUri { get; private set; }

    public UploadBatchStatus Status { get; private set; }
    public int TotalRecords { get; private set; }
    public int ProcessedCount { get; private set; }
    public int SuccessCount { get; private set; }
    public int FailureCount { get; private set; }
    public string? ErrorSummary { get; private set; }

    private readonly List<Vulnerability> _vulnerabilities = [];
    public IReadOnlyCollection<Vulnerability> Vulnerabilities => _vulnerabilities.AsReadOnly();

    private UploadBatch() { }

    public static UploadBatch Create(
        Guid sourceId,
        string originalFileName,
        string uploadedBy,
        string? rawFileBlobUri = null)
    {
        return new UploadBatch
        {
            SourceId = sourceId,
            OriginalFileName = originalFileName,
            RawFileBlobUri = rawFileBlobUri,
            Status = UploadBatchStatus.Queued,
            CreatedBy = uploadedBy
        };
    }

    public void Start()
    {
        Status = UploadBatchStatus.Processing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetTotalRecords(int total)
    {
        TotalRecords = total;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordSuccess()
    {
        ProcessedCount++;
        SuccessCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordFailure()
    {
        ProcessedCount++;
        FailureCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(string updatedBy)
    {
        Status = FailureCount == 0 ? UploadBatchStatus.Completed : UploadBatchStatus.CompletedWithErrors;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new UploadBatchCompletedEvent(Id, SourceId, TotalRecords, SuccessCount, FailureCount));
    }

    public void Fail(string errorSummary, string updatedBy)
    {
        Status = UploadBatchStatus.Failed;
        ErrorSummary = errorSummary;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(string updatedBy)
    {
        Status = UploadBatchStatus.Cancelled;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
