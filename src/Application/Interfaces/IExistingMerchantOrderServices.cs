using Vitreous.Onboarding.Application.ExistingMerchantOrders;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Interfaces;

public interface IExistingMerchantOrderRepository
{
    Task<(IReadOnlyList<ExistingMerchantOrder> Items, int TotalCount)> GetListAsync(
        ExistingMerchantOrderListQuery query,
        CancellationToken cancellationToken = default);

    Task<ExistingMerchantOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ExistingMerchantOrder order, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExistingMerchantOrder order, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IExistingMerchantOrderService
{
    Task<ExistingMerchantOrderListResponse> GetAllAsync(
        ExistingMerchantOrderListQuery query,
        CancellationToken cancellationToken = default);

    Task<ExistingMerchantOrderDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ExistingMerchantOrderDetailDto> CreateAsync(
        ExistingMerchantOrderCreateRequest request,
        Guid actingUserId,
        CancellationToken cancellationToken = default);

    Task<ExistingMerchantOrderDetailDto?> UpdateAsync(
        Guid id,
        ExistingMerchantOrderUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
