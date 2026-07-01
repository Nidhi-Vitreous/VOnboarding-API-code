namespace Vitreous.Onboarding.Domain.Enums;

/// <summary>
/// Onboarding permissions enforced by the RBAC engine.
/// </summary>
public enum OnboardingPermission
{
    Read,
    Create,
    Refile,
    Approve,
    Reject,
    Hold,
    Resume,
    Block,
    Submit,
    Resolve,
    InitiateBlock,
}
