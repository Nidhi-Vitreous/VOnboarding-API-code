using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Authorization;

public sealed class PermissionAuthorizationService(
    IRoleRepository roleRepository,
    IDepartmentResolver departmentResolver) : IPermissionAuthorizationService
{
    public async Task<bool> HasSystemPermissionAsync(
        User user,
        string systemPermission,
        CancellationToken cancellationToken = default)
    {
        if (!user.IsActive || string.IsNullOrWhiteSpace(systemPermission))
        {
            return false;
        }

        var context = await departmentResolver.ResolveAsync(user, cancellationToken);
        if (context.IsSuperAdmin)
        {
            return true;
        }

        var grantedPermissions = await GetGrantedSystemPermissionNamesAsync(user, cancellationToken);
        return grantedPermissions.Contains(systemPermission.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetGrantedSystemPermissionNamesAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        if (!user.IsActive)
        {
            return [];
        }

        var context = await departmentResolver.ResolveAsync(user, cancellationToken);
        if (context.IsSuperAdmin)
        {
            return PermissionSystemNames.All;
        }

        var roles = await roleRepository.GetRolesWithPermissionsByUserIdAsync(user.Id, cancellationToken);
        if (roles.Count > 0)
        {
            return roles
                .SelectMany(role => role.RolePermissions)
                .Select(assignment => assignment.Permission.SystemName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (string.IsNullOrWhiteSpace(user.Role))
        {
            return [];
        }

        var legacyRole = await roleRepository.GetRoleByNameAsync(user.Role, cancellationToken);
        if (legacyRole is null)
        {
            return [];
        }

        return legacyRole.RolePermissions
            .Select(assignment => assignment.Permission.SystemName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
