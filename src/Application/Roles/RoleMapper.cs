using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Roles;

internal static class RoleMapper
{
    internal static RoleDto ToDto(Role role, int activeUserCount = 0) => new()
    {
        Id = role.Id,
        RoleName = role.Name,
        DepartmentId = role.DepartmentId,
        DepartmentName = role.Department?.Name ?? string.Empty,
        SortOrder = role.SortOrder,
        IsSystemRole = role.IsSystemRole,
        IsActive = role.IsActive,
        ActiveUserCount = activeUserCount,
        CreatedAt = role.CreatedAt,
        UpdatedAt = role.UpdatedAt,
        Permissions = role.RolePermissions
            .Select(rp => rp.Permission)
            .OrderBy(p => p.Name)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                SystemName = p.SystemName,
                Name = p.Name,
                Description = p.Description,
            })
            .ToList(),
    };
}
