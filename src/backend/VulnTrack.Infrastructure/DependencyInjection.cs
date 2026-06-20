using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Infrastructure.Data;
using VulnTrack.Infrastructure.Services.Azure;
using VulnTrack.Infrastructure.Services.Dev;
using VulnTrack.Infrastructure.Services.Graph;
using VulnTrack.Infrastructure.Services.Identity;
using VulnTrack.Infrastructure.Settings;

namespace VulnTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDevelopmentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3).CommandTimeout(30)));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<AzureStorageSettings>(
            configuration.GetSection(AzureStorageSettings.SectionName));
        services.Configure<ServiceBusSettings>(
            configuration.GetSection(ServiceBusSettings.SectionName));
        services.Configure<GraphSettings>(
            configuration.GetSection(GraphSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBlobStorageService, LocalBlobStorageService>();
        services.AddSingleton<IServiceBusPublisher, StubServiceBusPublisher>();
        services.AddScoped<IGraphService, StubGraphService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var storageSettings = configuration.GetSection(AzureStorageSettings.SectionName)
            .Get<AzureStorageSettings>()
            ?? throw new InvalidOperationException(
                $"Required configuration section '{AzureStorageSettings.SectionName}' is missing.");

        var sbSettings = configuration.GetSection(ServiceBusSettings.SectionName)
            .Get<ServiceBusSettings>()
            ?? throw new InvalidOperationException(
                $"Required configuration section '{ServiceBusSettings.SectionName}' is missing.");

        // ── Entity Framework / Azure SQL ───────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql
                    .EnableRetryOnFailure(maxRetryCount: 3)
                    .CommandTimeout(30)));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // ── Settings ───────────────────────────────────────────────────────
        services.Configure<AzureStorageSettings>(
            configuration.GetSection(AzureStorageSettings.SectionName));
        services.Configure<ServiceBusSettings>(
            configuration.GetSection(ServiceBusSettings.SectionName));
        services.Configure<GraphSettings>(
            configuration.GetSection(GraphSettings.SectionName));

        // ── Azure SDK clients (DefaultAzureCredential → Managed Identity) ──
        var credential = new DefaultAzureCredential();

        var blobServiceClient = new BlobServiceClient(
            new Uri($"https://{storageSettings.AccountName}.blob.core.windows.net"),
            credential);

        var serviceBusClient = new ServiceBusClient(
            $"{sbSettings.Namespace}.servicebus.windows.net",
            credential);

        services.AddSingleton(blobServiceClient);
        services.AddSingleton(serviceBusClient);

        services.AddSingleton(new GraphServiceClient(
            credential,
            ["https://graph.microsoft.com/.default"]));

        // ── Application services ───────────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
        services.AddScoped<IGraphService, GraphService>();

        return services;
    }
}
