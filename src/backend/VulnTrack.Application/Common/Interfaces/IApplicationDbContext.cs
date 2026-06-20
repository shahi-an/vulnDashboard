using Microsoft.EntityFrameworkCore;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Vulnerability> Vulnerabilities { get; }
    DbSet<Team> Teams { get; }
    DbSet<VulnerabilitySource> VulnerabilitySources { get; }
    DbSet<StatusUpdate> StatusUpdates { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<UploadBatch> UploadBatches { get; }
    DbSet<ScheduledReminder> ScheduledReminders { get; }
    DbSet<VulnerabilityComment> Comments { get; }
    DbSet<Asset> Assets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
