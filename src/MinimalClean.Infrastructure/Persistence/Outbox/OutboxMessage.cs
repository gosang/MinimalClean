namespace MinimalClean.Infrastructure.Persistence.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredUtc { get; set; }
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public bool Published { get; set; } = false;
    public DateTime? PublishedUtc { get; set; }

    // Deduplication key (e.g., hash of payload or event ID)
    public string DeduplicationKey { get; set; } = default!;
}