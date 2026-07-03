using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Options;

namespace Vitreous.Onboarding.Infrastructure.Email;

public sealed class SmtpEmailService(
    IOptions<EmailSettings> emailSettings,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = emailSettings.Value;

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            throw new InvalidOperationException("EmailSettings:SmtpHost is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("EmailSettings:FromEmail is not configured.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _settings.SmtpHost,
            _settings.SmtpPort,
            SecureSocketOptions.StartTls,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("SMTP email accepted for delivery to {Recipient}.", RecipientMask.MaskEmail(to));
    }
}
