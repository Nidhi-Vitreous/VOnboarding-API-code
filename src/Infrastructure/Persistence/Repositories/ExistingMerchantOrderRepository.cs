using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.ExistingMerchantOrders;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class ExistingMerchantOrderRepository(ApplicationDbContext dbContext) : IExistingMerchantOrderRepository
{
    public async Task<(IReadOnlyList<ExistingMerchantOrder> Items, int TotalCount)> GetListAsync(
        ExistingMerchantOrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = dbContext.ExistingMerchantOrders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            orders = orders.Where(order =>
                order.MerchantName.ToLower().Contains(term)
                || order.Id.ToString().ToLower().Contains(term));
        }

        var totalCount = await orders.CountAsync(cancellationToken);

        var items = await orders
            .OrderByDescending(order => order.CreatedAt)
            .ThenBy(order => order.MerchantName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<ExistingMerchantOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ExistingMerchantOrders.FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task AddAsync(ExistingMerchantOrder order, CancellationToken cancellationToken = default)
    {
        dbContext.ExistingMerchantOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ExistingMerchantOrder order, CancellationToken cancellationToken = default)
    {
        dbContext.ExistingMerchantOrders.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.ExistingMerchantOrders
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        if (order is null)
        {
            return false;
        }

        dbContext.ExistingMerchantOrders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
