using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IPasswordResetService passwordResetService) : ControllerBase
{
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await passwordResetService.RequestPasswordResetAsync(request.Email, cancellationToken);
        return Ok(response);
    }
}
