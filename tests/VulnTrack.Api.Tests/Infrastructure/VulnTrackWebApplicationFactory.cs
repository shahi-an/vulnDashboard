using System.IO.Pipelines;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Infrastructure.Data;

namespace VulnTrack.Api.Tests.Infrastructure;

/// <summary>
/// Boots the real ASP.NET Core host via <see cref="Program"/> with:
/// <list type="bullet">
///   <item>EF Core InMemory database (isolated per factory instance)</item>
///   <item>Azure services replaced with no-op mocks</item>
///   <item>Auth overridden with <see cref="TestAuthHandler"/></item>
/// </list>
/// </summary>
public sealed class VulnTrackWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ── Minimal config to satisfy Infrastructure DI startup ─────────────────
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:TenantId"]             = "test-tenant",
                ["AzureAd:ClientId"]             = "test-client",
                ["AzureAd:Audience"]             = "api://test",
                ["AzureAd:Instance"]             = "https://login.microsoftonline.com/",
                ["MicrosoftGraph:BaseUrl"]        = "https://graph.microsoft.com/v1.0",
                ["MicrosoftGraph:Scopes"]         = "",
                ["AzureStorage:AccountName"]      = "teststorage",
                ["AzureStorage:AttachmentsContainer"] = "attachments",
                ["ServiceBus:Namespace"]          = "testbus",
                ["ServiceBus:VulnerabilityEventsQueue"] = "vulnerability-events",
                ["ServiceBus:NotificationsQueue"] = "notifications",
                ["Graph:SenderEmail"]             = "noreply@test.com",
                ["ConnectionStrings:DefaultConnection"] = "Server=test;Database=test",
                ["AllowedOrigins:0"]              = "http://localhost:5173",
            });
        });

        // ── Fix: .NET 10 runtime's System.Text.Json checks PipeWriter.UnflushedBytes,
        //         but the test host's ResponseBodyPipeWriter doesn't implement it.
        //         Replace IHttpResponseBodyFeature with StreamResponseBodyFeature (whose
        //         underlying PipeWriter is StreamPipeWriter which DOES implement
        //         UnflushedBytes in .NET 10+). ──────────────────────────────────────
        builder.ConfigureServices(services =>
            services.AddTransient<IStartupFilter>(_ => new ResponseBodyPipeWriterFixFilter()));

        builder.ConfigureServices(services =>
        {
            // ── Replace SQL Server DbContext with InMemory ───────────────────────
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));

            // IApplicationDbContext is already registered by AddInfrastructure;
            // the scoped factory still resolves ApplicationDbContext (now InMemory). ✓

            // ── Replace Azure application services with no-op mocks ─────────────
            services.RemoveAll<IBlobStorageService>();
            services.AddScoped<IBlobStorageService>(_ =>
            {
                var mock = new Mock<IBlobStorageService>();
                mock.Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync("https://teststorage.blob.core.windows.net/attachments/test.pdf");
                mock.Setup(b => b.GenerateSasUriAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Uri("https://teststorage.blob.core.windows.net/attachments/test.pdf?sas=1"));
                return mock.Object;
            });

            services.RemoveAll<IServiceBusPublisher>();
            services.AddSingleton<IServiceBusPublisher>(_ => Mock.Of<IServiceBusPublisher>());

            services.RemoveAll<IGraphService>();
            services.AddScoped<IGraphService>(_ => Mock.Of<IGraphService>());

            // ── Override authentication — replace JWT Bearer with test scheme ────
            services.Configure<AuthenticationOptions>(opts =>
            {
                opts.DefaultScheme              = TestAuthHandler.SchemeName;
                opts.DefaultAuthenticateScheme  = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme     = TestAuthHandler.SchemeName;
                opts.DefaultForbidScheme        = TestAuthHandler.SchemeName;
            });

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }
}

// In .NET 10 runtime (used via RollForward=LatestMajor), System.Text.Json accesses
// PipeWriter.UnflushedBytes when serialising controller responses. The test host's
// ResponseBodyPipeWriter does not implement this abstract property (throws
// NotImplementedException → wrapped as InvalidOperationException). This filter adds
// a middleware that replaces the response body feature with a wrapper whose Writer
// is created via PipeWriter.Create(stream), which returns a StreamPipeWriter that
// DOES implement UnflushedBytes in .NET 10+.
internal sealed class ResponseBodyPipeWriterFixFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        => app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                var original = context.Features.Get<IHttpResponseBodyFeature>();
                if (original is not null)
                {
                    context.Features.Set<IHttpResponseBodyFeature>(
                        new CompatResponseBodyFeature(original));
                }
                await nextMiddleware(context);
            });
            next(app);
        };
}

/// <summary>
/// Wraps IHttpResponseBodyFeature and returns a StreamPipeWriter for Writer
/// so that System.Text.Json's PipeWriter.UnflushedBytes check succeeds.
/// </summary>
internal sealed class CompatResponseBodyFeature(IHttpResponseBodyFeature inner)
    : IHttpResponseBodyFeature
{
    private PipeWriter? _writer;

    public Stream Stream => inner.Stream;

    // PipeWriter.Create returns StreamPipeWriter which implements UnflushedBytes.
    public PipeWriter Writer => _writer ??= System.IO.Pipelines.PipeWriter.Create(inner.Stream, new System.IO.Pipelines.StreamPipeWriterOptions(leaveOpen: true));

    public Task CompleteAsync() => inner.CompleteAsync();
    public void DisableBuffering() => inner.DisableBuffering();
    public Task SendFileAsync(string path, long offset, long? count, CancellationToken ct)
        => inner.SendFileAsync(path, offset, count, ct);
    public Task StartAsync(CancellationToken ct = default) => inner.StartAsync(ct);
}
