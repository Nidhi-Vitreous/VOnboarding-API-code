using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class UserServiceCreateTests
{
    private const string ValidPassword = "Password1!";

    [Fact]
    public async Task CreateAsync_valid_multi_role_create_writes_user_roles_and_sets_first_role_as_legacy_role()
    {
        var primaryRoleId = Guid.NewGuid();
        var secondaryRoleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [primaryRoleId] = CreateRole(primaryRoleId, "Terminal Operator", "Operations"),
                [secondaryRoleId] = CreateRole(secondaryRoleId, "Support", "Customer Success"),
            },
        };
        var passwordHasher = new FakePasswordHasher();
        var sut = CreateSut(userRepository, roleRepository, passwordHasher);

        var result = await sut.CreateAsync(ValidRequest([primaryRoleId, secondaryRoleId]));

        Assert.True(userRepository.AddCalled);
        Assert.Equal(1, userRepository.AddCallCount);
        Assert.NotNull(userRepository.LastAddedUser);
        Assert.Equal("Terminal Operator", userRepository.LastAddedUser!.Role);
        Assert.Equal("Terminal Operator", result.Role);
        Assert.Equal("Yash Patel", userRepository.LastAddedUser.FullName);
        Assert.Equal("Yash", userRepository.LastAddedUser.FirstName);
        Assert.Equal("Patel", userRepository.LastAddedUser.LastName);
        Assert.Equal(2, userRepository.LastAddedUser.UserRoles.Count);
        Assert.Contains(
            userRepository.LastAddedUser.UserRoles,
            userRole => userRole.RoleId == primaryRoleId);
        Assert.Contains(
            userRepository.LastAddedUser.UserRoles,
            userRole => userRole.RoleId == secondaryRoleId);
        Assert.Equal(2, result.Roles.Count);
        Assert.Equal("Terminal Operator", result.Roles[0].Name);
        Assert.Equal("Operations", result.Roles[0].DepartmentName);
        Assert.Equal($"HASHED::{ValidPassword}", userRepository.LastAddedUser.PasswordHash);
        Assert.NotEqual(ValidPassword, userRepository.LastAddedUser.PasswordHash);
        Assert.Null(userRepository.LastAddedUser.LastLoginAt);
        Assert.Null(result.LastLoginAt);
    }

    [Fact]
    public async Task CreateAsync_derives_username_from_email_local_part()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Support", "Operations"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        await sut.CreateAsync(ValidRequest([roleId]));

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
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Support", "Operations"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        await sut.CreateAsync(ValidRequest([roleId]));

        Assert.Equal(["yash.patel", "yash.patel1"], userRepository.UsernameChecks);
        Assert.Equal("yash.patel1", userRepository.LastAddedUser!.Username);
    }

    [Fact]
    public async Task CreateAsync_missing_role_throws_and_does_not_persist()
    {
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository();
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.CreateAsync(ValidRequest([Guid.NewGuid()])));

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
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Retired Role", "Operations", isActive: false),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.CreateAsync(ValidRequest([roleId])));

        Assert.Equal(UserMessages.InvalidRole, exception.Message);
        Assert.Contains(UserMessages.RoleInactive, exception.Details ?? []);
        Assert.False(userRepository.AddCalled);
    }

    [Fact]
    public async Task CreateAsync_password_mismatch_throws_validation_error()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Support", "Operations"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.CreateAsync(ValidRequest([roleId], request =>
            {
                request.ConfirmPassword = "Different1!";
            })));

        Assert.Equal("Validation failed.", exception.Message);
        Assert.Contains("Password and confirm password do not match.", exception.Details ?? []);
        Assert.False(userRepository.AddCalled);
    }

    [Fact]
    public async Task CreateAsync_weak_password_throws_validation_error()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Support", "Operations"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.CreateAsync(ValidRequest([roleId], request =>
            {
                request.Password = "weak";
                request.ConfirmPassword = "weak";
            })));

        Assert.Equal("Validation failed.", exception.Message);
        Assert.Contains(
            "Password must be at least 8 characters and include uppercase, lowercase, digit, and symbol.",
            exception.Details ?? []);
        Assert.False(userRepository.AddCalled);
    }

    [Fact]
    public async Task CreateAsync_empty_role_ids_throws_validation_error()
    {
        var userRepository = new FakeUserRepository();
        var roleRepository = new FakeRoleRepository();
        var sut = CreateSut(userRepository, roleRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.CreateAsync(ValidRequest([])));

        Assert.Equal("Validation failed.", exception.Message);
        Assert.Contains("At least one role is required.", exception.Details ?? []);
        Assert.False(userRepository.AddCalled);
    }

    private static UserCreateRequest ValidRequest(
        IReadOnlyList<Guid> roleIds,
        Action<UserCreateRequest>? configure = null)
    {
        var request = new UserCreateRequest
        {
            FirstName = "Yash",
            LastName = "Patel",
            Email = "yash.patel@example.com",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            RoleIds = roleIds.ToList(),
        };

        configure?.Invoke(request);
        return request;
    }

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

        public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByIdTrackedWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
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

        public Task SaveTrackedChangesAsync(CancellationToken cancellationToken = default) =>
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
        public string Hash(string password) => $"HASHED::{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }
}
