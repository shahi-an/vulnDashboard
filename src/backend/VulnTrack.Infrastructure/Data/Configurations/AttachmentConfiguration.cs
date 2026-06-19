using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Infrastructure.Data.Configurations;

internal sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.BlobUri)
            .IsRequired()
            .HasMaxLength(1024);

        builder.HasIndex(a => a.VulnerabilityId);
        builder.HasIndex(a => a.UploadBatchId);
    }
}
