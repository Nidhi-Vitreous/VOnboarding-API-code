using Vitreous.Onboarding.Domain.Enums;



namespace Vitreous.Onboarding.Application.Authorization;



/// <summary>

/// Maps onboarding request status transitions to RBAC permissions.

/// </summary>

public static class OnboardingStatusPermissions

{

    public static OnboardingPermission? GetRequiredPermission(

        OnboardingRequestStatus status,

        OnboardingRequestStatus? previousStatus)

    {

        if (previousStatus == OnboardingRequestStatus.OnHold

            && status is not OnboardingRequestStatus.OnHold)

        {

            return OnboardingPermission.Resume;

        }



        return status switch

        {

            OnboardingRequestStatus.Submitted => OnboardingPermission.Submit,

            OnboardingRequestStatus.Approved => OnboardingPermission.Approve,

            OnboardingRequestStatus.Rejected => OnboardingPermission.Reject,

            OnboardingRequestStatus.OnHold => OnboardingPermission.Hold,

            OnboardingRequestStatus.Blocked => OnboardingPermission.Block,

            OnboardingRequestStatus.BlockInitiated => OnboardingPermission.InitiateBlock,

            OnboardingRequestStatus.Resolved => OnboardingPermission.Resolve,

            _ => null,

        };

    }

}

