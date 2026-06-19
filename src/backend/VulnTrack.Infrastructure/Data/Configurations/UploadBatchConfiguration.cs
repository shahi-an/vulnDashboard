using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Infrastructure.Data.Configurations;

internal sealed class UploadBatchConfiguration : IEntityTypeConfiguration<UploadBatch>
{
    public void Configure(EntityTypeBuilder<UploadBatch> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(b => b.RawFileBlobUri)
            .HasMaxLength(1024);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(b => b.ErrorSummary)
            .HasMaxLength(4000);

        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasIndex(b => b.SourceId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CreatedAt);
    }
}
