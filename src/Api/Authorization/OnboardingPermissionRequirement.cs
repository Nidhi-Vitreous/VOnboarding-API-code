using Microsoft.AspNetCore.Authorization;
using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Api.Authorization;

public sealed class OnboardingPermissionRequirement : IAuthorizationRequirement
{
    public OnboardingPermissionRequirement(OnboardingPermission permission) => Permission = permission;

    public OnboardingPermission Permission { get; }
}
