using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Common;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Vulnerability> Vulnerabilities => Set<Vulnerability>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<VulnerabilityAttachment> Attachments => Set<VulnerabilityAttachment>();
    public DbSet<VulnerabilityComment> Comments => Set<VulnerabilityComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before persisting
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        return result;
    }
}
