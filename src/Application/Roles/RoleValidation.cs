using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Roles;

internal static class RoleValidation
{
    internal static void ValidateRequest(string roleName, Guid departmentId, IReadOnlyList<Guid> permissionIds)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            errors.Add("Role name is required.");
        }

        if (departmentId == Guid.Empty)
        {
            errors.Add("Department is required.");
        }

        if (permissionIds is null || permissionIds.Count == 0)
        {
            errors.Add("At least one permission must be selected.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessRuleException("Validation failed.", errors.ToArray());
        }
    }

    internal static async Task<Department> ValidateDepartmentExistsAsync(
        IDepartmentRepository departmentRepository,
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        if (department is null)
        {
            throw new BusinessRuleException(
                "Validation failed.",
                ["The selected department was not found."]);
        }

        return department;
    }

    internal static async Task ValidateRolePermissionsAsync(
        IRoleRepository roleRepository,
        string departmentName,
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = permissionIds.Distinct().ToList();
        var permissions = await roleRepository.GetPermissionsByIdsAsync(distinctIds, cancellationToken);

        if (permissions.Count != distinctIds.Count)
        {
            var foundIds = permissions.Select(permission => permission.Id).ToHashSet();
            var missingIds = distinctIds.Where(id => !foundIds.Contains(id)).Select(id => id.ToString());
            throw new BusinessRuleException(
                "One or more permissions are invalid.",
                missingIds.Select(id => $"Permission '{id}' was not found.").ToArray());
        }

        var allowedSystemNames = DepartmentRolePermissionCatalog
            .GetAllowedSystemNames(departmentName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allowedSystemNames.Count == 0)
        {
            throw new BusinessRuleException(
                "Validation failed.",
                ["No permissions are configured for the selected department."]);
        }

        var invalidPermissions = permissions
            .Where(permission => !allowedSystemNames.Contains(permission.SystemName))
            .Select(permission => permission.SystemName)
            .ToList();

        if (invalidPermissions.Count > 0)
        {
            throw new BusinessRuleException(
                "One or more permissions are not allowed for the selected department.",
                invalidPermissions.Select(name => $"Permission '{name}' is not allowed for this department.").ToArray());
        }
    }
}
