using Microsoft.AspNetCore.Authorization;
using Vitreous.Onboarding.Application.Authorization;

namespace Vitreous.Onboarding.Api.Authorization;

public static class SystemPermissionPolicies
{
    public const string Prefix = "Permission.";

    public static string For(string systemPermission) => $"{Prefix}{systemPermission}";

    public static void Register(AuthorizationOptions options)
    {
        foreach (var systemPermission in PermissionSystemNames.All)
        {
            options.AddPolicy(
                For(systemPermission),
                policy => policy.Requirements.Add(new SystemPermissionRequirement(systemPermission)));
        }
    }
}
