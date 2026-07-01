using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Api.Authorization;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Roles;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("departments")]
public sealed class DepartmentsController(IDepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    [RequireSystemPermission(PermissionSystemNames.RolesRead)]
    [ProducesResponseType(typeof(DepartmentListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await departmentService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}/permissions")]
    [RequireSystemPermission(PermissionSystemNames.RolesRead)]
    [ProducesResponseType(typeof(DepartmentPermissionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissions(Guid id, CancellationToken cancellationToken)
    {
        var response = await departmentService.GetPermissionsAsync(id, cancellationToken);
        return response is null
            ? NotFound(new ErrorResponse { Message = DepartmentMessages.NotFound })
            : Ok(response);
    }
}
