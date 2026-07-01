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
        if (context.IsAdmin)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(user.Role))
        {
            return false;
        }

        var role = await roleRepository.GetRoleByNameAsync(user.Role, cancellationToken);
        if (role is null)
        {
            return false;
        }

        return role.RolePermissions.Any(assignment =>
            string.Equals(assignment.Permission.SystemName, systemPermission.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
