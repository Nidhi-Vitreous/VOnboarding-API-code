using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Api.Authorization;

public sealed class OnboardingPermissionAuthorizationHandler(
    IUserRepository userRepository,
    Application.Interfaces.IAuthorizationService authorizationService)
    : AuthorizationHandler<OnboardingPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OnboardingPermissionRequirement requirement)
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

        if (await authorizationService.HasPermissionAsync(user, requirement.Permission))
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
