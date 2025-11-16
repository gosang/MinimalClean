using MinimalClean.Application.Abstractions;
using MinimalClean.Application.Orders.Dtos;

namespace MinimalClean.Api.Endpoints.Orders.List;

public record ListOrdersQuery(
    int PageNumber,
    int PageSize,
    string? SortBy,
    bool Desc) : IQuery<PagedResult<OrderListItemDto>>;
