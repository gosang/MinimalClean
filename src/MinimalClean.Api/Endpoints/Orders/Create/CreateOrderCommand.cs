using MinimalClean.Application.Abstractions;

namespace MinimalClean.Api.Endpoints.Orders.Create;

public record CreateOrderCommand(string CustomerName, decimal Total) : ICommand<Guid>;
