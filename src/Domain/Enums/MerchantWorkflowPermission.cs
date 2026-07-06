namespace Vitreous.Onboarding.Domain.Enums;

/// <summary>
/// Merchant application and order permissions enforced by the RBAC engine.
/// </summary>
public enum MerchantWorkflowPermission
{
    ApproveApplication,
    RejectApplication,
    HoldApplication,
    CompleteApplication,
    ApproveOrder,
    RejectOrder,
    HoldOrder,
    CompleteOrder,
}
