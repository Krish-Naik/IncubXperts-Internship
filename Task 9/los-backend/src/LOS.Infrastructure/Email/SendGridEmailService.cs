using LOS.Application.Email;
using LOS.Application.Options;
using Microsoft.Extensions.Logging;
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
            throw new InvalidOperationException(
                "SendGrid API key is not configured. "
                    + "Set Email__SendGrid__ApiKey in Azure App Service."
            );
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException(
                "SendGrid sender email is not configured. "
                    + "Set Email__SendGrid__FromEmail in Azure App Service."
            );
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        }

        var client = new SendGridClient(_settings.ApiKey);

        var message = MailHelper.CreateSingleEmail(
            new EmailAddress(_settings.FromEmail, _settings.FromName),
            new EmailAddress(toEmail),
            subject,
            plainTextContent: null,
            htmlContent: htmlBody
        );

        var response = await client.SendEmailAsync(message, ct);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = response.Body is null
                ? string.Empty
                : await response.Body.ReadAsStringAsync(ct);

            logger.LogError(
                "SendGrid rejected email. StatusCode: {StatusCode}; "
                    + "Recipient: {Recipient}; Response: {Response}",
                (int)response.StatusCode,
                toEmail,
                responseBody
            );

            throw new InvalidOperationException(
                $"SendGrid rejected email delivery with HTTP {(int)response.StatusCode}."
            );
        }

        logger.LogInformation(
            "SendGrid accepted email. StatusCode: {StatusCode}; Recipient: {Recipient}",
            (int)response.StatusCode,
            toEmail
        );
    }
}
