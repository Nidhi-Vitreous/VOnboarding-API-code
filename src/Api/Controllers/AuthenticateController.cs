using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("authenticate")]
public sealed class AuthenticateController(IAuthService authService) : ControllerBase
{
    [HttpGet("userName")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromQuery] string userName,
        [FromQuery] string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Validation failed",
                Details = ["userName and password are required."],
            });
        }

        var response = await authService.LoginAsync(userName, password, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new ErrorResponse { Message = "Unauthorized" });
        }

        return Ok(response);
    }

    [HttpGet("login")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("userId");

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            await authService.LogoutAsync(userId, cancellationToken);
        }

        return NoContent();
    }

    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Validation failed",
                Details = ["refresh_token is required."],
            });
        }

        var response = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new ErrorResponse { Message = "Unauthorized" });
        }

        return Ok(response);
    }

    [HttpPost("updateUserName")]
    [Authorize]
    [ProducesResponseType(typeof(UserNameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult UpdateUserName([FromBody] UpdateUsernameRequest request)
    {
        return BadRequest(new ErrorResponse
        {
            Message = "Username update is not implemented in this release.",
        });
    }

    [HttpGet("forgotUserName")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RecoveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult ForgotUserName([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Validation failed",
                Details = ["email is required."],
            });
        }

        return Ok(new RecoveryResponse
        {
            Message = "If an account exists for this email, username recovery instructions have been sent.",
        });
    }

    [HttpGet("forgotPassword")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RecoveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult ForgotPassword([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "Validation failed",
                Details = ["email is required."],
            });
        }

        return Ok(new RecoveryResponse
        {
            Message = "If an account exists for this email, password reset instructions have been sent.",
        });
    }
}
