using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Infrastructure.Data.Configurations;

internal sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.Owner)
            .HasMaxLength(200);

        builder.Property(a => a.Environment)
            .HasMaxLength(50);

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.Tags)
            .HasMaxLength(1000);

        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasIndex(a => a.Name);
        builder.HasIndex(a => a.Type);
    }
}
