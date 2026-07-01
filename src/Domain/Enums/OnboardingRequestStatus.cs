namespace Vitreous.Onboarding.Domain.Enums;



/// <summary>

/// Lifecycle status of an onboarding request.

/// </summary>

public enum OnboardingRequestStatus

{

    Draft,

    Submitted,

    Approved,

    Rejected,

    OnHold,

    Blocked,

    BlockInitiated,

    Resolved,

}

