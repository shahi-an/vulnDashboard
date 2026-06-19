namespace VulnTrack.Application.Common.Interfaces;

public interface IServiceBusPublisher
{
    Task PublishAsync<T>(string topicOrQueue, T message, CancellationToken cancellationToken = default)
        where T : class;
}
