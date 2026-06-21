using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using VulnTrack.Api.Auth;
using VulnTrack.Api.Middleware;
using VulnTrack.Application;
using VulnTrack.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Authentication ────────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, null);
}
else
{
    builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd")
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
        .AddInMemoryTokenCaches();
}

builder.Services.AddAuthorization();

// ── Application + Infrastructure layers ──────────────────────────────────────
builder.Services.AddApplication();

if (builder.Environment.IsDevelopment())
    builder.Services.AddDevelopmentInfrastructure(builder.Configuration);
else
    builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VulnTrack API",
        Version = "v1",
        Description = "Vulnerability Management Portal REST API"
    });

    if (!builder.Environment.IsDevelopment())
    {
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        [$"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user"] = "Access VulnTrack API"
                    }
                }
            }
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" } },
                []
            }
        });
    }
});

// ── Observability (Azure Monitor + OpenTelemetry) ─────────────────────────────
if (!builder.Environment.IsDevelopment())
    builder.Services.AddOpenTelemetry().UseAzureMonitor();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Frontend", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── Health checks ─────────────────────────────────────────────────────────────
var healthChecks = builder.Services.AddHealthChecks()
    .AddDbContextCheck<VulnTrack.Infrastructure.Data.ApplicationDbContext>("database");

if (!builder.Environment.IsDevelopment())
{
    healthChecks
        .AddAzureBlobStorage(
            sp => sp.GetRequiredService<BlobServiceClient>(),
            name: "blob-storage")
        .AddAzureServiceBusQueue(
            sp => (builder.Configuration["ServiceBus:Namespace"] ?? "devlocal") + ".servicebus.windows.net",
            sp => builder.Configuration["ServiceBus:NotificationsQueue"] ?? "notifications",
            sp => new DefaultAzureCredential(),
            name: "service-bus");
}

var app = builder.Build();

// ── Apply pending EF migrations on startup ───────────────────────────────
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<VulnTrack.Infrastructure.Data.ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VulnTrack API v1");
    if (!app.Environment.IsDevelopment())
    {
        c.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
        c.OAuthUsePkce();
    }
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

// Exposed for integration tests
public partial class Program { }
