using MediatR;
using VulnTrack.Domain.Common;

namespace VulnTrack.Application.Common.DomainEvents;

/// <summary>
/// Wraps a domain event so it can be dispatched through the MediatR pipeline.
/// Handlers implement INotificationHandler&lt;DomainEventNotification&lt;TEvent&gt;&gt;.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
