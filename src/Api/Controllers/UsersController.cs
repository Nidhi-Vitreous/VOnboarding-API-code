using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Api.Authorization;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [RequireSystemPermission(PermissionSystemNames.UsersRead)]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery(Name = "page")] int page = UserPaging.DefaultPage,
        [FromQuery(Name = "page_size")] int pageSize = UserPaging.DefaultPageSize,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var users = await userService.GetAllAsync(page, pageSize, search, cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.UsersRead)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(new ErrorResponse { Message = "User not found." });
        }

        return Ok(user);
    }

    [HttpPost]
    [RequireSystemPermission(PermissionSystemNames.UsersCreate)]
    [ProducesResponseType(typeof(UserCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.User.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.UsersUpdate)]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UserUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userService.UpdateAsync(id, request, cancellationToken);
        return user is null
            ? NotFound(new ErrorResponse { Message = "User not found." })
            : Ok(user);
    }
}
