using FluentAssertions;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;
using Xunit;

namespace VulnTrack.Domain.Tests;

public sealed class AssetTests
{
    [Fact]
    public void Create_SetsExpectedProperties()
    {
        var asset = Asset.Create(
            name: "db-prod-01",
            type: AssetType.Server,
            createdBy: "creator",
            description: "Primary database",
            owner: "infra@co.com",
            environment: "Production");

        asset.Name.Should().Be("db-prod-01");
        asset.Type.Should().Be(AssetType.Server);
        asset.CreatedBy.Should().Be("creator");
        asset.Description.Should().Be("Primary database");
        asset.Owner.Should().Be("infra@co.com");
        asset.Environment.Should().Be("Production");
        asset.Id.Should().NotBe(Guid.Empty);
        asset.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_OptionalFieldsDefaultToNull()
    {
        var asset = Asset.Create("server-01", AssetType.Server, "creator");

        asset.Description.Should().BeNull();
        asset.Owner.Should().BeNull();
        asset.Environment.Should().BeNull();
    }

    [Fact]
    public void Update_ChangesAllMutableFields()
    {
        var asset = Asset.Create("old-name", AssetType.Server, "creator");

        asset.Update("new-name", AssetType.CloudResource, "new description", "owner@co.com", "Staging", "updater");

        asset.Name.Should().Be("new-name");
        asset.Type.Should().Be(AssetType.CloudResource);
        asset.Description.Should().Be("new description");
        asset.Owner.Should().Be("owner@co.com");
        asset.Environment.Should().Be("Staging");
        asset.UpdatedBy.Should().Be("updater");
        asset.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_NullableFieldsCanBeCleared()
    {
        var asset = Asset.Create("server-01", AssetType.Server, "creator", "desc", "owner", "env");

        asset.Update("server-01", AssetType.Server, null, null, null, "updater");

        asset.Description.Should().BeNull();
        asset.Owner.Should().BeNull();
        asset.Environment.Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_SetsDeletedFlags()
    {
        var asset = Asset.Create("server-01", AssetType.Server, "creator");

        asset.MarkDeleted("deleter");

        asset.IsDeleted.Should().BeTrue();
        asset.DeletedBy.Should().Be("deleter");
        asset.DeletedAt.Should().NotBeNull();
    }
}
