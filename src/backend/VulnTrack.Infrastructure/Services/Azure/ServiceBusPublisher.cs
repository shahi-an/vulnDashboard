using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Infrastructure.Services.Azure;

internal sealed class ServiceBusPublisher(ServiceBusClient serviceBusClient)
    : IServiceBusPublisher, IAsyncDisposable
{
    // ServiceBusSender holds an AMQP link — create once per destination, not per call.
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public async Task PublishAsync<T>(
        string topicOrQueue,
        T message,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var sender = _senders.GetOrAdd(topicOrQueue, serviceBusClient.CreateSender);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var sbMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString()
        };

        await sender.SendMessageAsync(sbMessage, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();

        _senders.Clear();
    }
}
