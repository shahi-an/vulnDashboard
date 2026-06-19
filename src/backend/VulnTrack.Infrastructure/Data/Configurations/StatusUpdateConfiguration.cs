using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Infrastructure.Data.Configurations;

internal sealed class StatusUpdateConfiguration : IEntityTypeConfiguration<StatusUpdate>
{
    public void Configure(EntityTypeBuilder<StatusUpdate> builder)
    {
        builder.HasKey(su => su.Id);

        builder.Property(su => su.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(su => su.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(su => su.Comment)
            .HasMaxLength(2000);

        // FK defined on the Vulnerability side; index here for efficient history queries
        builder.HasIndex(su => su.VulnerabilityId);
        builder.HasIndex(su => su.CreatedAt);
    }
}
