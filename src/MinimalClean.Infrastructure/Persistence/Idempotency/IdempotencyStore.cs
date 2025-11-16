using Microsoft.EntityFrameworkCore;

namespace MinimalClean.Infrastructure.Persistence.Idempotency;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly AppDbContext _db;
    public IdempotencyStore(AppDbContext db) => _db = db;

    public Task<IdempotencyRecord?> GetAsync(string key, CancellationToken ct) =>
        _db.Idempotency.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, ct);

    public async Task SaveAsync(IdempotencyRecord record, CancellationToken ct)
    {
        _db.Idempotency.Add(record);
        await _db.SaveChangesAsync(ct);
    }
}