using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Infrastructure.Data;
using VulnTrack.Infrastructure.Services.Azure;
using VulnTrack.Infrastructure.Services.Graph;
using VulnTrack.Infrastructure.Services.Identity;
using VulnTrack.Infrastructure.Settings;

namespace VulnTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework / Azure SQL
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Settings
        services.Configure<AzureStorageSettings>(configuration.GetSection(AzureStorageSettings.SectionName));
        services.Configure<ServiceBusSettings>(configuration.GetSection(ServiceBusSettings.SectionName));

        // Azure clients (using DefaultAzureCredential for Managed Identity)
        var credential = new DefaultAzureCredential();
        var storageSettings = configuration.GetSection(AzureStorageSettings.SectionName).Get<AzureStorageSettings>()!;
        var sbSettings = configuration.GetSection(ServiceBusSettings.SectionName).Get<ServiceBusSettings>()!;

        services.AddSingleton(new BlobServiceClient(
            new Uri($"https://{storageSettings.AccountName}.blob.core.windows.net"),
            credential));

        services.AddSingleton(new ServiceBusClient(
            $"{sbSettings.Namespace}.servicebus.windows.net",
            credential));

        // Microsoft Graph
        services.AddSingleton(new GraphServiceClient(credential,
            ["https://graph.microsoft.com/.default"]));

        // Application services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
        services.AddScoped<IGraphService, GraphService>();

        return services;
    }
}
