namespace Vitreous.Onboarding.Application.Options;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Provider { get; set; } = "SMTP";

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "Vitreous Onboarding";

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SendGridApiKey { get; set; } = string.Empty;
}
