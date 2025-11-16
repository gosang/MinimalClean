using System.ComponentModel.DataAnnotations;

namespace MinimalClean.Infrastructure.Persistence.Inbox;

public class InboxRecord
{
    [Key]
    public string PayloadHash { get; set; } = default!;
    public DateTime ReceivedUtc { get; set; } = DateTime.UtcNow;
}