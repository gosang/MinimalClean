using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MinimalClean.Infrastructure.Persistence.Inbox;

public class InboxCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<InboxCleanupWorker> _logger;
    private readonly TimeSpan _ttl = TimeSpan.FromDays(30); // keep deduplication records for 30 days

    public InboxCleanupWorker(IServiceProvider sp, ILogger<InboxCleanupWorker> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoff = DateTime.UtcNow - _ttl;
                var expired = await db.InboxRecords
                    .Where(r => r.ReceivedUtc < cutoff)
                    .ToListAsync(stoppingToken);

                if (expired.Any())
                {
                    db.InboxRecords.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Cleaned up {Count} old inbox records", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inbox cleanup failed");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken); // run every 6 hours
        }
    }
}