using Microsoft.Extensions.Logging;
using MinimalClean.Application.Abstractions;
using MinimalClean.Domain.Orders.Events;

namespace MinimalClean.Application.Orders.Commands;

public class OrderCreatedHandler : IDomainEventHandler<OrderCreated>
{
    private readonly ILogger<OrderCreatedHandler> _logger;
    public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger) => _logger = logger;

    public Task Handle(OrderCreated @event, CancellationToken ct)
    {
        _logger.LogInformation("Order created: {OrderId} for {Customer}", @event.OrderId, @event.CustomerName);
        return Task.CompletedTask;
    }
}
