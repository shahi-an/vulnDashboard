using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Tests.Common;

/// <summary>
/// Minimal EF Core context backed by the InMemory provider.
/// Used in Application.Tests so that we don't need the Infrastructure project.
/// Does not dispatch domain events (that is Infrastructure's responsibility).
/// </summary>
internal sealed class TestDbContext : DbContext, IApplicationDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Vulnerability> Vulnerabilities => Set<Vulnerability>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<VulnerabilitySource> VulnerabilitySources => Set<VulnerabilitySource>();
    public DbSet<StatusUpdate> StatusUpdates => Set<StatusUpdate>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<UploadBatch> UploadBatches => Set<UploadBatch>();
    public DbSet<ScheduledReminder> ScheduledReminders => Set<ScheduledReminder>();
    public DbSet<VulnerabilityComment> Comments => Set<VulnerabilityComment>();
    public DbSet<Asset> Assets => Set<Asset>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Disable auto-detect so that iterating Entries() below does not trigger
        // a second DetectChanges() call on top of the one we run explicitly.
        ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            ChangeTracker.DetectChanges();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is VulnTrack.Domain.Common.BaseEntity entity)
                    entity.ClearDomainEvents();

                // EF Core InMemory cascade-state quirk: when a NEW dependent entity
                // (non-sentinel Guid PK from BaseEntity's field initializer) is discovered
                // in a private backing-field collection navigation of a Modified principal,
                // EF Core sets the dependent's state directly to Modified (because the PK
                // is non-sentinel so EF Core cannot tell if it is new or existing). EF then
                // calls InMemoryTable.Update() on an entity that was never INSERTed →
                // DbUpdateConcurrencyException. Identify these entities by the fact that
                // none of their scalar properties have actually changed (original == current
                // for every property), since the state was set by cascade, not by mutation.
                if (entry.State == EntityState.Modified
                    && entry.Properties.All(p => Equals(p.CurrentValue, p.OriginalValue)))
                {
                    entry.State = EntityState.Added;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft-delete filter (mirrors real configuration)
        modelBuilder.Entity<Vulnerability>().HasQueryFilter(v => !v.IsDeleted);

        // Relationships required for Include() to work, with explicit field access
        // so EF Core correctly tracks mutations to the private backing field collections.
        modelBuilder.Entity<Vulnerability>(b =>
        {
            b.HasKey(v => v.Id);

            b.HasMany(v => v.StatusUpdates)
                .WithOne()
                .HasForeignKey(su => su.VulnerabilityId);
            b.Navigation(v => v.StatusUpdates).HasField("_statusUpdates");

            b.HasMany(v => v.Reminders)
                .WithOne(r => r.Vulnerability)
                .HasForeignKey(r => r.VulnerabilityId);
            b.Navigation(v => v.Reminders).HasField("_reminders");

            b.HasMany(v => v.Comments)
                .WithOne()
                .HasForeignKey(c => c.VulnerabilityId);
            b.Navigation(v => v.Comments).HasField("_comments");

            b.HasMany(v => v.Attachments)
                .WithOne()
                .HasForeignKey(a => a.VulnerabilityId);
            b.Navigation(v => v.Attachments).HasField("_attachments");
        });

        modelBuilder.Entity<VulnerabilitySource>(b =>
        {
            b.HasMany(s => s.Vulnerabilities)
                .WithOne()
                .HasForeignKey(v => v.SourceId)
                .IsRequired(false);
        });
    }
}
