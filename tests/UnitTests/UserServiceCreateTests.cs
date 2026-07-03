using Microsoft.Extensions.Configuration;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class UserServiceCreateTests
{
    private const string ValidPassword = "Password1!";
    private const string DefaultEmailDomain = "company.local";

    [Fact]
    public async Task CreateAsync_valid_multi_role_create_writes_user_roles_and_hashed_password()
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
        Assert.Equal("yash.patel", userRepository.LastAddedUser.Username);
        Assert.Equal($"yash.patel@{DefaultEmailDomain}", userRepository.LastAddedUser.Email);
        Assert.Equal($"yash.patel@{DefaultEmailDomain}", result.Email);
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
    public async Task CreateAsync_derives_matching_username_and_email_from_sanitized_name()
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

        await sut.CreateAsync(ValidRequest([roleId], request =>
        {
            request.FirstName = "José";
            request.LastName = "Singh";
        }));

        Assert.Equal("jose.singh", userRepository.LastAddedUser!.Username);
        Assert.Equal($"jose.singh@{DefaultEmailDomain}", userRepository.LastAddedUser.Email);
    }

    [Fact]
    public async Task CreateAsync_uses_configured_email_domain()
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
        const string customDomain = "vitreous.test";
        var sut = CreateSut(userRepository, roleRepository, configuration: CreateConfiguration(customDomain));

        await sut.CreateAsync(ValidRequest([roleId]));

        Assert.Equal($"yash.patel@{customDomain}", userRepository.LastAddedUser!.Email);
    }

    [Fact]
    public async Task CreateAsync_same_name_assigns_suffix_one_to_both_username_and_email()
    {
        var roleId = Guid.NewGuid();
        var userRepository = new FakeUserRepository
        {
            ExistingUsernames = ["charan.singh"],
            ExistingEmails = [$"charan.singh@{DefaultEmailDomain}"],
        };
        var roleRepository = new FakeRoleRepository
        {
            Roles = new Dictionary<Guid, Role>
            {
                [roleId] = CreateRole(roleId, "Support", "Operations"),
            },
        };
        var sut = CreateSut(userRepository, roleRepository);

        await sut.CreateAsync(ValidRequest([roleId], request =>
        {
            request.FirstName = "Charan";
            request.LastName = "Singh";
        }));

        Assert.Equal(["charan.singh", "charan.singh1"], userRepository.UsernameChecks);
        Assert.Equal([$"charan.singh@{DefaultEmailDomain}", $"charan.singh1@{DefaultEmailDomain}"], userRepository.EmailChecks);
        Assert.Equal("charan.singh1", userRepository.LastAddedUser!.Username);
        Assert.Equal($"charan.singh1@{DefaultEmailDomain}", userRepository.LastAddedUser.Email);
    }

    [Fact]
    public async Task CreateAsync_advances_suffix_when_username_is_taken_but_email_is_free()
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

        Assert.Equal("yash.patel1", userRepository.LastAddedUser!.Username);
        Assert.Equal($"yash.patel1@{DefaultEmailDomain}", userRepository.LastAddedUser.Email);
    }

    [Fact]
    public async Task CreateAsync_unsanitizable_name_throws_and_does_not_persist()
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
                request.FirstName = "---";
                request.LastName = "Patel";
            })));

        Assert.Equal("Cannot derive a login from the provided name.", exception.Message);
        Assert.False(userRepository.AddCalled);
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
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            RoleIds = roleIds.ToList(),
        };

        configure?.Invoke(request);
        return request;
    }

    private static IConfiguration CreateConfiguration(string emailDomain) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Users:EmailDomain"] = emailDomain })
            .Build();

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
        FakePasswordHasher? passwordHasher = null,
        IConfiguration? configuration = null) =>
        new(
            userRepository,
            roleRepository,
            passwordHasher ?? new FakePasswordHasher(),
            configuration ?? CreateConfiguration(DefaultEmailDomain));

    private sealed class FakeUserRepository : IUserRepository
    {
        public HashSet<string> ExistingUsernames { get; init; } = [];
        public HashSet<string> ExistingEmails { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public bool AddCalled { get; private set; }
        public int AddCallCount { get; private set; }
        public User? LastAddedUser { get; private set; }
        public List<string> UsernameChecks { get; } = [];
        public List<string> EmailChecks { get; } = [];

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

        public Task<bool> EmailExistsAsync(
            string email,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default)
        {
            EmailChecks.Add(email);
            return Task.FromResult(ExistingEmails.Contains(email));
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
        public string Hash(string password) => $"HASHED::{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }
}
