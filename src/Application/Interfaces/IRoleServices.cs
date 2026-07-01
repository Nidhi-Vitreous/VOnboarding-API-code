using Vitreous.Onboarding.Application.Roles;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Interfaces;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role> CreateRoleAsync(Role role, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);
    Task<Role?> UpdateRoleAsync(
        Guid id,
        string name,
        string roleType,
        Guid departmentId,
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RoleNameExistsAsync(string name, Guid? excludeRoleId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetPermissionsByIdsAsync(IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetPermissionsBySystemNamesAsync(IReadOnlyList<string> systemNames, CancellationToken cancellationToken = default);
    Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default);
}

public interface IRoleService
{
    Task<RoleListResponse> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateAsync(RoleCreateDto request, CancellationToken cancellationToken = default);
    Task<RoleDto?> UpdateAsync(Guid id, RoleUpdateDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
