using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Options;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Auth;

public sealed class PasswordResetService(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IOptions<PasswordResetOptions> passwordResetOptions) : IPasswordResetService
{
    private const int TokenByteLength = 64;

    public const string SuccessMessage =
        "If an account exists for this email, a reset link has been sent.";

    private readonly PasswordResetOptions _options = passwordResetOptions.Value;

    public async Task<ForgotPasswordResponse> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = AuthValidation.ValidateAndNormalizeEmail(email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is not null && user.IsActive)
        {
            await CreateResetTokenAsync(user, cancellationToken);
        }

        return new ForgotPasswordResponse { Message = SuccessMessage };
    }

    private async Task CreateResetTokenAsync(User user, CancellationToken cancellationToken)
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
    }

    private static string GenerateSecureToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenByteLength));
}
