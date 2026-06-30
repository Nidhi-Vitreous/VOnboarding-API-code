using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(ApplicationDbContext dbContext) : IRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public async Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<Role> CreateRoleAsync(
        Role role,
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.Roles.Add(role);

        foreach (var permissionId in permissionIds.Distinct())
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (await GetRoleByIdAsync(role.Id, cancellationToken))!;
    }

    public async Task<Role?> UpdateRoleAsync(
        Guid id,
        string name,
        string roleType,
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role is null)
        {
            return null;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        role.Name = name;
        role.RoleType = roleType;
        role.UpdatedAt = DateTime.UtcNow;

        dbContext.RolePermissions.RemoveRange(role.RolePermissions);

        foreach (var permissionId in permissionIds.Distinct())
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetRoleByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role is null)
        {
            return false;
        }

        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> RoleNameExistsAsync(
        string name,
        Guid? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        var query = dbContext.Roles.AsNoTracking()
            .Where(r => r.Name.ToLower() == normalizedName);

        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRoleId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsByIdsAsync(
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        if (permissionIds.Count == 0)
        {
            return [];
        }

        var distinctIds = permissionIds.Distinct().ToList();
        return await dbContext.Permissions
            .AsNoTracking()
            .Where(p => distinctIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default)
    {
        var maxSortOrder = await dbContext.Roles.AsNoTracking()
            .Select(r => (int?)r.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSortOrder ?? 0) + 1;
    }
}
