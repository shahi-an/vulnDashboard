using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Features.Assets.Commands;
using VulnTrack.Application.Features.Assets.Queries;
using VulnTrack.Application.Tests.Common;
using VulnTrack.Domain.Enums;
using Xunit;

namespace VulnTrack.Application.Tests.Handlers;

public sealed class AssetHandlerTests
{
    // ── CreateAssetCommand ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsset_ValidCommand_ReturnsSuccessWithId()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new CreateAssetCommand("db-prod-01", AssetType.Server, "Primary DB", "infra@co.com", "Production"));

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsset_ValidCommand_PersistedInDatabase()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();
        var db = sp.GetRequiredService<IApplicationDbContext>();

        var result = await mediator.Send(new CreateAssetCommand("web-server-01", AssetType.WebApplication, null, null, null));

        var saved = await db.Assets.FindAsync(result.Value);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("web-server-01");
        saved.Type.Should().Be(AssetType.WebApplication);
        saved.CreatedBy.Should().Be("test-user-id");
    }

    [Fact]
    public async Task CreateAsset_OptionalFieldsAreNullable()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();
        var db = sp.GetRequiredService<IApplicationDbContext>();

        var result = await mediator.Send(new CreateAssetCommand("bare-server", AssetType.Server, null, null, null));

        var saved = await db.Assets.FindAsync(result.Value);
        saved!.Description.Should().BeNull();
        saved.Owner.Should().BeNull();
        saved.Environment.Should().BeNull();
    }

    // ── UpdateAssetCommand ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsset_ExistingAsset_UpdatesFields()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();
        var db = sp.GetRequiredService<IApplicationDbContext>();

        var createResult = await mediator.Send(new CreateAssetCommand("original-name", AssetType.Server, null, null, null));
        var id = createResult.Value;

        var updateResult = await mediator.Send(new UpdateAssetCommand(id, "updated-name", AssetType.CloudResource, "new desc", "owner@co.com", "Staging"));

        updateResult.Succeeded.Should().BeTrue();
        var saved = await db.Assets.FindAsync(id);
        saved!.Name.Should().Be("updated-name");
        saved.Type.Should().Be(AssetType.CloudResource);
        saved.Description.Should().Be("new desc");
        saved.Owner.Should().Be("owner@co.com");
        saved.Environment.Should().Be("Staging");
    }

    [Fact]
    public async Task UpdateAsset_UnknownId_ThrowsNotFoundException()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var act = () => mediator.Send(new UpdateAssetCommand(Guid.NewGuid(), "name", AssetType.Server, null, null, null));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── DeleteAssetCommand ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsset_ExistingAsset_SoftDeletes()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();
        var db = sp.GetRequiredService<IApplicationDbContext>();

        var createResult = await mediator.Send(new CreateAssetCommand("to-delete", AssetType.Server, null, null, null));
        var id = createResult.Value;

        var deleteResult = await mediator.Send(new DeleteAssetCommand(id));

        deleteResult.Succeeded.Should().BeTrue();
        var saved = await db.Assets.FindAsync(id);
        saved!.IsDeleted.Should().BeTrue();
        saved.DeletedBy.Should().Be("test-user-id");
    }

    [Fact]
    public async Task DeleteAsset_UnknownId_ThrowsNotFoundException()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var act = () => mediator.Send(new DeleteAssetCommand(Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── GetAssetsQuery ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAssets_NoFilter_ReturnsAllAssets()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new CreateAssetCommand("asset-a", AssetType.Server, null, null, null));
        await mediator.Send(new CreateAssetCommand("asset-b", AssetType.CloudResource, null, null, null));

        var result = await mediator.Send(new GetAssetsQuery());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAssets_SearchByName_FiltersResults()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new CreateAssetCommand("db-server", AssetType.Server, null, null, null));
        await mediator.Send(new CreateAssetCommand("web-server", AssetType.WebApplication, null, null, null));

        var result = await mediator.Send(new GetAssetsQuery(Search: "db"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("db-server");
    }

    [Fact]
    public async Task GetAssets_FilterByType_FiltersResults()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new CreateAssetCommand("server-1", AssetType.Server, null, null, null));
        await mediator.Send(new CreateAssetCommand("cloud-1", AssetType.CloudResource, null, null, null));

        var result = await mediator.Send(new GetAssetsQuery(Type: "Server"));

        result.Should().HaveCount(1);
        result[0].Type.Should().Be("Server");
    }

    [Fact]
    public async Task GetAssets_ReturnsOrderedByName()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new CreateAssetCommand("zebra", AssetType.Server, null, null, null));
        await mediator.Send(new CreateAssetCommand("alpha", AssetType.Server, null, null, null));

        var result = await mediator.Send(new GetAssetsQuery());

        result[0].Name.Should().Be("alpha");
        result[1].Name.Should().Be("zebra");
    }

    // ── GetAssetByIdQuery ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAssetById_ExistingId_ReturnsDto()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var createResult = await mediator.Send(new CreateAssetCommand("lookup-me", AssetType.Server, "desc", "owner@co.com", "Prod"));
        var id = createResult.Value;

        var dto = await mediator.Send(new GetAssetByIdQuery(id));

        dto.Should().NotBeNull();
        dto.Id.Should().Be(id);
        dto.Name.Should().Be("lookup-me");
        dto.Description.Should().Be("desc");
        dto.Owner.Should().Be("owner@co.com");
        dto.Environment.Should().Be("Prod");
        dto.Type.Should().Be("Server");
    }

    [Fact]
    public async Task GetAssetById_UnknownId_ThrowsNotFoundException()
    {
        using var sp = TestServiceProvider.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var act = () => mediator.Send(new GetAssetByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
