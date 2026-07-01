using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Domain.Entities;
using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Application.Interfaces;

public interface IDepartmentResolver
{
    Task<AuthorizationContext> ResolveAsync(User user, CancellationToken cancellationToken = default);
}

public interface IAuthorizationService
{
    Task<bool> CanCreateOnboardingAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> CanApproveAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> CanRejectAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> CanHoldAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> CanBlockAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> CanRefileAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> IsAdminAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(User user, OnboardingPermission permission, CancellationToken cancellationToken = default);
    Task<bool> CanSetStatusAsync(
        User user,
        OnboardingRequestStatus status,
        OnboardingRequestStatus? previousStatus = null,
        CancellationToken cancellationToken = default);
}

public interface IPermissionAuthorizationService
{
    Task<bool> HasSystemPermissionAsync(User user, string systemPermission, CancellationToken cancellationToken = default);
}
