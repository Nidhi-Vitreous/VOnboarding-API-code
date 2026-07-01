using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Api.Authorization;
using Vitreous.Onboarding.Api.Contracts;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("onboarding")]
[Authorize]
public sealed class OnboardingWorkflowController(
    IUserRepository userRepository,
    Application.Interfaces.IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [RequireOnboardingPermission(OnboardingPermission.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetById(Guid id) =>
        Ok(new { message = "Onboarding request retrieved.", id });

    [HttpPost]
    [RequireOnboardingPermission(OnboardingPermission.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Create() =>
        StatusCode(StatusCodes.Status201Created, new { message = "Onboarding request created.", status = nameof(OnboardingRequestStatus.Draft) });

    [HttpPost("{id:guid}/refile")]
    [RequireOnboardingPermission(OnboardingPermission.Refile)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Refile(Guid id) =>
        Ok(new { message = "Onboarding request refiled.", id });

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateOnboardingStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (OnboardingStatusPermissions.GetRequiredPermission(request.Status, request.PreviousStatus) is null)
        {
            return BadRequest(new { message = "Status cannot be set directly.", status = request.Status.ToString() });
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        if (!await authorizationService.CanSetStatusAsync(user, request.Status, request.PreviousStatus, cancellationToken))
        {
            return Forbid();
        }

        return Ok(new
        {
            message = "Onboarding request status updated.",
            id,
            status = request.Status.ToString(),
        });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var rawUserId = User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out userId);
    }
}
