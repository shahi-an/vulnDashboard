using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Infrastructure.Data.Configurations;

internal sealed class ScheduledReminderConfiguration : IEntityTypeConfiguration<ScheduledReminder>
{
    public void Configure(EntityTypeBuilder<ScheduledReminder> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RecipientEmail)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(r => r.RecipientUserId)
            .HasMaxLength(128);

        builder.Property(r => r.Message)
            .HasMaxLength(2000);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.FailureReason)
            .HasMaxLength(1000);

        // The Functions SLA timer queries: WHERE Status = 'Pending' AND ScheduledFor <= NOW()
        builder.HasIndex(r => new { r.Status, r.ScheduledFor });
        builder.HasIndex(r => r.VulnerabilityId);
    }
}
