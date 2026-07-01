using Vitreous.Onboarding.Domain.Enums;



namespace Vitreous.Onboarding.Application.Authorization;



/// <summary>

/// Resolved authorization context for a user.

/// </summary>

public sealed class AuthorizationContext

{

    public required Department Department { get; init; }

    public required bool IsAdmin { get; init; }

    public required bool HasAdminOverrideFlag { get; init; }

}

