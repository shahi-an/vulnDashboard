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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft-delete filter (mirrors real configuration)
        modelBuilder.Entity<Vulnerability>().HasQueryFilter(v => !v.IsDeleted);

        // Relationships required for Include() to work
        modelBuilder.Entity<Vulnerability>(b =>
        {
            b.HasMany(v => v.Reminders)
                .WithOne()
                .HasForeignKey(r => r.VulnerabilityId);

            b.HasMany(v => v.StatusUpdates)
                .WithOne()
                .HasForeignKey(su => su.VulnerabilityId);

            b.HasMany(v => v.Comments)
                .WithOne()
                .HasForeignKey(c => c.VulnerabilityId);

            b.HasMany(v => v.Attachments)
                .WithOne()
                .HasForeignKey(a => a.VulnerabilityId);
        });

        modelBuilder.Entity<VulnerabilitySource>(b =>
        {
            b.HasMany(s => s.Vulnerabilities)
                .WithOne(v => v.Source)
                .HasForeignKey(v => v.SourceId);
        });
    }
}
