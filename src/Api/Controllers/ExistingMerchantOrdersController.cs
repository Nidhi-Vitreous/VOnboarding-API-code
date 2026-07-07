using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitreous.Onboarding.Api.Authorization;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.ExistingMerchantOrders;
using Vitreous.Onboarding.Application.Interfaces;

namespace Vitreous.Onboarding.Api.Controllers;

[ApiController]
[Route("existing-merchant-orders")]
[Authorize]
public sealed class ExistingMerchantOrdersController(IExistingMerchantOrderService orderService) : ControllerBase
{
    [HttpGet]
    [RequireSystemPermission(PermissionSystemNames.ExistingMerchantOrderRead)]
    [ProducesResponseType(typeof(ExistingMerchantOrderListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery(Name = "page")] int page = ListPaging.DefaultPage,
        [FromQuery(Name = "page_size")] int pageSize = ListPaging.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        Ok(await orderService.GetAllAsync(
            new ExistingMerchantOrderListQuery
            {
                Search = search,
                Page = page,
                PageSize = pageSize,
            },
            cancellationToken));

    [HttpGet("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.ExistingMerchantOrderRead)]
    [ProducesResponseType(typeof(ExistingMerchantOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return order is null
            ? NotFound(new ErrorResponse { Message = ExistingMerchantOrderMessages.NotFound })
            : Ok(order);
    }

    [HttpPost]
    [RequireSystemPermission(PermissionSystemNames.ExistingMerchantOrderCreate)]
    [ProducesResponseType(typeof(ExistingMerchantOrderDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] ExistingMerchantOrderCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse { Message = "Unauthorized." });
        }

        var order = await orderService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.ExistingMerchantOrderUpdate)]
    [ProducesResponseType(typeof(ExistingMerchantOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] ExistingMerchantOrderUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var order = await orderService.UpdateAsync(id, request, cancellationToken);
        return order is null
            ? NotFound(new ErrorResponse { Message = ExistingMerchantOrderMessages.NotFound })
            : Ok(order);
    }

    [HttpDelete("{id:guid}")]
    [RequireSystemPermission(PermissionSystemNames.ExistingMerchantOrderDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await orderService.DeleteAsync(id, cancellationToken);
        return deleted
            ? NoContent()
            : NotFound(new ErrorResponse { Message = ExistingMerchantOrderMessages.NotFound });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var rawUserId = User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out userId);
    }
}
