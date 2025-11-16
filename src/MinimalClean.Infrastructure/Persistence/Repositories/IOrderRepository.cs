using MinimalClean.Application.Orders.Dtos;

namespace MinimalClean.Infrastructure.Persistence.Repositories;

public interface IOrderRepository
{
    Task<Domain.Orders.Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Domain.Orders.Order order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    Task<(IReadOnlyList<OrderListItemDto> Items, int TotalCount)> ListAsync(
        int pageNumber, int pageSize, string? sortBy, bool desc, CancellationToken ct);
}