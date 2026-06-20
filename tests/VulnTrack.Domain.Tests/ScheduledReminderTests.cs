using FluentAssertions;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;
using Xunit;

namespace VulnTrack.Domain.Tests;

/// <summary>
/// Tests for ScheduledReminder state transitions.
/// Reminders are created via Vulnerability.ScheduleReminder — no direct construction.
/// </summary>
public sealed class ScheduledReminderTests
{
    private static readonly Guid SourceId = Guid.NewGuid();

    [Fact]
    public void ScheduleReminder_StartsAsPending()
    {
        var reminder = CreateReminder();
        reminder.Status.Should().Be(ReminderStatus.Pending);
    }

    [Fact]
    public void ScheduleReminder_NormalisesAndStoresEmail()
    {
        var vuln = BuildVulnerability();
        var reminder = vuln.ScheduleReminder(" OPS@Example.COM ", DateTimeOffset.UtcNow.AddDays(1), "admin");
        reminder.RecipientEmail.Should().Be("ops@example.com");
    }

    [Fact]
    public void MarkSent_ChangesStatusToSent()
    {
        var reminder = CreateReminder();
        var before = DateTimeOffset.UtcNow;

        reminder.MarkSent();

        reminder.Status.Should().Be(ReminderStatus.Sent);
        reminder.SentAt.Should().NotBeNull();
        reminder.SentAt!.Value.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void MarkFailed_ChangesStatusToFailed_RecordsReason()
    {
        var reminder = CreateReminder();

        reminder.MarkFailed("SMTP timeout");

        reminder.Status.Should().Be(ReminderStatus.Failed);
        reminder.FailureReason.Should().Be("SMTP timeout");
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        var reminder = CreateReminder();
        var before = DateTimeOffset.UtcNow;

        reminder.Cancel();

        reminder.Status.Should().Be(ReminderStatus.Cancelled);
        reminder.CancelledAt.Should().NotBeNull();
        reminder.CancelledAt!.Value.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Skip_ChangesStatusToSkipped()
    {
        var reminder = CreateReminder();

        reminder.Skip();

        reminder.Status.Should().Be(ReminderStatus.Skipped);
    }

    private static Vulnerability BuildVulnerability() =>
        Vulnerability.Create("db-01", "192.168.0.5", VulnerabilityType.MissingPatch, Severity.High, "desc", SourceId, "scanner");

    private static ScheduledReminder CreateReminder()
    {
        var vuln = BuildVulnerability();
        return vuln.ScheduleReminder("ops@example.com", DateTimeOffset.UtcNow.AddDays(3), "admin");
    }
}
