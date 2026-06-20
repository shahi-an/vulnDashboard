using MediatR;
using Microsoft.Extensions.Logging;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Application.Common.Behaviours;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId;

        logger.LogInformation("VulnTrack Request: {RequestName} by {UserId} {@Request}",
            requestName, userId, request);

        try
        {
            var response = await next();
            logger.LogInformation("VulnTrack Response: {RequestName} completed", requestName);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VulnTrack Request Failed: {RequestName} by {UserId}",
                requestName, userId);
            throw;
        }
    }
}
