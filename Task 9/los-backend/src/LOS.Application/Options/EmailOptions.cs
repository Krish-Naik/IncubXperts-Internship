namespace LOS.Application.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "FakeSmtp";

    public FakeSmtpSettings FakeSmtp { get; set; } = new();

    public SendGridSettings SendGrid { get; set; } = new();
}

public class FakeSmtpSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class SendGridSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;
}
