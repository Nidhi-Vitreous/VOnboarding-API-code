using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Options;

namespace Vitreous.Onboarding.Infrastructure.Email;

public sealed class SendGridEmailService(
    IOptions<EmailSettings> emailSettings,
    ILogger<SendGridEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = emailSettings.Value;

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SendGridApiKey))
        {
            throw new InvalidOperationException("EmailSettings:SendGridApiKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("EmailSettings:FromEmail is not configured.");
        }

        var client = new SendGridClient(_settings.SendGridApiKey);
        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var message = MailHelper.CreateSingleEmail(from, new EmailAddress(to), subject, null, body);
        var response = await client.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"SendGrid returned {(int)response.StatusCode}: {responseBody}");
        }

        logger.LogInformation("SendGrid email accepted for delivery to {Recipient}.", RecipientMask.MaskEmail(to));
    }
}
