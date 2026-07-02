using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class UserServiceCreateTests
{
    [Fact]
    public async Task CreateAsync_valid_active_role_creates_user_with_hashed_password()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository
        {
            Role = new Role
            {
                Id = roleId,
                Name = "Terminal Operator",
                IsActive = true,
            },
        };
        var passwordHasher = new FakePasswordHasher();
        var sut = CreateSut(userRepository, roleRepository, passwordHasher);

        var result = await sut.CreateAsync(new UserCreateRequest
        {
            FullName = "Yash Patel",
            Email = "yash.patel@example.com",
            RoleId = roleId,
        });

        Assert.True(userRepository.AddCalled);
        Assert.Equal(1, userRepository.AddCallCount);
        Assert.NotNull(userRepository.LastAddedUser);
        Assert.Equal("Terminal Operator", userRepository.LastAddedUser!.Role);
        Assert.Equal("Terminal Operator", result.User.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.TemporaryPassword));
        Assert.NotEqual(result.TemporaryPassword, userRepository.LastAddedUser.PasswordHash);
        Assert.Equal($"HASHED::{result.TemporaryPassword}", userRepository.LastAddedUser.PasswordHash);
        Assert.Null(userRepository.LastAddedUser.LastLoginAt);
        Assert.Null(result.User.LastLoginAt);
    }

    [Fact]
    public async Task CreateAsync_derives_username_from_email_local_part()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository
        {
            Role = new Role { Id = roleId, Name = "Support", IsActive = true },
        };
        var sut = CreateSut(userRepository, roleRepository);

        await sut.CreateAsync(new UserCreateRequest
        {
            FullName = "Yash Patel",
            Email = "yash.patel@example.com",
            RoleId = roleId,
        });

        Assert.Equal("yash.patel", userRepository.LastAddedUser!.Username);
    }

    [Fact]
    public async Task CreateAsync_deduplicates_username_when_local_part_already_exists()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository
        {
            ExistingUsernames = ["yash.patel"],
        };
        var roleRepository = new FakeRoleRepository
        {
            Role = new Role { Id = roleId, Name = "Support", IsActive = true },
        };
        var sut = CreateSut(userRepository, roleRepository);

        await sut.CreateAsync(new UserCreateRequest
        {
            FullName = "Yash Patel",
            Email = "yash.patel@example.com",
            RoleId = roleId,
        });

        Assert.Equal(["yash.patel", "yash.patel1"], userRepository.UsernameChecks);
        Assert.Equal("yash.patel1", userRepository.LastAddedUser!.Username);
    }

    [Fact]
    public async Task CreateAsync_missing_role_throws_and_does_not_persist()
    {
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository { Role = null };
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.CreateAsync(new UserCreateRequest
            {
                FullName = "Yash Patel",
                Email = "yash.patel@example.com",
                RoleId = Guid.NewGuid(),
            }));

        Assert.Equal(UserMessages.InvalidRole, exception.Message);
        Assert.Contains(UserMessages.RoleNotFound, exception.Details ?? []);
        Assert.False(userRepository.AddCalled);
    }

    [Fact]
    public async Task CreateAsync_inactive_role_throws_and_does_not_persist()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
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
            sut.CreateAsync(new UserCreateRequest
            {
                FullName = "Yash Patel",
                Email = "yash.patel@example.com",
                RoleId = roleId,
            }));

        Assert.Equal(UserMessages.InvalidRole, exception.Message);
        Assert.Contains(UserMessages.RoleInactive, exception.Details ?? []);
        Assert.False(userRepository.AddCalled);
    }

    private static UserService CreateSut(
        FakeUserRepository userRepository,
        FakeRoleRepository roleRepository,
        FakePasswordHasher? passwordHasher = null) =>
        new(userRepository, roleRepository, passwordHasher ?? new FakePasswordHasher());

    private sealed class FakeUserRepository : IUserRepository
    {
        public HashSet<string> ExistingUsernames { get; init; } = [];
        public bool AddCalled { get; private set; }
        public int AddCallCount { get; private set; }
        public User? LastAddedUser { get; private set; }
        public List<string> UsernameChecks { get; } = [];

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            AddCalled = true;
            AddCallCount++;
            LastAddedUser = user;
            return Task.CompletedTask;
        }

        public Task<bool> UsernameExistsAsync(
            string username,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default)
        {
            UsernameChecks.Add(username);
            return Task.FromResult(ExistingUsernames.Contains(username));
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
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

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
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
        public string Hash(string password) => $"HASHED::{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }
}
