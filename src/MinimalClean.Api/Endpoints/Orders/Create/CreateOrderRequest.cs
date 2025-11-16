namespace MinimalClean.Api.Endpoints.Orders.Create;

public record CreateOrderRequest(string CustomerName, decimal Total);
