using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Application.Features.Vulnerabilities.Commands;
using VulnTrack.Api.Tests.Infrastructure;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;
using Xunit;

namespace VulnTrack.Api.Tests.Controllers;

/// <summary>
/// Integration tests for <c>/api/vulnerabilities</c>.
/// Each test spins up its own <see cref="VulnTrackWebApplicationFactory"/> so that
/// every test starts with a clean, isolated InMemory database.
/// </summary>
public sealed class VulnerabilitiesControllerTests : IDisposable
{
    private readonly VulnTrackWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public VulnerabilitiesControllerTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ── GET /api/vulnerabilities ─────────────────────────────────────────────

    [Fact]
    public async Task GetAll_EmptyDatabase_Returns200WithZeroTotalCount()
    {
        var response = await _client.GetAsync("/api/vulnerabilities");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<object>>();
        body.Should().NotBeNull();
        body!.TotalCount.Should().Be(0);
        body.Items.Should().BeEmpty();
    }

    // ── POST /api/vulnerabilities ────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidPayload_Returns201WithId()
    {
        var sourceId = await SeedSourceAsync();

        var response = await _client.PostAsJsonAsync("/api/vulnerabilities", BuildCreateCommand(sourceId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Create_ValidPayload_LocationHeaderPointsToNewResource()
    {
        var sourceId = await SeedSourceAsync();

        var response = await _client.PostAsJsonAsync("/api/vulnerabilities", BuildCreateCommand(sourceId));

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/vulnerabilities/");
    }

    [Fact]
    public async Task Create_EmptyServerName_Returns400()
    {
        var sourceId = await SeedSourceAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/vulnerabilities",
            BuildCreateCommand(sourceId) with { ServerName = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_InvalidServerIp_Returns400()
    {
        var sourceId = await SeedSourceAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/vulnerabilities",
            BuildCreateCommand(sourceId) with { ServerIp = "not-an-ip" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/vulnerabilities/{id} ────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        var sourceId = await SeedSourceAsync();
        var id = await CreateVulnerabilityAsync(sourceId);

        var response = await _client.GetAsync($"/api/vulnerabilities/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/vulnerabilities/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PATCH /api/vulnerabilities/{id}/status ───────────────────────────────

    [Fact]
    public async Task UpdateStatus_ExistingId_Returns204()
    {
        var sourceId = await SeedSourceAsync();
        var id = await CreateVulnerabilityAsync(sourceId);

        var command = new UpdateVulnerabilityStatusCommand(id, VulnerabilityStatus.InProgress, "Started");
        var response = await _client.PatchAsJsonAsync($"/api/vulnerabilities/{id}/status", command);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateStatus_UnknownId_Returns404()
    {
        var unknownId = Guid.NewGuid();
        var command = new UpdateVulnerabilityStatusCommand(unknownId, VulnerabilityStatus.InProgress, null);

        var response = await _client.PatchAsJsonAsync($"/api/vulnerabilities/{unknownId}/status", command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatus_MismatchedRouteAndBodyId_Returns400()
    {
        var routeId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var command = new UpdateVulnerabilityStatusCommand(bodyId, VulnerabilityStatus.InProgress, null);

        var response = await _client.PatchAsJsonAsync($"/api/vulnerabilities/{routeId}/status", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> SeedSourceAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var source = VulnerabilitySource.Create("Integration Test Source", "seeder");
        db.VulnerabilitySources.Add(source);
        await db.SaveChangesAsync();
        return source.Id;
    }

    private async Task<Guid> CreateVulnerabilityAsync(Guid sourceId)
    {
        var response = await _client.PostAsJsonAsync("/api/vulnerabilities", BuildCreateCommand(sourceId));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static CreateVulnerabilityCommand BuildCreateCommand(Guid sourceId) =>
        new(
            ServerName: "integration-test-server",
            ServerIp: "192.168.1.100",
            VulnerabilityType: VulnerabilityType.MissingPatch,
            Severity: Severity.High,
            Description: "Integration test vulnerability",
            SourceId: sourceId,
            Solution: null,
            CveId: null,
            CvssScore: null,
            DiscoveredAt: null,
            UploadBatchId: null);
}
