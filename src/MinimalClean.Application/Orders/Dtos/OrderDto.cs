namespace MinimalClean.Application.Orders.Dtos;

public record OrderDto(Guid Id, string CustomerName, decimal Total, string Status);