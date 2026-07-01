using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;
using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Application.Authorization;

public sealed class AuthorizationService(IDepartmentResolver departmentResolver) : IAuthorizationService
{
    public Task<bool> CanCreateOnboardingAsync(User user, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(user, OnboardingPermission.Create, cancellationToken);

    public Task<bool> CanApproveAsync(User user, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(user, OnboardingPermission.Approve, cancellationToken);

    public Task<bool> CanRejectAsync(User user, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(user, OnboardingPermission.Reject, cancellationToken);

    public Task<bool> CanHoldAsync(User user, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(user, OnboardingPermission.Hold, cancellationToken);

    public Task<bool> CanBlockAsync(User user, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(user, OnboardingPermission.Block, cancellationToken);

    public Task<bool> CanRefileAsync(User user, CancellationToken cancellationToken = default) =>
        HasPermissionAsync(user, OnboardingPermission.Refile, cancellationToken);

    public Task<bool> IsAdminAsync(User user, CancellationToken cancellationToken = default) =>
        EvaluateAsync(user, context => context.IsAdmin, cancellationToken);

    public Task<bool> HasPermissionAsync(
        User user,
        OnboardingPermission permission,
        CancellationToken cancellationToken = default) =>
        EvaluateAsync(
            user,
            context => context.IsAdmin || DepartmentPermissionRegistry.HasPermission(context.Department, permission),
            cancellationToken);

    public Task<bool> CanSetStatusAsync(
        User user,
        OnboardingRequestStatus status,
        OnboardingRequestStatus? previousStatus = null,
        CancellationToken cancellationToken = default)
    {
        var permission = OnboardingStatusPermissions.GetRequiredPermission(status, previousStatus);
        if (permission is null)
        {
            return Task.FromResult(false);
        }

        return HasPermissionAsync(user, permission.Value, cancellationToken);
    }

    private async Task<bool> EvaluateAsync(
        User user,
        Func<AuthorizationContext, bool> evaluator,
        CancellationToken cancellationToken)
    {
        if (!user.IsActive)
        {
            return false;
        }

        var context = await departmentResolver.ResolveAsync(user, cancellationToken);
        return evaluator(context);
    }
}
