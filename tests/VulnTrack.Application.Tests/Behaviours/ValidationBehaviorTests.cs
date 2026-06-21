using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VulnTrack.Application.Features.Vulnerabilities.Commands;
using VulnTrack.Application.Tests.Common;
using VulnTrack.Domain.Enums;
using Xunit;
using Severity = VulnTrack.Domain.Enums.Severity;

namespace VulnTrack.Application.Tests.Behaviours;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ValidCreateCommand_DoesNotThrow()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var command = new CreateVulnerabilityCommand(
            ServerName: "web-01",
            ServerIp: "192.168.0.1",
            VulnerabilityType: VulnerabilityType.MissingPatch,
            Severity: Severity.High,
            Description: "Valid description",
            SourceId: Guid.NewGuid(),
            Solution: null,
            CveId: null,
            CvssScore: null,
            DiscoveredAt: null,
            UploadBatchId: null);

        var act = () => mediator.Send(command);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_EmptyServerName_ThrowsValidationException()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var command = new CreateVulnerabilityCommand(
            ServerName: "",
            ServerIp: "10.0.0.1",
            VulnerabilityType: VulnerabilityType.MissingPatch,
            Severity: Severity.High,
            Description: "desc",
            SourceId: Guid.NewGuid(),
            Solution: null,
            CveId: null,
            CvssScore: null,
            DiscoveredAt: null,
            UploadBatchId: null);

        var act = () => mediator.Send(command);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(f => f.PropertyName == "ServerName");
    }

    [Fact]
    public async Task Handle_InvalidIpAddress_ThrowsValidationException()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var command = new CreateVulnerabilityCommand(
            ServerName: "web-01",
            ServerIp: "not-an-ip",
            VulnerabilityType: VulnerabilityType.MissingPatch,
            Severity: Severity.High,
            Description: "desc",
            SourceId: Guid.NewGuid(),
            Solution: null,
            CveId: null,
            CvssScore: null,
            DiscoveredAt: null,
            UploadBatchId: null);

        var act = () => mediator.Send(command);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(f => f.PropertyName == "ServerIp");
    }

    [Fact]
    public async Task Handle_InvalidCvssScore_ThrowsValidationException()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var command = new CreateVulnerabilityCommand(
            ServerName: "web-01",
            ServerIp: "10.0.0.1",
            VulnerabilityType: VulnerabilityType.MissingPatch,
            Severity: Severity.High,
            Description: "desc",
            SourceId: Guid.NewGuid(),
            Solution: null,
            CveId: null,
            CvssScore: 11.0m,      // > 10.0 — invalid
            DiscoveredAt: null,
            UploadBatchId: null);

        var act = () => mediator.Send(command);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(f => f.PropertyName == "CvssScore");
    }

    [Fact]
    public async Task Handle_MalformedCveId_ThrowsValidationException()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var command = new CreateVulnerabilityCommand(
            ServerName: "web-01",
            ServerIp: "10.0.0.1",
            VulnerabilityType: VulnerabilityType.MissingPatch,
            Severity: Severity.High,
            Description: "desc",
            SourceId: Guid.NewGuid(),
            Solution: null,
            CveId: "NOTACVE",
            CvssScore: null,
            DiscoveredAt: null,
            UploadBatchId: null);

        var act = () => mediator.Send(command);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(f => f.PropertyName == "CveId");
    }
}
