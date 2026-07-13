using LOS.Application.Email;
using LOS.Application.Options;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LOS.Infrastructure.Email;

public class SendGridEmailService(
    IOptions<EmailOptions> options,
    ILogger<SendGridEmailService> logger
) : IEmailService
{
    private readonly SendGridSettings _settings = options.Value.SendGrid;

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            logger.LogError(
                "SendGrid API key is not configured — check the Email__SendGrid__ApiKey app setting."
            );
            throw new InvalidOperationException("SendGrid is not configured.");
        }

        var client = new SendGridClient(_settings.ApiKey);
        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var msg = MailHelper.CreateSingleEmail(
            from,
            new EmailAddress(toEmail),
            subject,
            null,
            htmlBody
        );
        var response = await client.SendEmailAsync(msg, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(ct);
            logger.LogError(
                "SendGrid send to {Email} failed with status {Status}: {Body}",
                toEmail,
                response.StatusCode,
                body
            );
            throw new InvalidOperationException(
                $"SendGrid rejected the email ({response.StatusCode})."
            );
        }

        logger.LogInformation("SendGrid: email sent successfully to {Email}", toEmail);
    }
}
