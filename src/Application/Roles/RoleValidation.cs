using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;

namespace Vitreous.Onboarding.Application.Roles;

internal static class RoleValidation
{
    internal static void ValidateRequest(string roleName, IReadOnlyList<Guid> permissionIds)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            errors.Add("Role name is required.");
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

    internal static async Task ValidatePermissionsExistAsync(
        IRoleRepository roleRepository,
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = permissionIds.Distinct().ToList();
        var permissions = await roleRepository.GetPermissionsByIdsAsync(distinctIds, cancellationToken);

        if (permissions.Count != distinctIds.Count)
        {
            var foundIds = permissions.Select(p => p.Id).ToHashSet();
            var missingIds = distinctIds.Where(id => !foundIds.Contains(id)).Select(id => id.ToString());
            throw new BusinessRuleException(
                "One or more permissions are invalid.",
                missingIds.Select(id => $"Permission '{id}' was not found.").ToArray());
        }
    }
}
