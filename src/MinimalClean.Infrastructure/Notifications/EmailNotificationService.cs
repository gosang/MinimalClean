using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MinimalClean.Infrastructure.Notifications;

public interface IEmailNotificationService
{
    Task SendAlertAsync(string subject, string body, CancellationToken ct);
}

public class EmailNotificationService : IEmailNotificationService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _toEmail;

    public EmailNotificationService(IConfiguration config)
    {
        _apiKey = config["SendGrid:ApiKey"]!;
        _fromEmail = config["SendGrid:FromEmail"]!;
        _toEmail = config["SendGrid:ToEmail"]!;
    }

    public async Task SendAlertAsync(string subject, string body, CancellationToken ct)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail, "RabbitMQ DLQ Monitor");
        var to = new EmailAddress(_toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

        await client.SendEmailAsync(msg, ct);
    }
}
