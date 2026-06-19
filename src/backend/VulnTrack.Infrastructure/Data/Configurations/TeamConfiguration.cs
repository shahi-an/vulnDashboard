using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Infrastructure.Data.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);
        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.TeamLeadEmail)
            .HasMaxLength(254);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
