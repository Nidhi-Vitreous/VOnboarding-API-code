namespace Vitreous.Onboarding.Application.Auth;

public static class PasswordResetEmailContent
{
    public const string Subject = "Password Reset Request";

    public static string BuildHtmlBody(string resetLink, int expiryMinutes) =>
        $"""
        <p>We received a request to reset your password.</p>
        <p><a href="{resetLink}">Reset your password</a></p>
        <p>This link expires in {expiryMinutes} minutes.</p>
        <p>If you did not request a password reset, you can safely ignore this email.</p>
        """;
}
