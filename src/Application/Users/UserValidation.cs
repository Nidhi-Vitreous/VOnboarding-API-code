using System.Net.Mail;
using System.Text.RegularExpressions;
using Vitreous.Onboarding.Application.Common;

namespace Vitreous.Onboarding.Application.Users;

internal static partial class UserValidation
{
    [GeneratedRegex(@"^[\d\s+\-().]{7,32}$")]
    private static partial Regex PhoneNumberPattern();

    internal static void ValidateRequest(UserCreateRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            errors.Add("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            errors.Add("Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }
        else if (!IsValidEmail(request.Email))
        {
            errors.Add("Email format is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }
        else if (!IsStrongPassword(request.Password))
        {
            errors.Add("Password must be at least 8 characters and include uppercase, lowercase, digit, and symbol.");
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            errors.Add("Confirm password is required.");
        }
        else if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            errors.Add("Password and confirm password do not match.");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !IsValidPhoneNumber(request.PhoneNumber))
        {
            errors.Add("Phone number format is invalid.");
        }

        if (request.RoleIds is null || request.RoleIds.Count == 0)
        {
            errors.Add("At least one role is required.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessRuleException("Validation failed.", errors.ToArray());
        }
    }

    internal static void ValidateUpdateRequest(UserUpdateRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            errors.Add("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            errors.Add("Last name is required.");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !IsValidPhoneNumber(request.PhoneNumber))
        {
            errors.Add("Phone number format is invalid.");
        }

        if (request.RoleIds is null || request.RoleIds.Count == 0)
        {
            errors.Add("At least one role is required.");
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

    private static bool IsStrongPassword(string password)
    {
        if (password.Length < 8)
        {
            return false;
        }

        var hasUpper = false;
        var hasLower = false;
        var hasDigit = false;
        var hasSymbol = false;

        foreach (var character in password)
        {
            if (char.IsUpper(character))
            {
                hasUpper = true;
            }
            else if (char.IsLower(character))
            {
                hasLower = true;
            }
            else if (char.IsDigit(character))
            {
                hasDigit = true;
            }
            else
            {
                hasSymbol = true;
            }
        }

        return hasUpper && hasLower && hasDigit && hasSymbol;
    }

    private static bool IsValidPhoneNumber(string phoneNumber) =>
        PhoneNumberPattern().IsMatch(phoneNumber.Trim());
}
