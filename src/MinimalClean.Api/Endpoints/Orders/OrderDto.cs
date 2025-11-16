namespace MinimalClean.Api.Endpoints.Orders;

public record OrderDto(Guid Id, string CustomerName, decimal Total, string Status);