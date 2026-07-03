using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Options;

namespace Vitreous.Onboarding.Infrastructure.Email;

internal static class EmailServiceCollectionExtensions
{
    internal static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        var provider = configuration[$"{EmailSettings.SectionName}:Provider"] ?? "SMTP";

        switch (provider.Trim().ToUpperInvariant())
        {
            case "SENDGRID":
                services.AddSingleton<IEmailService, SendGridEmailService>();
                break;
            case "SMTP":
                services.AddSingleton<IEmailService, SmtpEmailService>();
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported email provider '{provider}'. Use SMTP or SendGrid.");
        }

        return services;
    }
}
