using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
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
