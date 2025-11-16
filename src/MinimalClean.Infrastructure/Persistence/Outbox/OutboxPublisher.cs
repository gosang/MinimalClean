using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using System.Text.Json;

namespace MinimalClean.Infrastructure.Persistence.Outbox;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public OutboxPublisher(IServiceProvider sp, ILogger<OutboxPublisher> logger, ResiliencePipelineProvider<string> pipelineProvider)
    {
        _sp = sp;
        _logger = logger;
        _pipelineProvider = pipelineProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pipeline = _pipelineProvider.GetPipeline("outboxPublisher");

        while (!stoppingToken.IsCancellationRequested)
        {
           
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pending = await db.OutboxMessages
                .Where(m => !m.Published)
                .OrderBy(m => m.OccurredUtc)
                .Take(20)
                .ToListAsync(stoppingToken);

            var seenKeys = new HashSet<string>();
            var seenHashes = new HashSet<string>();

            foreach (var msg in pending)
            {
                //if (!seenKeys.Add(msg.DeduplicationKey))
                //{
                //    _logger.LogWarning("Skipping duplicate outbox message {Key}", msg.DeduplicationKey);
                //    msg.Published = true; // mark as published to avoid retry
                //    msg.PublishedUtc = DateTime.UtcNow;
                //    continue;
                //}

                if (!seenHashes.Add(msg.PayloadHash))
                {
                    _logger.LogWarning("Skipping duplicate outbox message {Hash}", msg.PayloadHash);
                    msg.Published = true;
                    msg.PublishedUtc = DateTime.UtcNow;
                    continue;
                }

                try
                {
                    await pipeline.ExecuteAsync(async ct =>
                    {
                        var type = Type.GetType(msg.Type)!;
                        var ev = (Domain.Common.IDomainEvent)JsonSerializer.Deserialize(msg.Payload, type)!;

                        var handlerType = typeof(Application.Abstractions.IDomainEventHandler<>).MakeGenericType(type);
                        var handlers = scope.ServiceProvider.GetServices(handlerType);

                        foreach (var handler in handlers)
                        {
                            var method = handlerType.GetMethod("Handle")!;
                            await (Task)method.Invoke(handler, [ev, ct])!;
                        }

                        msg.Published = true;
                        msg.PublishedUtc = DateTime.UtcNow;
                    }, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox message {MessageId}", msg.Id);
                }
            }

            await db.SaveChangesAsync(stoppingToken);          

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}