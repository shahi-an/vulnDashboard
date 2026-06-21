using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Features.Users.Queries;
using VulnTrack.Application.Tests.Common;
using Xunit;

namespace VulnTrack.Application.Tests.Handlers;

public sealed class SearchUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToGraphService_AndMapsResults()
    {
        var userMock = new Mock<IGraphUserDto>();
        userMock.Setup(u => u.Id).Returns("user-1");
        userMock.Setup(u => u.DisplayName).Returns("John Doe");
        userMock.Setup(u => u.Mail).Returns("john@example.com");
        userMock.Setup(u => u.JobTitle).Returns("Engineer");

        var graphMock = new Mock<IGraphService>();
        graphMock.Setup(g => g.SearchUsersAsync("john", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IGraphUserDto> { userMock.Object });

        using var sp = TestServiceProvider.Build(graph: graphMock);
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SearchUsersQuery("john"));

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("user-1");
        result[0].DisplayName.Should().Be("John Doe");
        result[0].Email.Should().Be("john@example.com");
        result[0].JobTitle.Should().Be("Engineer");

        graphMock.Verify(g => g.SearchUsersAsync("john", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyGraphResult_ReturnsEmptyList()
    {
        var graphMock = new Mock<IGraphService>();
        graphMock.Setup(g => g.SearchUsersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IGraphUserDto>());

        using var sp = TestServiceProvider.Build(graph: graphMock);
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SearchUsersQuery("nobody"));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NullMailInResult_MapsEmailToNull()
    {
        var userMock = new Mock<IGraphUserDto>();
        userMock.Setup(u => u.Id).Returns("user-2");
        userMock.Setup(u => u.DisplayName).Returns("Jane Smith");
        userMock.Setup(u => u.Mail).Returns((string?)null);
        userMock.Setup(u => u.JobTitle).Returns((string?)null);

        var graphMock = new Mock<IGraphService>();
        graphMock.Setup(g => g.SearchUsersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IGraphUserDto> { userMock.Object });

        using var sp = TestServiceProvider.Build(graph: graphMock);
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SearchUsersQuery("jane"));

        result[0].Email.Should().BeNull();
        result[0].JobTitle.Should().BeNull();
    }
}
