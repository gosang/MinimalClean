using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MinimalClean.Infrastructure.Persistence.Outbox;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(IServiceProvider sp, ILogger<OutboxPublisher> logger)
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

                var pending = await db.OutboxMessages
                    .Where(m => !m.Published)
                    .OrderBy(m => m.OccurredUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                var seenKeys = new HashSet<string>();

                foreach (var msg in pending)
                {
                    if (!seenKeys.Add(msg.DeduplicationKey))
                    {
                        _logger.LogWarning("Skipping duplicate outbox message {Key}", msg.DeduplicationKey);
                        msg.Published = true; // mark as published to avoid retry
                        msg.PublishedUtc = DateTime.UtcNow;
                        continue;
                    }

                    try
                    {
                        // Deserialize event
                        var type = Type.GetType(msg.Type)!;
                        var ev = (Domain.Common.IDomainEvent)JsonSerializer.Deserialize(msg.Payload, type)!;

                        // Resolve handlers
                        var handlerType = typeof(Application.Abstractions.IDomainEventHandler<>).MakeGenericType(type);
                        var handlers = scope.ServiceProvider.GetServices(handlerType);

                        foreach (var handler in handlers)
                        {
                            var method = handlerType.GetMethod("Handle")!;
                            await (Task)method.Invoke(handler, new object[] { ev, stoppingToken })!;
                        }

                        msg.Published = true;
                        msg.PublishedUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish outbox message {MessageId}", msg.Id);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher loop failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}