using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalClean.Infrastructure.Notifications;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace MinimalClean.Infrastructure.Persistence.Inbox;

public class DlqBatchConsumer : BackgroundService
{
    private readonly ConnectionFactory _connection;
    private readonly ILogger<DlqBatchConsumer> _logger;
    private readonly IEmailNotificationService _emailService;

    // Thread-safe collection for DLQ messages
    private readonly ConcurrentBag<string> _dlqMessages = new();

    public DlqBatchConsumer(ConnectionFactory connection, ILogger<DlqBatchConsumer> logger, IEmailNotificationService emailService)
    {
        _connection = connection;
        _logger = logger;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken: stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync("domain_events.dlx", ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);

        var dlq = await channel.QueueDeclareAsync(
            queue: "domain_events.dlq",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(dlq.QueueName, "domain_events.dlx", "", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
                var messageId = ea.BasicProperties.MessageId ?? "(no id)";
                var type = ea.BasicProperties.Type ?? "(unknown type)";

                var entry = $"Id={messageId}, Type={type}, Payload={payload}";
                _dlqMessages.Add(entry);

                _logger.LogWarning("DLQ message captured for batch alert: {Entry}", entry);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture DLQ message");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(dlq.QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);

        // Periodically send batch alerts
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

            if (_dlqMessages.IsEmpty) continue;

            var batch = string.Join("\n\n", _dlqMessages.ToArray());
            _dlqMessages.Clear();

            await _emailService.SendAlertAsync(
                subject: $"DLQ Batch Alert ({DateTime.UtcNow})",
                body: $"The following messages were captured in DLQ:\n\n{batch}",
                ct: stoppingToken);

            _logger.LogInformation("Sent DLQ batch alert with {Count} messages", batch.Split('\n').Length);
        }
    }
}