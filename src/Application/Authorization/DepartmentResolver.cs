using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Authorization;

public sealed class DepartmentResolver(IRoleRepository roleRepository) : IDepartmentResolver
{
    public async Task<AuthorizationContext> ResolveAsync(User user, CancellationToken cancellationToken = default)
    {
        var role = string.IsNullOrWhiteSpace(user.Role)
            ? null
            : await roleRepository.GetRoleByNameAsync(user.Role, cancellationToken);

        var department = role?.Department?.Name is { Length: > 0 } departmentName
            ? DepartmentPermissionRegistry.ResolveDepartmentByName(departmentName)
            : DepartmentPermissionRegistry.ResolveDepartment(user.Role, role?.RoleType);

        var assignedRoles = await roleRepository.GetRolesWithPermissionsByUserIdAsync(user.Id, cancellationToken);

        var isSuperAdmin = DepartmentPermissionRegistry.IsSuperAdminRoleName(user.Role)
            || assignedRoles.Any(assignedRole =>
                DepartmentPermissionRegistry.IsSuperAdminRoleName(assignedRole.Name)
                || assignedRole.IsSystemRole);

        var isAdmin = DepartmentPermissionRegistry.IsAdminDepartment(department)
            || DepartmentPermissionRegistry.IsAdminRoleName(user.Role)
            || assignedRoles.Any(assignedRole => DepartmentPermissionRegistry.IsAdminRoleName(assignedRole.Name));

        if (isAdmin)
        {
            department = Domain.Enums.Department.Admin;
        }

        return new AuthorizationContext
        {
            Department = department,
            IsAdmin = isAdmin,
            IsSuperAdmin = isSuperAdmin,
        };
    }
}
