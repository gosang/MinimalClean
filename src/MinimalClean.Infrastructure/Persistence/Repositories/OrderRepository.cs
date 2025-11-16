using Microsoft.EntityFrameworkCore;
using MinimalClean.Application.Orders.Dtos;
using MinimalClean.Domain.Orders;

namespace MinimalClean.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct) =>
        await _db.Orders.AddAsync(order, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    public async Task<(IReadOnlyList<OrderListItemDto> Items, int TotalCount)> ListAsync(
        int pageNumber, int pageSize, string? sortBy, bool desc, CancellationToken ct)
    {
        var query = _db.Orders.AsNoTracking();

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "customername" => desc ? query.OrderByDescending(o => o.CustomerName) : query.OrderBy(o => o.CustomerName),
            "total" => desc ? query.OrderByDescending(o => o.Total) : query.OrderBy(o => o.Total),
            "status" => desc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            _ => desc ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListItemDto(o.Id, o.CustomerName, o.Total, o.Status.ToString()))
            .ToListAsync(ct);

        return (items, total);
    }
}