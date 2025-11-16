namespace MinimalClean.Domain.Orders;

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string CustomerName { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public Order(string customerName, decimal total)
    {
        CustomerName = customerName;
        Total = total;
    }

    public void MarkPaid() => Status = OrderStatus.Paid;
}

public enum OrderStatus { Pending, Paid, Cancelled }