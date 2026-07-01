using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Roles;

public sealed class RoleService(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    IDepartmentRepository departmentRepository) : IRoleService
{
    public async Task<RoleListResponse> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.GetAllRolesWithPermissionsAsync(cancellationToken);
        var userCounts = await userRepository.GetActiveUserCountsByRoleNamesAsync(
            roles.Select(r => r.Name),
            cancellationToken);

        return new RoleListResponse
        {
            Data = roles
                .Select(r => RoleMapper.ToDto(r, ResolveActiveUserCount(userCounts, r.Name)))
                .ToList(),
        };
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetRoleByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return null;
        }

        var userCounts = await userRepository.GetActiveUserCountsByRoleNamesAsync([role.Name], cancellationToken);
        return RoleMapper.ToDto(role, ResolveActiveUserCount(userCounts, role.Name));
    }

    public async Task<RoleDto> CreateAsync(RoleCreateDto request, CancellationToken cancellationToken = default)
    {
        RoleValidation.ValidateRequest(request.RoleName, request.DepartmentId, request.PermissionIds);

        if (await roleRepository.RoleNameExistsAsync(request.RoleName, cancellationToken: cancellationToken))
        {
            throw new BusinessRuleException(RoleMessages.NameMustBeUnique, RoleMessages.DuplicateNameDetail);
        }

        var department = await RoleValidation.ValidateDepartmentExistsAsync(
            departmentRepository,
            request.DepartmentId,
            cancellationToken);

        await RoleValidation.ValidateRolePermissionsAsync(
            roleRepository,
            department.Name,
            request.PermissionIds,
            cancellationToken);

        var now = DateTime.UtcNow;
        var roleName = request.RoleName.Trim();
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            RoleType = department.Name,
            DepartmentId = department.Id,
            SortOrder = await roleRepository.GetNextSortOrderAsync(cancellationToken),
            IsSystemRole = false,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var created = await roleRepository.CreateRoleAsync(role, request.PermissionIds, cancellationToken);
        return RoleMapper.ToDto(created);
    }

    public async Task<RoleDto?> UpdateAsync(
        Guid id,
        RoleUpdateDto request,
        CancellationToken cancellationToken = default)
    {
        RoleValidation.ValidateRequest(request.RoleName, request.DepartmentId, request.PermissionIds);

        var existingRole = await roleRepository.GetRoleByIdAsync(id, cancellationToken);
        if (existingRole is null)
        {
            return null;
        }

        if (existingRole.IsSystemRole)
        {
            throw new BusinessRuleException(RoleMessages.CannotModify, RoleMessages.SystemRoleCannotBeModified);
        }

        if (await roleRepository.RoleNameExistsAsync(request.RoleName, id, cancellationToken))
        {
            throw new BusinessRuleException(RoleMessages.NameMustBeUnique, RoleMessages.DuplicateNameDetail);
        }

        var department = await RoleValidation.ValidateDepartmentExistsAsync(
            departmentRepository,
            request.DepartmentId,
            cancellationToken);

        await RoleValidation.ValidateRolePermissionsAsync(
            roleRepository,
            department.Name,
            request.PermissionIds,
            cancellationToken);

        var roleName = request.RoleName.Trim();
        var updated = await roleRepository.UpdateRoleAsync(
            id,
            roleName,
            department.Name,
            department.Id,
            request.PermissionIds,
            cancellationToken);

        return updated is null ? null : RoleMapper.ToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetRoleByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return false;
        }

        if (role.IsSystemRole)
        {
            throw new BusinessRuleException(RoleMessages.CannotDelete, RoleMessages.SystemRoleProtected);
        }

        if (await userRepository.RoleNameInUseAsync(role.Name, cancellationToken))
        {
            throw new BusinessRuleException(RoleMessages.CannotDelete, RoleMessages.RoleInUse);
        }

        return await roleRepository.DeleteRoleAsync(id, cancellationToken);
    }

    private static int ResolveActiveUserCount(IReadOnlyDictionary<string, int> counts, string roleName) =>
        counts.TryGetValue(roleName, out var count) ? count : 0;
}
