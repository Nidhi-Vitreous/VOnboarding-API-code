using Microsoft.AspNetCore.Authorization;

namespace Vitreous.Onboarding.Api.Authorization;

public sealed class SystemPermissionRequirement : IAuthorizationRequirement
{
    public SystemPermissionRequirement(string systemPermission) => SystemPermission = systemPermission;

    public string SystemPermission { get; }
}
