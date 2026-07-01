using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Vitreous.Onboarding.Application.Interfaces;

namespace Vitreous.Onboarding.Api.Authorization;

public sealed class SystemPermissionAuthorizationHandler(
    IUserRepository userRepository,
    IPermissionAuthorizationService permissionAuthorizationService)
    : AuthorizationHandler<SystemPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SystemPermissionRequirement requirement)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            return;
        }

        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        if (await permissionAuthorizationService.HasSystemPermissionAsync(user, requirement.SystemPermission))
        {
            context.Succeed(requirement);
        }
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId)
    {
        var rawUserId = principal.FindFirstValue("userId")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out userId);
    }
}
