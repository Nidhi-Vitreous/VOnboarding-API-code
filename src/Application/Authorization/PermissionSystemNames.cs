namespace Vitreous.Onboarding.Application.Authorization;

/// <summary>
/// Stable permission identifiers stored in the database and used for authorization policies.
/// </summary>
public static class PermissionSystemNames
{
    public const string UsersRead = "users.read";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";

    public const string RolesRead = "roles.read";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";
    public const string RolesDelete = "roles.delete";

    public const string MerchantRead = "merchant.read";
    public const string MerchantCreate = "merchant.create";
    public const string MerchantUpdate = "merchant.update";
    public const string MerchantDelete = "merchant.delete";
    public const string MerchantApplicationApprove = "merchant.application.approve";
    public const string MerchantApplicationReject = "merchant.application.reject";
    public const string MerchantApplicationHold = "merchant.application.hold";
    public const string MerchantApplicationComplete = "merchant.application.complete";
    public const string MerchantOrderApprove = "merchant.order.approve";
    public const string MerchantOrderReject = "merchant.order.reject";
    public const string MerchantOrderHold = "merchant.order.hold";
    public const string MerchantOrderComplete = "merchant.order.complete";

    public const string DashboardView = "dashboard.view";

    public const string OnboardingRead = "onboarding.read";
    public const string OnboardingCreate = "onboarding.create";
    public const string OnboardingRefile = "onboarding.refile";
    public const string OnboardingApprove = "onboarding.approve";
    public const string OnboardingReject = "onboarding.reject";
    public const string OnboardingHold = "onboarding.hold";
    public const string OnboardingResume = "onboarding.resume";
    public const string OnboardingBlock = "onboarding.block";
    public const string OnboardingSubmit = "onboarding.submit";
    public const string OnboardingResolve = "onboarding.resolve";
    public const string OnboardingBlockInitiate = "onboarding.block.initiate";

    public static readonly string[] All =
    [
        UsersRead, UsersCreate, UsersUpdate, UsersDelete,
        RolesRead, RolesCreate, RolesUpdate, RolesDelete,
        MerchantRead, MerchantCreate, MerchantUpdate, MerchantDelete,
        MerchantApplicationApprove, MerchantApplicationReject,
        MerchantApplicationHold, MerchantApplicationComplete,
        MerchantOrderApprove, MerchantOrderReject,
        MerchantOrderHold, MerchantOrderComplete,
        DashboardView,
        OnboardingRead, OnboardingCreate, OnboardingRefile, OnboardingApprove,
        OnboardingReject, OnboardingHold, OnboardingResume, OnboardingBlock,
        OnboardingSubmit, OnboardingResolve, OnboardingBlockInitiate,
    ];
}
