namespace Vitreous.Onboarding.Application.Common;

public sealed class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string[]? Details { get; set; }
}
