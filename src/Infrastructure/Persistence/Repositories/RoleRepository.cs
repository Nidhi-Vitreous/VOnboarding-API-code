using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(ApplicationDbContext dbContext) : IRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Roles
            .AsNoTracking()
            .Include(r => r.Department)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Roles
            .AsNoTracking()
            .Include(r => r.Department)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return dbContext.Roles
            .AsNoTracking()
            .Include(r => r.Department)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<Role> CreateRoleAsync(
        Role role,
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.Roles.Add(role);

        var permissions = await GetPermissionsByIdsAsync(permissionIds, cancellationToken);
        foreach (var permission in permissions)
        {
            dbContext.RolePermissions.Add(CreateRolePermission(role, permission));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (await GetRoleByIdAsync(role.Id, cancellationToken))!;
    }

    public async Task<Role?> UpdateRoleAsync(
        Guid id,
        string name,
        string roleType,
        Guid departmentId,
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
        role.DepartmentId = departmentId;
        role.UpdatedAt = DateTime.UtcNow;

        dbContext.RolePermissions.RemoveRange(role.RolePermissions);

        var permissions = await GetPermissionsByIdsAsync(permissionIds, cancellationToken);
        foreach (var permission in permissions)
        {
            dbContext.RolePermissions.Add(CreateRolePermission(role, permission));
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

    public async Task<IReadOnlyList<Permission>> GetPermissionsBySystemNamesAsync(
        IReadOnlyList<string> systemNames,
        CancellationToken cancellationToken = default)
    {
        if (systemNames.Count == 0)
        {
            return [];
        }

        var normalizedNames = systemNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        return await dbContext.Permissions
            .AsNoTracking()
            .Where(p => normalizedNames.Contains(p.SystemName.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default)
    {
        var maxSortOrder = await dbContext.Roles.AsNoTracking()
            .Select(r => (int?)r.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSortOrder ?? 0) + 1;
    }

    private static RolePermission CreateRolePermission(Role role, Permission permission) => new()
    {
        RoleId = role.Id,
        PermissionId = permission.Id,
        RoleName = role.Name,
        PermissionName = permission.Name,
    };
}
