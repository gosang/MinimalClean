using MinimalClean.Domain.Common;

namespace MinimalClean.Domain.Orders.Events;

public class OrderCreated : IDomainEvent
{
    public Guid OrderId { get; }
    public string CustomerName { get; }
    public DateTime OccurredUtc { get; } = DateTime.UtcNow;

    public OrderCreated(Guid orderId, string customerName)
    {
        OrderId = orderId;
        CustomerName = customerName;
    }
}
