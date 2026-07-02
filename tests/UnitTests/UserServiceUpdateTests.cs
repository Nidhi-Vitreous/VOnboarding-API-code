using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class UserServiceUpdateTests
{
    [Fact]
    public async Task UpdateAsync_unknown_id_returns_null_without_persisting()
    {
        var userRepository = new FakeUserRepository();
        var sut = CreateSut(userRepository);

        var result = await sut.UpdateAsync(Guid.NewGuid(), new UserUpdateRequest { Department = "New" });

        Assert.Null(result);
        Assert.False(userRepository.UpdateCalled);
    }

    [Fact]
    public async Task UpdateAsync_valid_active_role_updates_role_name_and_persists()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateUser(userId);
        var userRepository = new FakeUserRepository { User = user };
        var roleRepository = new FakeRoleRepository
        {
            Role = new Role
            {
                Id = roleId,
                Name = "Terminal Operator",
                IsActive = true,
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var result = await sut.UpdateAsync(userId, new UserUpdateRequest { RoleId = roleId });

        Assert.NotNull(result);
        Assert.Equal("Terminal Operator", result.Role);
        Assert.True(userRepository.UpdateCalled);
        Assert.NotNull(userRepository.LastUpdatedUser);
        Assert.Equal("Terminal Operator", userRepository.LastUpdatedUser!.Role);
    }

    [Fact]
    public async Task UpdateAsync_missing_role_throws_and_does_not_persist()
    {
        var userId = Guid.NewGuid();
        var userRepository = new FakeUserRepository { User = CreateUser(userId) };
        var roleRepository = new FakeRoleRepository { Role = null };
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.UpdateAsync(userId, new UserUpdateRequest { RoleId = Guid.NewGuid() }));

        Assert.Equal(UserMessages.InvalidRole, exception.Message);
        Assert.Contains(UserMessages.RoleNotFound, exception.Details ?? []);
        Assert.False(userRepository.UpdateCalled);
    }

    [Fact]
    public async Task UpdateAsync_inactive_role_throws_and_does_not_persist()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository { User = CreateUser(userId) };
        var roleRepository = new FakeRoleRepository
        {
            Role = new Role
            {
                Id = roleId,
                Name = "Retired Role",
                IsActive = false,
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.UpdateAsync(userId, new UserUpdateRequest { RoleId = roleId }));

        Assert.Equal(UserMessages.InvalidRole, exception.Message);
        Assert.Contains(UserMessages.RoleInactive, exception.Details ?? []);
        Assert.False(userRepository.UpdateCalled);
    }

    [Fact]
    public async Task UpdateAsync_department_only_leaves_role_and_phone_unchanged()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.Role = "Support";
        user.PhoneNumber = "+15550001111";
        var userRepository = new FakeUserRepository { User = user };
        var sut = CreateSut(userRepository);

        var result = await sut.UpdateAsync(userId, new UserUpdateRequest { Department = "  Billing  " });

        Assert.NotNull(result);
        Assert.Equal("Billing", result.Department);
        Assert.Equal("Support", result.Role);
        Assert.Equal("+15550001111", result.PhoneNumber);
        Assert.True(userRepository.UpdateCalled);
        Assert.Equal("Billing", userRepository.LastUpdatedUser!.Department);
        Assert.Equal("Support", userRepository.LastUpdatedUser.Role);
        Assert.Equal("+15550001111", userRepository.LastUpdatedUser.PhoneNumber);
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
        Department = "Ops",
        PhoneNumber = "+15550001111",
        IsActive = true,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? User { get; init; }
        public bool UpdateCalled { get; private set; }
        public User? LastUpdatedUser { get; private set; }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(User is not null && User.Id == id ? User : null);

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            UpdateCalled = true;
            LastUpdatedUser = user;
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
            throw new NotImplementedException();

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
        public Role? Role { get; init; }

        public Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Role is not null && Role.Id == id ? Role : null);

        public Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default) =>
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
