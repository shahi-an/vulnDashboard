using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VulnTrack.Api.Tests.Infrastructure;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Features.Assets.Commands;
using VulnTrack.Application.Features.Assets.Queries;
using VulnTrack.Domain.Enums;
using Xunit;

namespace VulnTrack.Api.Tests.Controllers;

public sealed class AssetsControllerTests : IDisposable
{
    private readonly VulnTrackWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public AssetsControllerTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ── GET /api/assets ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_EmptyDatabase_Returns200WithEmptyArray()
    {
        var response = await _client.GetAsync("/api/assets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AssetDto[]>();
        body.Should().NotBeNull();
        body!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithAssets_ReturnsAll()
    {
        await SeedAssetAsync("server-a", AssetType.Server);
        await SeedAssetAsync("server-b", AssetType.CloudResource);

        var response = await _client.GetAsync("/api/assets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AssetDto[]>();
        body!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_SearchFilter_ReturnsMatchingAssets()
    {
        await SeedAssetAsync("db-prod-01", AssetType.Server);
        await SeedAssetAsync("web-prod-01", AssetType.WebApplication);

        var response = await _client.GetAsync("/api/assets?search=db");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AssetDto[]>();
        body!.Should().HaveCount(1);
        body![0].Name.Should().Be("db-prod-01");
    }

    // ── GET /api/assets/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        var id = await SeedAssetAsync("lookup-server", AssetType.Server);

        var response = await _client.GetAsync($"/api/assets/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AssetDto>();
        body!.Name.Should().Be("lookup-server");
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/assets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/assets ──────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidPayload_Returns201WithId()
    {
        var response = await _client.PostAsJsonAsync("/api/assets",
            new CreateAssetCommand("new-server", AssetType.Server, "A server", "owner@co.com", "Production"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/assets",
            new CreateAssetCommand("", AssetType.Server, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT /api/assets/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingAsset_Returns204()
    {
        var id = await SeedAssetAsync("original-name", AssetType.Server);

        var response = await _client.PutAsJsonAsync($"/api/assets/{id}",
            new { Name = "updated-name", Type = "CloudResource", Description = (string?)null, Owner = (string?)null, Environment = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_UnknownId_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/api/assets/{Guid.NewGuid()}",
            new { Name = "x", Type = "Server", Description = (string?)null, Owner = (string?)null, Environment = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/assets/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingAsset_Returns204()
    {
        var id = await SeedAssetAsync("to-delete", AssetType.Server);

        var response = await _client.DeleteAsync($"/api/assets/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/assets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> SeedAssetAsync(string name, AssetType type)
    {
        var response = await _client.PostAsJsonAsync("/api/assets",
            new CreateAssetCommand(name, type, null, null, null));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
