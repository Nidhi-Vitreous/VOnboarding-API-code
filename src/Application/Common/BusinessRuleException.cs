namespace Vitreous.Onboarding.Application.Common;

public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message, params string[] details)
        : base(message)
    {
        Details = details.Length > 0 ? details : null;
    }

    public string[]? Details { get; }
}
