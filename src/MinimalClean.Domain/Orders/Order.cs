using MinimalClean.Domain.Common;
using MinimalClean.Domain.Orders.Events;

namespace MinimalClean.Domain.Orders;

public class Order : HasDomainEvents
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string CustomerName { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public Order(string customerName, decimal total)
    {
        CustomerName = customerName;
        Total = total;
        Raise(new OrderCreated(Id, CustomerName));
    }

    public void MarkPaid() => Status = OrderStatus.Paid;
}

public enum OrderStatus { Pending, Paid, Cancelled }