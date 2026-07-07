using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.ExistingMerchantOrders;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.ExistingMerchantOrders;

public sealed class ExistingMerchantOrderService(
    IExistingMerchantOrderRepository orderRepository,
    IUserRepository userRepository) : IExistingMerchantOrderService
{
    public async Task<ExistingMerchantOrderListResponse> GetAllAsync(
        ExistingMerchantOrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = ListPaging.NormalizePage(query.Page);
        var pageSize = ListPaging.NormalizePageSize(query.PageSize);
        var normalizedQuery = new ExistingMerchantOrderListQuery
        {
            Search = query.Search,
            Page = page,
            PageSize = pageSize,
        };

        var (items, totalCount) = await orderRepository.GetListAsync(normalizedQuery, cancellationToken);

        return new ExistingMerchantOrderListResponse
        {
            Data = items.Select(MapToSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = ListPaging.ComputeTotalPages(totalCount, pageSize),
        };
    }

    public async Task<ExistingMerchantOrderDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : MapToDetail(order);
    }

    public async Task<ExistingMerchantOrderDetailDto> CreateAsync(
        ExistingMerchantOrderCreateRequest request,
        Guid actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var actingUser = await userRepository.GetByIdAsync(actingUserId, cancellationToken);
        if (actingUser is null)
        {
            throw new BusinessRuleException(ExistingMerchantOrderMessages.ActingUserNotFound);
        }

        var now = DateTime.UtcNow;
        var order = new ExistingMerchantOrder
        {
            Id = Guid.NewGuid(),
            MerchantName = request.MerchantName.Trim(),
            Product = request.Product?.Trim(),
            Notes = request.Notes?.Trim(),
            Status = ExistingMerchantOrderStatusNames.Draft,
            CreatedBy = actingUser.Id,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await orderRepository.AddAsync(order, cancellationToken);
        return MapToDetail(order);
    }

    public async Task<ExistingMerchantOrderDetailDto?> UpdateAsync(
        Guid id,
        ExistingMerchantOrderUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.MerchantName))
        {
            order.MerchantName = request.MerchantName.Trim();
        }

        order.Product = request.Product?.Trim();
        order.Notes = request.Notes?.Trim();
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.UpdateAsync(order, cancellationToken);
        return MapToDetail(order);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        orderRepository.DeleteAsync(id, cancellationToken);

    private static void ValidateCreateRequest(ExistingMerchantOrderCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MerchantName))
        {
            throw new BusinessRuleException(ExistingMerchantOrderMessages.MerchantNameRequired);
        }
    }

    private static ExistingMerchantOrderSummaryDto MapToSummary(ExistingMerchantOrder order) => new()
    {
        Id = order.Id,
        MerchantName = order.MerchantName,
        Status = order.Status,
        Product = order.Product,
        CreatedAt = order.CreatedAt,
    };

    private static ExistingMerchantOrderDetailDto MapToDetail(ExistingMerchantOrder order) => new()
    {
        Id = order.Id,
        MerchantName = order.MerchantName,
        Status = order.Status,
        Product = order.Product,
        Notes = order.Notes,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
    };
}
