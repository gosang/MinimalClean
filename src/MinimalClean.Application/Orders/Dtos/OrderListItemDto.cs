namespace MinimalClean.Application.Orders.Dtos;

public record OrderListItemDto(Guid Id, string CustomerName, decimal Total, string Status);