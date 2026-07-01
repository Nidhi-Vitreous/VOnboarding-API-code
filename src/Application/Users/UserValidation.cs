using System.Net.Mail;
using Vitreous.Onboarding.Application.Common;

namespace Vitreous.Onboarding.Application.Users;

internal static class UserValidation
{
    internal static void ValidateRequest(UserCreateRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors.Add("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }
        else if (!IsValidEmail(request.Email))
        {
            errors.Add("Email format is invalid.");
        }

        if (request.RoleId == Guid.Empty)
        {
            errors.Add("Role is required.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessRuleException("Validation failed.", errors.ToArray());
        }
    }

    private static bool IsValidEmail(string email)
    {
        var trimmed = email.Trim();

        try
        {
            var address = new MailAddress(trimmed);
            return address.Address.Equals(trimmed, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
