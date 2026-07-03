using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetTokenRepository(ApplicationDbContext dbContext) : IPasswordResetTokenRepository
{
    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        dbContext.PasswordResetTokens.Add(token);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateActiveForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken);

        if (activeTokens.Count == 0)
        {
            return;
        }

        foreach (var token in activeTokens)
        {
            token.UsedAt = utcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        dbContext.PasswordResetTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task MarkAsUsedAsync(
        PasswordResetToken token,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        token.UsedAt = utcNow;
        dbContext.PasswordResetTokens.Update(token);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
