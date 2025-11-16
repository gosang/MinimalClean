using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalClean.Infrastructure.Notifications;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MinimalClean.Infrastructure.Persistence.Inbox;

public class DlqConsumer : BackgroundService
{
    private readonly ConnectionFactory _connection;
    private readonly ILogger<DlqConsumer> _logger;
    private readonly IEmailNotificationService _emailService;

    public DlqConsumer(ConnectionFactory connection, ILogger<DlqConsumer> logger, IEmailNotificationService emailService)
    {
        _connection = connection;
        _logger = logger;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken: stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Ensure DLX and DLQ exist
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

                // ✅ Log the failed message
                _logger.LogWarning("DLQ message received. Id={MessageId}, Type={Type}, Payload={Payload}",
                    messageId, type, payload);

                // 🚨 Alert integration (example: send to monitoring system)
                await SendAlertAsync(messageId, type, payload, stoppingToken);

                // 🚨 Send email alert
                await _emailService.SendAlertAsync(
                    subject: $"DLQ Alert: {type} ({messageId})",
                    body: $"A message has landed in the DLQ.\n\nId: {messageId}\nType: {type}\nPayload: {payload}",
                    ct: stoppingToken);

                // ✅ ACK so DLQ doesn’t redeliver endlessly
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process DLQ message");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(dlq.QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task SendAlertAsync(string messageId, string type, string payload, CancellationToken ct)
    {
        // Example: integrate with Slack, email, or monitoring system
        // For now, just log an "alert" entry
        _logger.LogError("🚨 ALERT: DLQ message {MessageId} of type {Type} requires investigation", messageId, type);

        // Replace with actual integration (HTTP call, SMTP, etc.)
        return Task.CompletedTask;
    }
}