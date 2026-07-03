namespace Vitreous.Onboarding.Application.Common;

public static class RecipientMask
{
    public static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        return atIndex <= 1 ? "***" : $"{email[0]}***{email[(atIndex - 1)..]}";
    }
}
