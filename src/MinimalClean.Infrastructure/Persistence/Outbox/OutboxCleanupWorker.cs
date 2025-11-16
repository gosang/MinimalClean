using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MinimalClean.Infrastructure.Persistence.Outbox;

public class OutboxCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<OutboxCleanupWorker> _logger;
    private readonly TimeSpan _ttl = TimeSpan.FromDays(7);

    public OutboxCleanupWorker(IServiceProvider sp, ILogger<OutboxCleanupWorker> logger)
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
                var expired = await db.OutboxMessages
                    .Where(m => m.Published && m.PublishedUtc < cutoff)
                    .ToListAsync(stoppingToken);

                if (expired.Any())
                {
                    db.OutboxMessages.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Cleaned up {Count} old outbox messages", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox cleanup failed");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // run hourly
        }
    }
}