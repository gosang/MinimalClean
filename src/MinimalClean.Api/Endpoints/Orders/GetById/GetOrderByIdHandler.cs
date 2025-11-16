using MinimalClean.Application.Abstractions;
using MinimalClean.Infrastructure.Persistence.Repositories;

namespace MinimalClean.Api.Endpoints.Orders.GetById;

public class GetOrderByIdHandler : IHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _repo;
    public GetOrderByIdHandler(IOrderRepository repo) => _repo = repo;

    public async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(query.Id, ct);
        return order is null
            ? null
            : new OrderDto(order.Id, order.CustomerName, order.Total, order.Status.ToString());
    }
}