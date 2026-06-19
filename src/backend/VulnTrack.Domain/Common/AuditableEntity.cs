namespace VulnTrack.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public void MarkDeleted(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }
}
