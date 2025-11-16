using MinimalClean.Application.Abstractions;
using MinimalClean.Application.Orders.Dtos;
using MinimalClean.Infrastructure.Persistence.Repositories;

namespace MinimalClean.Api.Endpoints.Orders.List;

public class ListOrdersHandler : IHandler<ListOrdersQuery, PagedResult<OrderListItemDto>>
{
    private readonly IOrderRepository _repo;
    public ListOrdersHandler(IOrderRepository repo) => _repo = repo;

    public async Task<PagedResult<OrderListItemDto>> Handle(ListOrdersQuery query, CancellationToken ct)
    {
        var (items, total) = await _repo.ListAsync(query.PageNumber, query.PageSize, query.SortBy, query.Desc, ct);
        return new PagedResult<OrderListItemDto>(items, query.PageNumber, query.PageSize, total);
    }
}
