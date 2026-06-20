using Microsoft.Extensions.Logging;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Infrastructure.Services.Dev;

internal sealed class StubGraphService(ILogger<StubGraphService> logger) : IGraphService
{
    public Task<IGraphUserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("[DEV] GetUserAsync({UserId}) suppressed", userId);
        return Task.FromResult<IGraphUserDto?>(null);
    }

    public Task<IReadOnlyList<IGraphUserDto>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("[DEV] SearchUsersAsync('{Query}') suppressed", query);
        return Task.FromResult<IReadOnlyList<IGraphUserDto>>([]);
    }

    public Task SendEmailAsync(string toEmail, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[DEV] Email suppressed → To: {To} | Subject: {Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
