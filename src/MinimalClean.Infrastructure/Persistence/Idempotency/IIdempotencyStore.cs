namespace MinimalClean.Infrastructure.Persistence.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken ct);
    Task SaveAsync(IdempotencyRecord record, CancellationToken ct);
}