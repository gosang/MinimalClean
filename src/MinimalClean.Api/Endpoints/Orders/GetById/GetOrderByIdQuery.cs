using MinimalClean.Application.Abstractions;

namespace MinimalClean.Api.Endpoints.Orders.GetById;

public record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto?>;
