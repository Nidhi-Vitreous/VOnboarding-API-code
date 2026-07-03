using Microsoft.Extensions.Configuration;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class UserServiceStatusTests
{
    [Fact]
    public async Task SetStatusAsync_unknown_id_returns_null_without_persisting()
    {
        var userRepository = new FakeUserRepository();
        var sut = CreateSut(userRepository);

        var result = await sut.SetStatusAsync(
            Guid.NewGuid(),
            new UserStatusUpdateRequest { IsActive = false });

        Assert.Null(result);
        Assert.False(userRepository.UpdateCalled);
    }

    [Fact]
    public async Task SetStatusAsync_deactivate_active_user_persists_and_returns_response()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, isActive: true);
        var userRepository = new FakeUserRepository { User = user };
        var sut = CreateSut(userRepository);

        var result = await sut.SetStatusAsync(
            userId,
            new UserStatusUpdateRequest { IsActive = false });

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.False(result.IsActive);
        Assert.True(userRepository.UpdateCalled);
        Assert.NotNull(userRepository.LastUpdatedUser);
        Assert.False(userRepository.LastUpdatedUser!.IsActive);
        Assert.Equal(result.UpdatedAt, userRepository.LastUpdatedUser.UpdatedAt);
    }

    [Fact]
    public async Task SetStatusAsync_activate_inactive_user_persists_and_returns_response()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, isActive: false);
        var userRepository = new FakeUserRepository { User = user };
        var sut = CreateSut(userRepository);

        var result = await sut.SetStatusAsync(
            userId,
            new UserStatusUpdateRequest { IsActive = true });

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.True(result.IsActive);
        Assert.True(userRepository.UpdateCalled);
        Assert.NotNull(userRepository.LastUpdatedUser);
        Assert.True(userRepository.LastUpdatedUser!.IsActive);
        Assert.Equal(result.UpdatedAt, userRepository.LastUpdatedUser.UpdatedAt);
    }

    private static UserService CreateSut(FakeUserRepository userRepository) =>
        new(
            userRepository,
            new FakeRoleRepository(),
            new FakePasswordHasher(),
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Users:EmailDomain"] = "company.local" })
                .Build());

    private static User CreateUser(Guid id, bool isActive) => new()
    {
        Id = id,
        Username = "jane",
        Email = "jane@example.com",
        PasswordHash = "hash",
        Role = "Support",
        Department = "Ops",
        PhoneNumber = "+15550001111",
        IsActive = isActive,
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

        public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByIdTrackedWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            UpdateCalled = true;
            LastUpdatedUser = user;
            return Task.CompletedTask;
        }

        public Task SaveTrackedChangesAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

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

        public Task<bool> EmailExistsAsync(
            string email,
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
        public Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

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

        public Task<IReadOnlyList<Role>> GetRolesWithPermissionsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
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
