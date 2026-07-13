using LOS.Application.Email;
using Microsoft.Extensions.Logging;

namespace LOS.Infrastructure.Email;

public class ResilientEmailService(IEmailService inner, ILogger<ResilientEmailService> logger)
    : IEmailService
{
    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct
    )
    {
        try
        {
            await inner.SendAsync(toEmail, subject, htmlBody, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email to {Email} with subject '{Subject}'. The triggering action already completed and was not rolled back.",
                toEmail,
                subject
            );
        }
    }
}
