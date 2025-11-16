using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MinimalClean.Infrastructure.Persistence.Inbox;

public class InboxConsumer : BackgroundService
{
    private readonly ConnectionFactory _connection;
    private readonly IServiceProvider _sp;
    private readonly ILogger<InboxConsumer> _logger;

    public InboxConsumer(ConnectionFactory connection, IServiceProvider sp, ILogger<InboxConsumer> logger)
    {
        _connection = connection;
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken: stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync("domain_events", ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);

        var queue = await channel.QueueDeclareAsync("", durable: true, exclusive: true, autoDelete: true, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queue.QueueName, "domain_events", "", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var payload = Encoding.UTF8.GetString(ea.Body.ToArray());

                var headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>();
                var payloadHash = headers.TryGetValue("PayloadHash", out var h)
                    ? h is byte[] hb ? Encoding.UTF8.GetString(hb) : h?.ToString() ?? ""
                    : "";

                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Deduplication
                if (string.IsNullOrWhiteSpace(payloadHash) ||
                    await db.InboxRecords.AnyAsync(r => r.PayloadHash == payloadHash, stoppingToken))
                {
                    _logger.LogInformation("Skipped duplicate or missing-hash message");
                    return;
                }

                db.InboxRecords.Add(new InboxRecord { PayloadHash = payloadHash });
                await db.SaveChangesAsync(stoppingToken);

                var typeName = ea.BasicProperties.Type;
                var type = Type.GetType(typeName)!;
                var ev = (Domain.Common.IDomainEvent)JsonSerializer.Deserialize(payload, type)!;

                var handlerType = typeof(Application.Abstractions.IDomainEventHandler<>).MakeGenericType(type);
                var handlers = scope.ServiceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    var method = handlerType.GetMethod("Handle")!;
                    await (Task)method.Invoke(handler, new object[] { ev, stoppingToken })!;
                }

                _logger.LogInformation("Processed event {Type} with hash {Hash}", typeName, payloadHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message");
                // If you use manual acks, consider Nack here
            }
        };

        await channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}