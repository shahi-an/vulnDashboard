using Microsoft.Extensions.Logging;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Infrastructure.Services.Dev;

internal sealed class StubServiceBusPublisher(ILogger<StubServiceBusPublisher> logger) : IServiceBusPublisher
{
    public Task PublishAsync<T>(string topicOrQueue, T message,
        CancellationToken cancellationToken = default) where T : class
    {
        logger.LogInformation("[DEV] ServiceBus publish suppressed → Queue: {Queue} Type: {Type}",
            topicOrQueue, typeof(T).Name);
        return Task.CompletedTask;
    }
}
