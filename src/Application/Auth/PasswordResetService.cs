using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Options;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Auth;

public sealed class PasswordResetService(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IEmailService emailService,
    IOptions<PasswordResetOptions> passwordResetOptions,
    ILogger<PasswordResetService> logger) : IPasswordResetService
{
    private const int TokenByteLength = 64;

    public const string SuccessMessage =
        "If an account exists for this email, a reset link has been sent.";

    private readonly PasswordResetOptions _options = passwordResetOptions.Value;

    public async Task<RecoveryResponse> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = AuthValidation.ValidateAndNormalizeEmail(email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is not null && user.IsActive)
        {
            var resetToken = await CreateResetTokenAsync(user, cancellationToken);
            await TrySendResetEmailAsync(user.Email ?? normalizedEmail, resetToken, cancellationToken);
        }

        return new RecoveryResponse { Message = SuccessMessage };
    }

    private async Task<PasswordResetToken> CreateResetTokenAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        await passwordResetTokenRepository.InvalidateActiveForUserAsync(user.Id, utcNow, cancellationToken);

        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = GenerateSecureToken(),
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddMinutes(_options.ResolveTokenTtlMinutes()),
        };

        await passwordResetTokenRepository.AddAsync(token, cancellationToken);
        return token;
    }

    private async Task TrySendResetEmailAsync(
        string email,
        PasswordResetToken resetToken,
        CancellationToken cancellationToken)
    {
        var resetLink = BuildResetLink(resetToken.Token);
        if (resetLink is null)
        {
            return;
        }

        var expiryMinutes = _options.ResolveTokenTtlMinutes();

        try
        {
            await emailService.SendEmailAsync(
                email,
                PasswordResetEmailContent.Subject,
                PasswordResetEmailContent.BuildHtmlBody(resetLink, expiryMinutes),
                cancellationToken);

            logger.LogInformation(
                "Password reset email sent successfully to {Email}.",
                RecipientMask.MaskEmail(email));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send password reset email to {Email}.",
                RecipientMask.MaskEmail(email));
        }
    }

    private string? BuildResetLink(string token)
    {
        if (string.IsNullOrWhiteSpace(_options.FrontendBaseUrl))
        {
            logger.LogError(
                "Password reset email was not sent because {Setting} is not configured.",
                $"{PasswordResetOptions.SectionName}:FrontendBaseUrl");
            return null;
        }

        var baseUrl = _options.FrontendBaseUrl.TrimEnd('/');
        var path = _options.ResetPasswordPath.Trim('/');
        return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}";
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
