using System.Net.Mail;
using Vitreous.Onboarding.Application.Common;

namespace Vitreous.Onboarding.Application.Auth;

internal static class AuthValidation
{
    internal static string ValidateAndNormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BusinessRuleException("Validation failed.", "Email is required.");
        }

        var trimmed = email.Trim();
        if (!IsValidEmail(trimmed))
        {
            throw new BusinessRuleException("Validation failed.", "Email format is invalid.");
        }

        return trimmed;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
