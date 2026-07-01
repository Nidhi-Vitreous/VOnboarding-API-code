using Microsoft.AspNetCore.Authorization;
using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireOnboardingPermissionAttribute : AuthorizeAttribute
{
    public RequireOnboardingPermissionAttribute(OnboardingPermission permission)
        : base(OnboardingAuthorizationPolicies.For(permission))
    {
    }
}
