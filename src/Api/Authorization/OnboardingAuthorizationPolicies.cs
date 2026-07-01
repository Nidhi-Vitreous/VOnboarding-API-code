using Microsoft.AspNetCore.Authorization;
using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Api.Authorization;

public static class OnboardingAuthorizationPolicies
{
    public const string Prefix = "OnboardingPermission.";

    public static string For(OnboardingPermission permission) => $"{Prefix}{permission}";

    public static void Register(AuthorizationOptions options)
    {
        foreach (var permission in Enum.GetValues<OnboardingPermission>())
        {
            options.AddPolicy(
                For(permission),
                policy => policy.Requirements.Add(new OnboardingPermissionRequirement(permission)));
        }
    }
}
