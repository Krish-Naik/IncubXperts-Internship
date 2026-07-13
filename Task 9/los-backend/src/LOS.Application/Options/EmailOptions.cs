namespace LOS.Application.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Mailtrap";

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "Loan Origination System";

    public bool FailSilently { get; set; }

    public string SendGridApiKey { get; set; } = string.Empty;

    public string MailtrapHost { get; set; } = "sandbox.smtp.mailtrap.io";

    public int MailtrapPort { get; set; } = 587;

    public string MailtrapUsername { get; set; } = string.Empty;

    public string MailtrapPassword { get; set; } = string.Empty;

    public bool MailtrapUseSsl { get; set; } = true;
}
