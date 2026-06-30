namespace LOS.Application.Options;

public class SecurityOptions
{
    public const string SectionName = "Security";
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int PasswordResetTokenHours { get; set; } = 2;
    public int InviteTokenHours { get; set; } = 48;
    public int PasswordHistoryCount { get; set; } = 3;
}
