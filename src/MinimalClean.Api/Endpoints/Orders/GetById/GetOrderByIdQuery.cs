using MinimalClean.Application.Abstractions;
using MinimalClean.Application.Orders.Dtos;

namespace MinimalClean.Api.Endpoints.Orders.GetById;

public record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto?>;
