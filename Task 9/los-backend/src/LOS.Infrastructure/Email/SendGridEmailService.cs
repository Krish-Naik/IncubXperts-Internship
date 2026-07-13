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
            logger.LogWarning("SendGrid send failed with status {Status}", response.StatusCode);
    }
}
