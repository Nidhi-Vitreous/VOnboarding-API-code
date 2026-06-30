using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Roles;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("userRoles")]
[Authorize]
public sealed class UserRolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(RoleListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await roleService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
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
