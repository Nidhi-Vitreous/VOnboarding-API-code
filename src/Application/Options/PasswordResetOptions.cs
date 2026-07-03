namespace Vitreous.Onboarding.Application.Options;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";
    public const int MinTokenTtlMinutes = 15;
    public const int MaxTokenTtlMinutes = 30;
    public const int DefaultTokenTtlMinutes = 20;

    public int TokenTtlMinutes { get; set; } = DefaultTokenTtlMinutes;

    public int ResolveTokenTtlMinutes() =>
        TokenTtlMinutes is >= MinTokenTtlMinutes and <= MaxTokenTtlMinutes
            ? TokenTtlMinutes
            : DefaultTokenTtlMinutes;
}
