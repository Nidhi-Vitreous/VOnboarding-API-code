namespace Vitreous.Onboarding.Application.Interfaces;

public interface IEmailService
{
    /// <param name="body">HTML email body.</param>
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
