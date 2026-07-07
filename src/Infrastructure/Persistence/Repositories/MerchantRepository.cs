using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Merchants;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class MerchantRepository(ApplicationDbContext dbContext) : IMerchantRepository
{
    public async Task<(IReadOnlyList<Merchant> Items, int TotalCount)> GetListAsync(
        MerchantListQuery query,
        CancellationToken cancellationToken = default)
    {
        var merchants = dbContext.Merchants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim().ToLowerInvariant();
            merchants = merchants.Where(m => m.Role.ToLower() == role);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            merchants = merchants.Where(m => m.Status.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            merchants = merchants.Where(m =>
                m.MerchantName.ToLower().Contains(term)
                || m.Id.ToString().ToLower().Contains(term));
        }

        var totalCount = await merchants.CountAsync(cancellationToken);

        merchants = merchants
            .OrderByDescending(m => m.CreatedAt)
            .ThenBy(m => m.MerchantName);

        merchants = merchants
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var items = await merchants.ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<Merchant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Merchants.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task AddAsync(Merchant merchant, CancellationToken cancellationToken = default)
    {
        dbContext.Merchants.Add(merchant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Merchant merchant, CancellationToken cancellationToken = default)
    {
        dbContext.Merchants.Update(merchant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MerchantStatusHistory>> GetStatusHistoryAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default) =>
        await dbContext.MerchantStatusHistory
            .AsNoTracking()
            .Where(h => h.MerchantId == merchantId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);

    public async Task AddStatusHistoryAsync(
        MerchantStatusHistory history,
        CancellationToken cancellationToken = default)
    {
        dbContext.MerchantStatusHistory.Add(history);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetAuditLogAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default) =>
        await dbContext.AuditLog
            .AsNoTracking()
            .Where(a => a.EntityType == "Merchant" && a.EntityId == merchantId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAuditLogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLog.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var merchant = await dbContext.Merchants
            .Include(m => m.StatusHistory)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (merchant is null)
        {
            return false;
        }

        var auditEntries = await dbContext.AuditLog
            .Where(entry => entry.EntityType == "Merchant" && entry.EntityId == id)
            .ToListAsync(cancellationToken);

        dbContext.AuditLog.RemoveRange(auditEntries);
        dbContext.Merchants.Remove(merchant);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
