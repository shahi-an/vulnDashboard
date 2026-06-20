using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Features.Reminders.Commands;
using VulnTrack.Application.Tests.Common;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;
using Xunit;

namespace VulnTrack.Application.Tests.Handlers;

public sealed class ProcessDueRemindersCommandHandlerTests
{
    [Fact]
    public async Task Handle_NoPendingReminders_ReturnsZero()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var processed = await mediator.Send(new ProcessDueRemindersCommand());

        processed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PendingDueReminder_SendsEmailAndMarksAsSent()
    {
        var graphMock = new Mock<IGraphService>();
        graphMock.Setup(g => g.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var sp = TestServiceProvider.Build(graph: graphMock);
        var db = sp.GetRequiredService<IApplicationDbContext>();
        var mediator = sp.GetRequiredService<IMediator>();

        await SeedReminderAsync(db, scheduledFor: DateTimeOffset.UtcNow.AddHours(-1));

        var processed = await mediator.Send(new ProcessDueRemindersCommand());

        processed.Should().Be(1);
        graphMock.Verify(g => g.SendEmailAsync("ops@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        var reminder = db.ScheduledReminders.Single();
        reminder.Status.Should().Be(ReminderStatus.Sent);
        reminder.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_FutureReminder_NotProcessed()
    {
        using var sp = TestServiceProvider.Build();
        var db = sp.GetRequiredService<IApplicationDbContext>();
        var mediator = sp.GetRequiredService<IMediator>();

        await SeedReminderAsync(db, scheduledFor: DateTimeOffset.UtcNow.AddDays(1));

        var processed = await mediator.Send(new ProcessDueRemindersCommand());

        processed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RemediatedVulnerability_SkipsReminderWithoutEmail()
    {
        var graphMock = new Mock<IGraphService>();
        using var sp = TestServiceProvider.Build(graph: graphMock);
        var db = sp.GetRequiredService<IApplicationDbContext>();
        var mediator = sp.GetRequiredService<IMediator>();

        await SeedReminderAsync(db, scheduledFor: DateTimeOffset.UtcNow.AddHours(-1), status: VulnerabilityStatus.Remediated);

        var processed = await mediator.Send(new ProcessDueRemindersCommand());

        processed.Should().Be(1);
        graphMock.Verify(g => g.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        var reminder = db.ScheduledReminders.Single();
        reminder.Status.Should().Be(ReminderStatus.Skipped);
    }

    [Fact]
    public async Task Handle_EmailThrows_MarksReminderAsFailed()
    {
        var graphMock = new Mock<IGraphService>();
        graphMock.Setup(g => g.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP timeout"));

        using var sp = TestServiceProvider.Build(graph: graphMock);
        var db = sp.GetRequiredService<IApplicationDbContext>();
        var mediator = sp.GetRequiredService<IMediator>();

        await SeedReminderAsync(db, scheduledFor: DateTimeOffset.UtcNow.AddHours(-1));

        // Handler catches exceptions per-reminder; total processed = 0 (exception counts as not processed)
        var processed = await mediator.Send(new ProcessDueRemindersCommand());

        processed.Should().Be(0);
        var reminder = db.ScheduledReminders.Single();
        reminder.Status.Should().Be(ReminderStatus.Failed);
        reminder.FailureReason.Should().Contain("SMTP timeout");
    }

    private static async Task SeedReminderAsync(
        IApplicationDbContext db,
        DateTimeOffset scheduledFor,
        VulnerabilityStatus vulnStatus = VulnerabilityStatus.Open)
    {
        var vuln = Vulnerability.Create("srv-01", "10.0.0.1", VulnerabilityType.MissingPatch, Severity.High, "desc", Guid.NewGuid(), "seeder");

        if (vulnStatus != VulnerabilityStatus.Open)
            vuln.UpdateStatus(vulnStatus, "seeder");

        vuln.ScheduleReminder("ops@example.com", scheduledFor, "admin");
        vuln.ClearDomainEvents();

        db.Vulnerabilities.Add(vuln);
        await db.SaveChangesAsync();
    }
}
