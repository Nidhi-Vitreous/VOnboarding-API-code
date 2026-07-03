using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class PermissionAuthorizationServiceTests
{
    [Fact]
    public async Task HasSystemPermissionAsync_grants_permission_when_second_role_has_it()
    {
        var userId = Guid.NewGuid();
        var roleRepository = new FakeRoleRepository
        {
            RolesByUserId =
            {
                [userId] =
                [
                    CreateRole("Role One", PermissionSystemNames.UsersRead),
                    CreateRole("Role Two", PermissionSystemNames.UsersUpdate),
                ],
            },
        };
        var sut = CreateSut(roleRepository);

        var result = await sut.HasSystemPermissionAsync(
            CreateUser(userId, roleName: "Role One"),
            PermissionSystemNames.UsersUpdate);

        Assert.True(result);
    }

    [Fact]
    public async Task HasSystemPermissionAsync_denies_when_no_role_grants_permission_and_does_not_use_legacy_fallback()
    {
        var userId = Guid.NewGuid();
        var roleRepository = new FakeRoleRepository
        {
            RolesByUserId =
            {
                [userId] =
                [
                    CreateRole("Assigned Role One", PermissionSystemNames.UsersRead),
                    CreateRole("Assigned Role Two", PermissionSystemNames.DashboardView),
                ],
            },
            RolesByName =
            {
                ["Legacy Role"] = CreateRole("Legacy Role", PermissionSystemNames.UsersUpdate),
            },
        };
        var sut = CreateSut(roleRepository);

        var result = await sut.HasSystemPermissionAsync(
            CreateUser(userId, roleName: "Legacy Role"),
            PermissionSystemNames.UsersUpdate);

        Assert.False(result);
        Assert.False(roleRepository.GetRoleByNameCalled);
    }

    [Fact]
    public async Task HasSystemPermissionAsync_returns_false_for_inactive_user()
    {
        var sut = CreateSut(new FakeRoleRepository());

        var result = await sut.HasSystemPermissionAsync(
            CreateUser(Guid.NewGuid(), isActive: false),
            PermissionSystemNames.UsersRead);

        Assert.False(result);
    }

    [Fact]
    public async Task HasSystemPermissionAsync_returns_false_for_empty_permission()
    {
        var sut = CreateSut(new FakeRoleRepository());

        var result = await sut.HasSystemPermissionAsync(
            CreateUser(Guid.NewGuid()),
            "   ");

        Assert.False(result);
    }

    [Fact]
    public async Task HasSystemPermissionAsync_returns_true_for_admin_bypass()
    {
        var sut = CreateSut(
            new FakeRoleRepository(),
            new FakeDepartmentResolver { Context = new AuthorizationContext { Department = Domain.Enums.Department.Admin, IsAdmin = true, HasAdminOverrideFlag = false } });

        var result = await sut.HasSystemPermissionAsync(
            CreateUser(Guid.NewGuid()),
            PermissionSystemNames.UsersDelete);

        Assert.True(result);
    }

    [Fact]
    public async Task HasSystemPermissionAsync_uses_legacy_role_when_user_has_zero_assigned_roles_and_role_grants_permission()
    {
        var userId = Guid.NewGuid();
        var roleRepository = new FakeRoleRepository
        {
            RolesByUserId = { [userId] = [] },
            RolesByName =
            {
                ["Legacy Role"] = CreateRole("Legacy Role", PermissionSystemNames.UsersRead),
            },
        };
        var sut = CreateSut(roleRepository);

        var result = await sut.HasSystemPermissionAsync(
            CreateUser(userId, roleName: "Legacy Role"),
            PermissionSystemNames.UsersRead);

        Assert.True(result);
        Assert.True(roleRepository.GetRoleByNameCalled);
    }

    [Fact]
    public async Task HasSystemPermissionAsync_returns_false_for_legacy_role_without_permission_when_user_has_zero_assigned_roles()
    {
        var userId = Guid.NewGuid();
        var roleRepository = new FakeRoleRepository
        {
            RolesByUserId = { [userId] = [] },
            RolesByName =
            {
                ["Legacy Role"] = CreateRole("Legacy Role", PermissionSystemNames.UsersRead),
            },
        };
        var sut = CreateSut(roleRepository);

        var result = await sut.HasSystemPermissionAsync(
            CreateUser(userId, roleName: "Legacy Role"),
            PermissionSystemNames.UsersUpdate);

        Assert.False(result);
    }

    private static PermissionAuthorizationService CreateSut(
        FakeRoleRepository roleRepository,
        FakeDepartmentResolver? departmentResolver = null) =>
        new(roleRepository, departmentResolver ?? new FakeDepartmentResolver());

    private static User CreateUser(
        Guid id,
        string roleName = "Support",
        bool isActive = true) =>
        new()
        {
            Id = id,
            Username = "test-user",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = roleName,
            FullName = "Test User",
            IsActive = isActive,
        };

    private static Role CreateRole(string name, params string[] permissionSystemNames)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            RoleType = "System",
            IsActive = true,
        };

        role.RolePermissions = permissionSystemNames
            .Select(systemName => new RolePermission
            {
                Role = role,
                Permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    SystemName = systemName,
                    Name = systemName,
                },
            })
            .ToList();

        return role;
    }

    private sealed class FakeRoleRepository : IRoleRepository
    {
        public Dictionary<Guid, IReadOnlyList<Role>> RolesByUserId { get; init; } = [];
        public Dictionary<string, Role> RolesByName { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public bool GetRoleByNameCalled { get; private set; }

        public Task<IReadOnlyList<Role>> GetRolesWithPermissionsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(RolesByUserId.TryGetValue(userId, out var roles) ? roles : []);

        public Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            GetRoleByNameCalled = true;
            return Task.FromResult(RolesByName.TryGetValue(name, out var role) ? role : null);
        }

        public Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<(IReadOnlyList<Role> Items, int TotalCount)> GetPageAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role> CreateRoleAsync(
            Role role,
            IReadOnlyList<Guid> permissionIds,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role?> UpdateRoleAsync(
            Guid id,
            string name,
            string roleType,
            Guid departmentId,
            IReadOnlyList<Guid> permissionIds,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> RoleNameExistsAsync(
            string name,
            Guid? excludeRoleId = null,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<Permission>> GetPermissionsByIdsAsync(
            IReadOnlyList<Guid> permissionIds,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<Permission>> GetPermissionsBySystemNamesAsync(
            IReadOnlyList<string> systemNames,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeDepartmentResolver : IDepartmentResolver
    {
        public AuthorizationContext Context { get; init; } = new()
        {
            Department = Domain.Enums.Department.Support,
            IsAdmin = false,
            HasAdminOverrideFlag = false,
        };

        public Task<AuthorizationContext> ResolveAsync(User user, CancellationToken cancellationToken = default) =>
            Task.FromResult(Context);
    }
}
