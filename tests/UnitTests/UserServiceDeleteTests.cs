using Microsoft.Extensions.Configuration;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class UserProtectionRulesTests
{
    [Fact]
    public void ValidateSuperAdminSelfServiceOnly_allows_super_admin_to_edit_self()
    {
        var user = CreateUser("SUPER ADMIN");

        UserProtectionRules.ValidateSuperAdminSelfServiceOnly(user, user);
    }

    [Fact]
    public void ValidateSuperAdminSelfServiceOnly_blocks_admin_from_editing_super_admin()
    {
        var actor = CreateUser("Admin");
        var target = CreateUser("SUPER ADMIN");

        var exception = Assert.Throws<BusinessRuleException>(() =>
            UserProtectionRules.ValidateSuperAdminSelfServiceOnly(actor, target));

        Assert.Equal(UserMessages.CannotEditSuperAdmin, exception.Message);
    }

    [Fact]
    public void ValidateDeletion_blocks_self_delete()
    {
        var user = CreateUser("Admin");

        var exception = Assert.Throws<BusinessRuleException>(() =>
            UserProtectionRules.ValidateDeletion(user, user));

        Assert.Equal(UserMessages.CannotDeleteOwnAccount, exception.Message);
    }

    [Fact]
    public void ValidateDeletion_blocks_deleting_super_admin_target()
    {
        var actor = CreateUser("Admin");
        var target = CreateUser("SUPER ADMIN");

        var exception = Assert.Throws<BusinessRuleException>(() =>
            UserProtectionRules.ValidateDeletion(actor, target));

        Assert.Equal(UserMessages.CannotDeleteSuperAdmin, exception.Message);
    }

    [Fact]
    public void ValidateDeletion_allows_super_admin_to_delete_admin_target()
    {
        var actor = CreateUser("SUPER ADMIN");
        var target = CreateUser("Admin");

        UserProtectionRules.ValidateDeletion(actor, target);
    }

    [Fact]
    public void ValidateDeletion_blocks_admin_from_deleting_super_admin_assigned_via_roles()
    {
        var actor = CreateUser("Admin");
        var target = CreateUser("Support");
        target.UserRoles.Add(new UserRole
        {
            Role = new Role { Name = "Super Admin" },
        });

        var exception = Assert.Throws<BusinessRuleException>(() =>
            UserProtectionRules.ValidateDeletion(actor, target));

        Assert.Equal(UserMessages.CannotDeleteSuperAdmin, exception.Message);
    }

    private static User CreateUser(string role) => new()
    {
        Id = Guid.NewGuid(),
        Username = "user",
        PasswordHash = "hash",
        Role = role,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };
}

public class UserServiceDeleteTests
{
    [Fact]
    public async Task DeleteAsync_unknown_target_returns_false_without_deleting()
    {
        var actorId = Guid.NewGuid();
        var userRepository = new FakeUserRepository
        {
            Actor = CreateUser(actorId, "SUPER ADMIN"),
        };
        var sut = CreateSut(userRepository);

        var deleted = await sut.DeleteAsync(Guid.NewGuid(), actorId);

        Assert.False(deleted);
        Assert.False(userRepository.DeleteCalled);
    }

    [Fact]
    public async Task DeleteAsync_super_admin_can_delete_admin_user()
    {
        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var userRepository = new FakeUserRepository
        {
            Actor = CreateUser(actorId, "SUPER ADMIN"),
            Target = CreateUser(targetId, "Admin"),
        };
        var sut = CreateSut(userRepository);

        var deleted = await sut.DeleteAsync(targetId, actorId);

        Assert.True(deleted);
        Assert.True(userRepository.DeleteCalled);
        Assert.Equal(targetId, userRepository.LastDeletedUser!.Id);
    }

    [Fact]
    public async Task DeleteAsync_admin_cannot_delete_super_admin()
    {
        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var userRepository = new FakeUserRepository
        {
            Actor = CreateUser(actorId, "Admin"),
            Target = CreateUser(targetId, "SUPER ADMIN"),
        };
        var sut = CreateSut(userRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.DeleteAsync(targetId, actorId));

        Assert.Equal(UserMessages.CannotDeleteSuperAdmin, exception.Message);
        Assert.False(userRepository.DeleteCalled);
    }

    [Fact]
    public async Task DeleteAsync_user_cannot_delete_own_account()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, "SUPER ADMIN");
        var userRepository = new FakeUserRepository
        {
            Actor = user,
            Target = user,
        };
        var sut = CreateSut(userRepository);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.DeleteAsync(userId, userId));

        Assert.Equal(UserMessages.CannotDeleteOwnAccount, exception.Message);
        Assert.False(userRepository.DeleteCalled);
    }

    private static UserService CreateSut(FakeUserRepository userRepository) =>
        new(
            userRepository,
            new FakeRoleRepository(),
            new FakePasswordHasher(),
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Users:EmailDomain"] = "company.local" })
                .Build());

    private static User CreateUser(Guid id, string role) => new()
    {
        Id = id,
        Username = "user",
        PasswordHash = "hash",
        Role = role,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? Actor { get; init; }
        public User? Target { get; init; }
        public bool DeleteCalled { get; private set; }
        public User? LastDeletedUser { get; private set; }

        public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Actor is not null && Actor.Id == id ? Actor : null);

        public Task<User?> GetByIdTrackedWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Target is not null && Target.Id == id ? Target : null);

        public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            DeleteCalled = true;
            LastDeletedUser = user;
            return Task.CompletedTask;
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

        public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task SaveTrackedChangesAsync(CancellationToken cancellationToken = default) =>
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
