using LOS.Application.Email;
using LOS.Application.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LOS.Infrastructure.Email;

public class FakeSmtpEmailService(
    IOptions<EmailOptions> options,
    ILogger<FakeSmtpEmailService> logger
) : IEmailService
{
    private readonly FakeSmtpSettings _settings = options.Value.FakeSmtp;

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct
    )
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("LOS Notifications", _settings.Username));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.Auto, ct);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            logger.LogInformation("Mailtrap: email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Mailtrap send FAILED for {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
    }
}
