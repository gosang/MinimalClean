namespace MinimalClean.Infrastructure.Persistence.Idempotency;

public class IdempotencyRecord
{
    public string Key { get; set; } = default!;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public Guid? ResultResourceId { get; set; } // e.g., Order Id
}