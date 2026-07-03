using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class UserServiceUpdateTests
{
    [Fact]
    public async Task UpdateAsync_persists_via_save_tracked_changes_not_repository_update()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository { User = CreateUser(userId) };
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Support", "Ops"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        await sut.UpdateAsync(userId, ValidUpdateRequest([roleId]));

        Assert.True(userRepository.SaveTrackedChangesCalled);
        Assert.False(userRepository.UpdateCalled);
    }

    [Fact]
    public async Task UpdateAsync_unknown_id_returns_null_without_persisting()
    {
        var userRepository = new FakeUserRepository();
        var sut = CreateSut(userRepository);

        var result = await sut.UpdateAsync(Guid.NewGuid(), ValidUpdateRequest([Guid.NewGuid()]));

        Assert.Null(result);
        Assert.False(userRepository.SaveTrackedChangesCalled);
    }

    [Fact]
    public async Task UpdateAsync_replaces_role_set_by_adding_and_removing_user_roles()
    {
        var userId = Guid.NewGuid();
        var keepRoleId = Guid.NewGuid();
        var removeRoleId = Guid.NewGuid();
        var addRoleId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.UserRoles =
        [
            CreateUserRole(userId, keepRoleId, "Support", "Ops"),
            CreateUserRole(userId, removeRoleId, "Legacy", "Ops"),
        ];
        var userRepository = new FakeUserRepository { User = user };
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [keepRoleId] = CreateRole(keepRoleId, "Support", "Ops"),
                [addRoleId] = CreateRole(addRoleId, "Terminal Operator", "Operations"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var result = await sut.UpdateAsync(userId, ValidUpdateRequest([keepRoleId, addRoleId]));

        Assert.NotNull(result);
        Assert.True(userRepository.SaveTrackedChangesCalled);
        Assert.Equal(2, userRepository.LastUpdatedUser!.UserRoles.Count);
        Assert.DoesNotContain(userRepository.LastUpdatedUser.UserRoles, userRole => userRole.RoleId == removeRoleId);
        Assert.Contains(userRepository.LastUpdatedUser.UserRoles, userRole => userRole.RoleId == keepRoleId);
        Assert.Contains(userRepository.LastUpdatedUser.UserRoles, userRole => userRole.RoleId == addRoleId);
        Assert.Equal(2, result.Roles.Count);
        Assert.Equal("Support", result.Role);
        Assert.Equal("Support", result.Roles[0].Name);
        Assert.Equal("Terminal Operator", result.Roles[1].Name);
    }

    [Fact]
    public async Task UpdateAsync_missing_role_throws_and_does_not_persist()
    {
        var userId = Guid.NewGuid();
        var userRepository = new FakeUserRepository { User = CreateUser(userId) };
        var roleRepository = new FakeRoleRepository();
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.UpdateAsync(userId, ValidUpdateRequest([Guid.NewGuid()])));

        Assert.Equal(UserMessages.InvalidRole, exception.Message);
        Assert.Contains(UserMessages.RoleNotFound, exception.Details ?? []);
        Assert.False(userRepository.SaveTrackedChangesCalled);
    }

    [Fact]
    public async Task UpdateAsync_inactive_role_throws_and_does_not_persist()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository { User = CreateUser(userId) };
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Retired Role", "Ops", isActive: false),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.UpdateAsync(userId, ValidUpdateRequest([roleId])));

        Assert.Equal(UserMessages.InvalidRole, exception.Message);
        Assert.Contains(UserMessages.RoleInactive, exception.Details ?? []);
        Assert.False(userRepository.SaveTrackedChangesCalled);
    }

    [Fact]
    public async Task UpdateAsync_empty_role_ids_throws_validation_error()
    {
        var userId = Guid.NewGuid();
        var userRepository = new FakeUserRepository { User = CreateUser(userId) };
        var sut = CreateSut(userRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.UpdateAsync(userId, ValidUpdateRequest([])));

        Assert.Equal("Validation failed.", exception.Message);
        Assert.Contains("At least one role is required.", exception.Details ?? []);
        Assert.False(userRepository.SaveTrackedChangesCalled);
    }

    [Fact]
    public async Task UpdateAsync_updates_scalar_fields_and_legacy_role_from_first_role()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateUser(userId);
        var userRepository = new FakeUserRepository { User = user };
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Terminal Operator", "Operations"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var result = await sut.UpdateAsync(userId, ValidUpdateRequest([roleId], request =>
        {
            request.FirstName = "  Yash  ";
            request.LastName = "  Patel  ";
            request.PhoneNumber = "+1 (555) 123-4567";
            request.OfficeNumber = "  101  ";
            request.Notes = "  Notes  ";
            request.TwoFactorEnabled = true;
        }));

        Assert.NotNull(result);
        Assert.Equal("Yash", result.FirstName);
        Assert.Equal("Patel", result.LastName);
        Assert.Equal("Yash Patel", result.FullName);
        Assert.Equal("+1 (555) 123-4567", result.PhoneNumber);
        Assert.Equal("Terminal Operator", result.Role);
        Assert.Equal("Operations", result.Roles[0].DepartmentName);
        Assert.True(userRepository.SaveTrackedChangesCalled);
        Assert.Equal("Yash", userRepository.LastUpdatedUser!.FirstName);
        Assert.Equal("Patel", userRepository.LastUpdatedUser.LastName);
        Assert.Equal("Yash Patel", userRepository.LastUpdatedUser.FullName);
        Assert.Equal("101", userRepository.LastUpdatedUser.OfficeNumber);
        Assert.Equal("Notes", userRepository.LastUpdatedUser.Notes);
        Assert.True(userRepository.LastUpdatedUser.TwoFactorEnabled);
    }

    [Fact]
    public async Task GetByIdAsync_returns_roles_with_names_and_department()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.UserRoles = [CreateUserRole(userId, roleId, "Support", "Customer Success")];
        var userRepository = new FakeUserRepository { UserWithRoles = user };
        var sut = CreateSut(userRepository);

        var result = await sut.GetByIdAsync(userId);

        Assert.NotNull(result);
        Assert.Single(result.Roles);
        Assert.Equal(roleId, result.Roles[0].Id);
        Assert.Equal("Support", result.Roles[0].Name);
        Assert.Equal("Customer Success", result.Roles[0].DepartmentName);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task GetAllAsync_returns_roles_with_names_and_department()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.UserRoles = [CreateUserRole(userId, roleId, "Support", "Customer Success")];
        var userRepository = new FakeUserRepository
        {
            PageItems = [user],
            TotalCount = 1,
        };
        var sut = CreateSut(userRepository);

        var result = await sut.GetAllAsync(1, 10);

        Assert.Single(result.Data);
        Assert.Single(result.Data[0].Roles);
        Assert.Equal("Support", result.Data[0].Roles[0].Name);
        Assert.Equal("Customer Success", result.Data[0].Roles[0].DepartmentName);
    }

    private static UserUpdateRequest ValidUpdateRequest(
        IReadOnlyList<Guid> roleIds,
        Action<UserUpdateRequest>? configure = null)
    {
        var request = new UserUpdateRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            RoleIds = roleIds.ToList(),
        };

        configure?.Invoke(request);
        return request;
    }

    private static UserService CreateSut(
        FakeUserRepository userRepository,
        FakeRoleRepository? roleRepository = null) =>
        new(userRepository, roleRepository ?? new FakeRoleRepository(), new FakePasswordHasher());

    private static User CreateUser(Guid id) => new()
    {
        Id = id,
        Username = "jane",
        Email = "jane@example.com",
        PasswordHash = "hash",
        Role = "Support",
        FullName = "Jane Doe",
        FirstName = "Jane",
        LastName = "Doe",
        Department = "Ops",
        PhoneNumber = "+15550001111",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UserRoles = [],
    };

    private static Role CreateRole(
        Guid id,
        string name,
        string departmentName,
        bool isActive = true) =>
        new()
        {
            Id = id,
            Name = name,
            IsActive = isActive,
            Department = new Department
            {
                Id = Guid.NewGuid(),
                Name = departmentName,
            },
        };

    private static UserRole CreateUserRole(
        Guid userId,
        Guid roleId,
        string roleName,
        string departmentName) =>
        new()
        {
            UserId = userId,
            RoleId = roleId,
            Role = CreateRole(roleId, roleName, departmentName),
        };

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? User { get; init; }
        public User? UserWithRoles { get; init; }
        public IReadOnlyList<User> PageItems { get; init; } = [];
        public int TotalCount { get; init; }
        public bool UpdateCalled { get; private set; }
        public bool SaveTrackedChangesCalled { get; private set; }
        public User? LastUpdatedUser { get; private set; }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(User is not null && User.Id == id ? User : null);

        public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(UserWithRoles is not null && UserWithRoles.Id == id ? UserWithRoles : null);

        public Task<User?> GetByIdTrackedWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(User is not null && User.Id == id ? User : null);

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            UpdateCalled = true;
            LastUpdatedUser = user;
            return Task.CompletedTask;
        }

        public Task SaveTrackedChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveTrackedChangesCalled = true;
            LastUpdatedUser = User;
            return Task.CompletedTask;
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<(IReadOnlyList<User> Items, int TotalCount)> GetPageAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<User> Items, int TotalCount)>((PageItems, TotalCount));

        public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> UsernameExistsAsync(
            string username,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> RoleNameInUseAsync(string roleName, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyDictionary<string, int>> GetActiveUserCountsByRoleNamesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeRoleRepository : IRoleRepository
    {
        public Dictionary<Guid, Role> Roles { get; init; } = [];

        public Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Roles.TryGetValue(id, out var role) ? role : null);

        public Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<(IReadOnlyList<Role> Items, int TotalCount)> GetPageAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default) =>
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

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => password;

        public bool Verify(string password, string passwordHash) => password == passwordHash;
    }
}
