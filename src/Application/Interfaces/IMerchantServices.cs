using System.Text.Json;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Merchants;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Interfaces;

public interface IMerchantRepository
{
    Task<(IReadOnlyList<Merchant> Items, int TotalCount)> GetListAsync(
        MerchantListQuery query,
        CancellationToken cancellationToken = default);

    Task<Merchant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Merchant merchant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Merchant merchant, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MerchantStatusHistory>> GetStatusHistoryAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);
    Task AddStatusHistoryAsync(MerchantStatusHistory history, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogEntry>> GetAuditLogAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}

public interface IMerchantService
{
    Task<MerchantListResponse> GetAllAsync(MerchantListQuery query, CancellationToken cancellationToken = default);
    Task<MerchantDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MerchantDetailDto> CreateAsync(MerchantCreateRequest request, CancellationToken cancellationToken = default);
    Task<MerchantDetailDto?> UpdateAsync(
        Guid id,
        MerchantUpdateRequest request,
        Guid actingUserId,
        CancellationToken cancellationToken = default);
    Task<MerchantStatusResponse?> TransitionStatusAsync(
        Guid id,
        MerchantStatusTransitionRequest request,
        Guid actingUserId,
        CancellationToken cancellationToken = default);
    Task<StatusHistoryListResponse?> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuditLogListResponse?> GetAuditLogAsync(Guid id, CancellationToken cancellationToken = default);
}
