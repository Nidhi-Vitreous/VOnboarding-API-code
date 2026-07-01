using Vitreous.Onboarding.Application.Interfaces;

namespace Vitreous.Onboarding.Application.Roles;

public sealed class DepartmentService(IDepartmentRepository departmentRepository, IRoleRepository roleRepository)
    : IDepartmentService
{
    public async Task<DepartmentListResponse> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await departmentRepository.GetAllAsync(cancellationToken);
        return new DepartmentListResponse
        {
            Data = departments
                .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name })
                .ToList(),
        };
    }

    public async Task<DepartmentPermissionsResponse?> GetPermissionsAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        if (department is null)
        {
            return null;
        }

        var groupDefinitions = DepartmentRolePermissionCatalog.GetPermissionGroups(department.Name);
        var allowedSystemNames = DepartmentRolePermissionCatalog.GetAllowedSystemNames(department.Name);
        var permissions = await roleRepository.GetPermissionsBySystemNamesAsync(
            allowedSystemNames.ToList(),
            cancellationToken);
        var permissionsBySystemName = permissions.ToDictionary(p => p.SystemName, StringComparer.OrdinalIgnoreCase);

        return new DepartmentPermissionsResponse
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            Groups = groupDefinitions
                .Select(group => new DepartmentPermissionGroupDto
                {
                    Key = group.Key,
                    Title = group.Title,
                    Permissions = group.Permissions
                        .Select(definition =>
                        {
                            if (!permissionsBySystemName.TryGetValue(definition.SystemName, out var permission))
                            {
                                return null;
                            }

                            return new DepartmentPermissionItemDto
                            {
                                Id = permission.Id,
                                SystemName = permission.SystemName,
                                Name = permission.Name,
                                Description = permission.Description,
                            };
                        })
                        .Where(item => item is not null)
                        .Cast<DepartmentPermissionItemDto>()
                        .ToList(),
                })
                .Where(group => group.Permissions.Count > 0)
                .ToList(),
        };
    }
}
