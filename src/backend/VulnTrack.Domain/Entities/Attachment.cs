using VulnTrack.Domain.Common;

namespace VulnTrack.Domain.Entities;

/// <summary>
/// File attached to a vulnerability, stored in Azure Blob Storage.
/// May originate from a manual upload or from an UploadBatch.
/// </summary>
public sealed class Attachment : BaseEntity
{
    public Guid VulnerabilityId { get; private set; }
    public Guid? UploadBatchId { get; private set; }

    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>Blob URI without SAS token — token generated on demand by IBlobStorageService.</summary>
    public string BlobUri { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }

    private Attachment() { }

    public static Attachment Create(
        Guid vulnerabilityId,
        string fileName,
        string contentType,
        string blobUri,
        long fileSizeBytes,
        string uploadedBy,
        Guid? uploadBatchId = null)
    {
        return new Attachment
        {
            VulnerabilityId = vulnerabilityId,
            UploadBatchId = uploadBatchId,
            FileName = fileName,
            ContentType = contentType,
            BlobUri = blobUri,
            FileSizeBytes = fileSizeBytes,
            CreatedBy = uploadedBy
        };
    }
}
