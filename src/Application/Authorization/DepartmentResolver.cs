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

        var hasAdminOverrideFlag = role?.IsSystemRole == true;
        var isAdmin = DepartmentPermissionRegistry.IsAdminDepartment(department)
            || hasAdminOverrideFlag
            || DepartmentPermissionRegistry.IsAdminRoleName(user.Role);

        if (isAdmin)
        {
            department = Domain.Enums.Department.Admin;
        }

        return new AuthorizationContext
        {
            Department = department,
            IsAdmin = isAdmin,
            HasAdminOverrideFlag = hasAdminOverrideFlag,
        };
    }
}
