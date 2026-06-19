using Microsoft.EntityFrameworkCore;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Vulnerability> Vulnerabilities { get; }
    DbSet<Asset> Assets { get; }
    DbSet<VulnerabilityAttachment> Attachments { get; }
    DbSet<VulnerabilityComment> Comments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
