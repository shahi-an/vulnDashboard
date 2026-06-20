using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Application.Tests.Common;

/// <summary>
/// Builds a <see cref="ServiceProvider"/> that runs the full Application MediatR pipeline
/// against an EF Core InMemory database, with all external services mocked.
/// Each call produces an isolated provider with its own database.
/// Callers must dispose the returned provider.
/// </summary>
internal static class TestServiceProvider
{
    public static ServiceProvider Build(
        Mock<ICurrentUserService>? currentUser = null,
        Mock<IBlobStorageService>? blob = null,
        Mock<IServiceBusPublisher>? serviceBus = null,
        Mock<IGraphService>? graph = null)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        // Full Application layer: MediatR handlers + validators + behaviours
        services.AddApplication();

        // Unique InMemory database per provider instance → test isolation
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<TestDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<TestDbContext>());

        // Mocked external dependencies
        var userMock = currentUser ?? DefaultCurrentUser();
        services.AddScoped(_ => userMock.Object);

        var busMock = serviceBus ?? new Mock<IServiceBusPublisher>();
        services.AddScoped(_ => busMock.Object);

        var blobMock = blob ?? new Mock<IBlobStorageService>();
        services.AddScoped(_ => blobMock.Object);

        var graphMock = graph ?? new Mock<IGraphService>();
        services.AddScoped(_ => graphMock.Object);

        return services.BuildServiceProvider(validateScopes: false);
    }

    private static Mock<ICurrentUserService> DefaultCurrentUser()
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(u => u.UserId).Returns("test-user-id");
        mock.Setup(u => u.UserEmail).Returns("test@example.com");
        return mock;
    }
}
