using MinimalClean.Application.Abstractions;
using MinimalClean.Domain.Orders;
using MinimalClean.Infrastructure.Persistence.Repositories;

namespace MinimalClean.Api.Endpoints.Orders.Create;

public class CreateOrderHandler : IHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repo;

    public CreateOrderHandler(IOrderRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = new Order(cmd.CustomerName, cmd.Total);
        await _repo.AddAsync(order, ct);
        await _repo.SaveChangesAsync(ct);
        return order.Id;
    }
}