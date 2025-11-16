using MinimalClean.Domain.Common;

namespace MinimalClean.Application.Abstractions;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent @event, CancellationToken ct);
}