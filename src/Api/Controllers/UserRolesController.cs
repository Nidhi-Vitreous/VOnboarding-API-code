using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Api.Authorization;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Roles;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("userRoles")]
public sealed class UserRolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [RequireSystemPermission(PermissionSystemNames.RolesRead)]
    [ProducesResponseType(typeof(RoleListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await roleService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.RolesRead)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleService.GetByIdAsync(id, cancellationToken);
        return role is null
            ? NotFound(new ErrorResponse { Message = RoleMessages.NotFound })
            : Ok(role);
    }

    [HttpPost]
    [RequireSystemPermission(PermissionSystemNames.RolesCreate)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] RoleCreateDto request, CancellationToken cancellationToken)
    {
        var role = await roleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.RolesUpdate)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] RoleUpdateDto request,
        CancellationToken cancellationToken)
    {
        var role = await roleService.UpdateAsync(id, request, cancellationToken);
        return role is null
            ? NotFound(new ErrorResponse { Message = RoleMessages.NotFound })
            : Ok(role);
    }

    [HttpDelete("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.RolesDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await roleService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound(new ErrorResponse { Message = RoleMessages.NotFound });
}
