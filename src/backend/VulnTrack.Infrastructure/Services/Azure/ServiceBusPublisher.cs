using System.Text.Json;
using Azure.Messaging.ServiceBus;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Infrastructure.Services.Azure;

internal sealed class ServiceBusPublisher(ServiceBusClient serviceBusClient) : IServiceBusPublisher
{
    public async Task PublishAsync<T>(string topicOrQueue, T message, CancellationToken cancellationToken = default)
        where T : class
    {
        var sender = serviceBusClient.CreateSender(topicOrQueue);
        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var sbMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString()
        };

        await sender.SendMessageAsync(sbMessage, cancellationToken);
    }
}
