using MinimalClean.Application.Abstractions;
using MinimalClean.Domain.Orders;
using MinimalClean.Infrastructure.Persistence;
using MinimalClean.Infrastructure.Persistence.Outbox;
using MinimalClean.Infrastructure.Persistence.Repositories;
using System.Text.Json;

namespace MinimalClean.Api.Endpoints.Orders.Create;

public class CreateOrderHandler : IHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repo;
    private readonly AppDbContext _db;

    public CreateOrderHandler(IOrderRepository repo, AppDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = new Order(cmd.CustomerName, cmd.Total);
        await _repo.AddAsync(order, ct);

        // Persist domain events into Outbox
        foreach (var ev in order.DomainEvents)
        {
            var message = new OutboxMessage
            {
                OccurredUtc = ev.OccurredUtc,
                Type = ev.GetType().FullName!,
                Payload = JsonSerializer.Serialize(ev),
                DeduplicationKey = $"{ev.GetType().Name}:{order.Id}" // simple example
            };
            _db.OutboxMessages.Add(message);
        }

        await _repo.SaveChangesAsync(ct);
        order.ClearEvents();

        return order.Id;
    }
}