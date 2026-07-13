using LOS.Application.Email;
using LOS.Application.Options;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LOS.Infrastructure.Email;

public sealed class SendGridEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        IOptions<EmailOptions> options,
        ILogger<SendGridEmailService> logger
    )
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(_options.SendGridApiKey))
        {
            throw new InvalidOperationException("Email:SendGridApiKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException("Email:FromEmail is not configured.");
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("A recipient email address is required.", nameof(toEmail));
        }

        var client = new SendGridClient(_options.SendGridApiKey);

        var message = MailHelper.CreateSingleEmail(
            new EmailAddress(_options.FromEmail, _options.FromName),
            new EmailAddress(toEmail),
            subject,
            plainTextContent: StripHtml(htmlBody),
            htmlContent: htmlBody
        );

        var response = await client.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "SendGrid rejected email. StatusCode: {StatusCode}; Recipient: {Recipient}; Response: {Response}",
                response.StatusCode,
                toEmail,
                responseBody
            );

            throw new InvalidOperationException(
                $"SendGrid rejected email delivery with status {(int)response.StatusCode}."
            );
        }

        _logger.LogInformation(
            "SendGrid accepted email. StatusCode: {StatusCode}; Recipient: {Recipient}; Subject: {Subject}",
            response.StatusCode,
            toEmail,
            subject
        );
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            html ?? string.Empty,
            "<.*?>",
            string.Empty
        );
    }
}
