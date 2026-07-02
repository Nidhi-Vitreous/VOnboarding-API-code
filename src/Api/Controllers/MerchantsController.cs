using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Api.Authorization;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Merchants;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("merchants")]
[Authorize]
public sealed class MerchantsController(IMerchantService merchantService) : ControllerBase
{
    [HttpGet]
    [RequireSystemPermission(PermissionSystemNames.MerchantRead)]
    [ProducesResponseType(typeof(MerchantListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery(Name = "page")] int page = ListPaging.DefaultPage,
        [FromQuery(Name = "page_size")] int pageSize = ListPaging.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        Ok(await merchantService.GetAllAsync(
            new MerchantListQuery
            {
                Role = role,
                Status = status,
                Search = search,
                Page = page,
                PageSize = pageSize,
            },
            cancellationToken));

    [HttpPost]
    [RequireSystemPermission(PermissionSystemNames.MerchantCreate)]
    [ProducesResponseType(typeof(MerchantDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] MerchantCreateRequest request,
        CancellationToken cancellationToken)
    {
        var merchant = await merchantService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = merchant.Id }, merchant);
    }

    [HttpGet("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.MerchantRead)]
    [ProducesResponseType(typeof(MerchantDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var merchant = await merchantService.GetByIdAsync(id, cancellationToken);
        return merchant is null
            ? NotFound(new ErrorResponse { Message = MerchantMessages.NotFound })
            : Ok(merchant);
    }

    [HttpPatch("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.MerchantUpdate)]
    [ProducesResponseType(typeof(MerchantDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] MerchantUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse { Message = "Unauthorized" });
        }

        var merchant = await merchantService.UpdateAsync(id, request, userId, cancellationToken);
        return merchant is null
            ? NotFound(new ErrorResponse { Message = MerchantMessages.NotFound })
            : Ok(merchant);
    }

    [HttpPatch("{id:guid}/status")]
    [RequireSystemPermission(PermissionSystemNames.MerchantUpdate)]
    [ProducesResponseType(typeof(MerchantStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid id,
        [FromBody] MerchantStatusTransitionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse { Message = "Unauthorized" });
        }

        var result = await merchantService.TransitionStatusAsync(id, request, userId, cancellationToken);
        return result is null
            ? NotFound(new ErrorResponse { Message = MerchantMessages.NotFound })
            : Ok(result);
    }

    [HttpGet("{id:guid}/history")]
    [RequireSystemPermission(PermissionSystemNames.MerchantRead)]
    [ProducesResponseType(typeof(StatusHistoryListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await merchantService.GetStatusHistoryAsync(id, cancellationToken);
        return history is null
            ? NotFound(new ErrorResponse { Message = MerchantMessages.NotFound })
            : Ok(history);
    }

    [HttpGet("{id:guid}/audit")]
    [RequireSystemPermission(PermissionSystemNames.MerchantRead)]
    [ProducesResponseType(typeof(AuditLogListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLog(Guid id, CancellationToken cancellationToken)
    {
        var auditLog = await merchantService.GetAuditLogAsync(id, cancellationToken);
        return auditLog is null
            ? NotFound(new ErrorResponse { Message = MerchantMessages.NotFound })
            : Ok(auditLog);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var rawUserId = User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out userId);
    }
}
