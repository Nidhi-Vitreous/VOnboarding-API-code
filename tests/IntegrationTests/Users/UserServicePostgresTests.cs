using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Infrastructure.Persistence;
using Vitreous.Onboarding.IntegrationTests.Fixtures;

namespace Vitreous.Onboarding.IntegrationTests.Users;

[Collection(PostgresUserTestCollection.Name)]
public sealed class UserServicePostgresTests(PostgresUserTestFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() =>
        fixture.IsDatabaseAvailable ? fixture.ResetUserDataAsync() : Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    private void SkipUnlessDatabaseAvailable() =>
        Skip.If(!fixture.IsDatabaseAvailable, PostgresUserTestFixture.SkipReason);

    [SkippableFact]
    public async Task CreateAsync_with_two_roles_persists_user_roles_and_hashed_password()
    {
        SkipUnlessDatabaseAvailable();
        await using var scope = fixture.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        const string plainPassword = "Strong@123";

        var result = await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Integration",
            LastName = "Create",
            Password = plainPassword,
            ConfirmPassword = plainPassword,
            PhoneNumber = "5551234567",
            RoleIds = [PostgresUserTestFixture.RoleOneId, PostgresUserTestFixture.RoleTwoId],
            IsActive = true,
        });

        var persistedRoleIds = await dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == result.Id)
            .Select(userRole => userRole.RoleId)
            .ToListAsync();

        Assert.Equal(2, persistedRoleIds.Count);
        Assert.Contains(PostgresUserTestFixture.RoleOneId, persistedRoleIds);
        Assert.Contains(PostgresUserTestFixture.RoleTwoId, persistedRoleIds);

        var persistedUser = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == result.Id);

        Assert.NotEqual(plainPassword, persistedUser.PasswordHash);
        Assert.True(passwordHasher.Verify(plainPassword, persistedUser.PasswordHash));
        Assert.Equal(PostgresUserTestFixture.RoleOneName, persistedUser.Role);

        Assert.Equal(2, result.Roles.Count);
        Assert.Contains(
            result.Roles,
            role => role.Id == PostgresUserTestFixture.RoleOneId
                && role.Name == PostgresUserTestFixture.RoleOneName
                && role.DepartmentName == PostgresUserTestFixture.AdminDepartmentName);
        Assert.Contains(
            result.Roles,
            role => role.Id == PostgresUserTestFixture.RoleTwoId
                && role.Name == PostgresUserTestFixture.RoleTwoName
                && role.DepartmentName == PostgresUserTestFixture.SupportDepartmentName);
    }

    [SkippableFact]
    public async Task UpdateAsync_replaces_role_set_in_database()
    {
        SkipUnlessDatabaseAvailable();
        await using var scope = fixture.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var createResult = await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Integration",
            LastName = "Update",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            RoleIds = [PostgresUserTestFixture.RoleOneId, PostgresUserTestFixture.RoleTwoId],
            IsActive = true,
        });

        var updateResult = await userService.UpdateAsync(createResult.Id, new UserUpdateRequest
        {
            FirstName = "Integration",
            LastName = "Update",
            PhoneNumber = "5551234567",
            RoleIds = [PostgresUserTestFixture.RoleTwoId, PostgresUserTestFixture.RoleThreeId],
        });

        Assert.NotNull(updateResult);

        var persistedRoleIds = await dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == createResult.Id)
            .Select(userRole => userRole.RoleId)
            .ToListAsync();

        Assert.Equal(2, persistedRoleIds.Count);
        Assert.DoesNotContain(PostgresUserTestFixture.RoleOneId, persistedRoleIds);
        Assert.Contains(PostgresUserTestFixture.RoleTwoId, persistedRoleIds);
        Assert.Contains(PostgresUserTestFixture.RoleThreeId, persistedRoleIds);

        Assert.False(await dbContext.UserRoles.AnyAsync(userRole =>
            userRole.UserId == createResult.Id && userRole.RoleId == PostgresUserTestFixture.RoleOneId));
        Assert.True(await dbContext.UserRoles.AnyAsync(userRole =>
            userRole.UserId == createResult.Id && userRole.RoleId == PostgresUserTestFixture.RoleThreeId));

        Assert.Equal(PostgresUserTestFixture.RoleTwoName, updateResult.Role);
        Assert.Equal(2, updateResult.Roles.Count);
        Assert.Contains(updateResult.Roles, role => role.Id == PostgresUserTestFixture.RoleTwoId);
        Assert.Contains(updateResult.Roles, role => role.Id == PostgresUserTestFixture.RoleThreeId);
    }

    [SkippableFact]
    public async Task UpdateAsync_unknown_id_returns_null_and_does_not_change_database()
    {
        SkipUnlessDatabaseAvailable();
        await using var scope = fixture.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Existing",
            LastName = "User",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            RoleIds = [PostgresUserTestFixture.RoleOneId],
            IsActive = true,
        });

        var usersBefore = await dbContext.Users.AsNoTracking().CountAsync();
        var userRolesBefore = await dbContext.UserRoles.AsNoTracking().CountAsync();

        var result = await userService.UpdateAsync(Guid.NewGuid(), new UserUpdateRequest
        {
            FirstName = "Ghost",
            LastName = "User",
            RoleIds = [PostgresUserTestFixture.RoleTwoId],
        });

        Assert.Null(result);

        Assert.Equal(usersBefore, await dbContext.Users.AsNoTracking().CountAsync());
        Assert.Equal(userRolesBefore, await dbContext.UserRoles.AsNoTracking().CountAsync());
    }

    [SkippableFact]
    public async Task Read_paths_load_roles_with_name_and_department()
    {
        SkipUnlessDatabaseAvailable();
        await using var scope = fixture.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        const string officeNumber = "5559876543";
        const string notes = "Integration read-path notes";
        const bool twoFactorEnabled = true;

        var createResult = await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Integration",
            LastName = "Read",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            OfficeNumber = officeNumber,
            Notes = notes,
            TwoFactorEnabled = twoFactorEnabled,
            RoleIds = [PostgresUserTestFixture.RoleOneId, PostgresUserTestFixture.RoleTwoId],
            IsActive = true,
        });

        var userById = await userRepository.GetByIdWithRolesAsync(createResult.Id);
        Assert.NotNull(userById);
        Assert.Equal(officeNumber, userById.OfficeNumber);
        Assert.Equal(notes, userById.Notes);
        Assert.Equal(twoFactorEnabled, userById.TwoFactorEnabled);
        Assert.Equal(2, userById.UserRoles.Count);
        foreach (var userRole in userById.UserRoles)
        {
            Assert.NotNull(userRole.Role);
            Assert.False(string.IsNullOrWhiteSpace(userRole.Role.Name));
            Assert.NotNull(userRole.Role.Department);
            Assert.False(string.IsNullOrWhiteSpace(userRole.Role.Department.Name));
        }

        var (pageItems, totalCount) = await userRepository.GetPageAsync(1, 10, search: null);
        Assert.True(totalCount >= 1);
        var pagedUser = pageItems.Single(user => user.Id == createResult.Id);
        Assert.Equal(2, pagedUser.UserRoles.Count);
        foreach (var userRole in pagedUser.UserRoles)
        {
            Assert.NotNull(userRole.Role);
            Assert.False(string.IsNullOrWhiteSpace(userRole.Role.Name));
            Assert.NotNull(userRole.Role.Department);
            Assert.False(string.IsNullOrWhiteSpace(userRole.Role.Department.Name));
        }

        var detail = await userService.GetByIdAsync(createResult.Id);
        Assert.NotNull(detail);
        Assert.Equal(officeNumber, detail.OfficeNumber);
        Assert.Equal(notes, detail.Notes);
        Assert.Equal(twoFactorEnabled, detail.TwoFactorEnabled);
        Assert.Equal(2, detail.Roles.Count);
        Assert.Contains(
            detail.Roles,
            role => role.Id == PostgresUserTestFixture.RoleOneId
                && role.Name == PostgresUserTestFixture.RoleOneName
                && role.DepartmentName == PostgresUserTestFixture.AdminDepartmentName);
        Assert.Contains(
            detail.Roles,
            role => role.Id == PostgresUserTestFixture.RoleTwoId
                && role.Name == PostgresUserTestFixture.RoleTwoName
                && role.DepartmentName == PostgresUserTestFixture.SupportDepartmentName);

        var list = await userService.GetAllAsync(1, 10);
        var summary = list.Data.Single(user => user.Id == createResult.Id);
        Assert.Equal(2, summary.Roles.Count);
        Assert.Contains(summary.Roles, role => role.Id == PostgresUserTestFixture.RoleOneId);
        Assert.Contains(summary.Roles, role => role.Id == PostgresUserTestFixture.RoleTwoId);

        var trackedRoleNames = await dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.UserId == createResult.Id)
            .Join(
                dbContext.Roles.AsNoTracking(),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Name)
            .ToListAsync();

        Assert.Equal(2, trackedRoleNames.Count);
        Assert.All(detail.Roles, role => Assert.Contains(role.Name, trackedRoleNames));
    }

    [SkippableFact]
    public async Task GetProfileAsync_returns_roles_with_department_and_deduped_departments()
    {
        SkipUnlessDatabaseAvailable();
        await using var scope = fixture.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var createResult = await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Integration",
            LastName = "Profile",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            RoleIds = [PostgresUserTestFixture.RoleOneId, PostgresUserTestFixture.RoleTwoId],
            IsActive = true,
        });

        var profile = await userService.GetProfileAsync(createResult.Id);

        Assert.NotNull(profile);
        Assert.Equal(PostgresUserTestFixture.RoleOneName, profile.Role);
        Assert.Equal(2, profile.Roles.Count);
        Assert.Contains(
            profile.Roles,
            role => role.Id == PostgresUserTestFixture.RoleOneId
                && role.Name == PostgresUserTestFixture.RoleOneName
                && role.DepartmentName == PostgresUserTestFixture.AdminDepartmentName);
        Assert.Contains(
            profile.Roles,
            role => role.Id == PostgresUserTestFixture.RoleTwoId
                && role.Name == PostgresUserTestFixture.RoleTwoName
                && role.DepartmentName == PostgresUserTestFixture.SupportDepartmentName);

        Assert.Equal(2, profile.Departments.Count);
        Assert.Equal(PostgresUserTestFixture.AdminDepartmentName, profile.Departments[0]);
        Assert.Equal(PostgresUserTestFixture.SupportDepartmentName, profile.Departments[1]);
    }

    [SkippableFact]
    public async Task CreateAsync_same_name_assigns_matching_suffix_to_username_and_email()
    {
        SkipUnlessDatabaseAvailable();
        await using var scope = fixture.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        const string emailDomain = "company.local";

        var firstResult = await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Charan",
            LastName = "Singh",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            RoleIds = [PostgresUserTestFixture.RoleOneId],
            IsActive = true,
        });

        var secondResult = await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Charan",
            LastName = "Singh",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            RoleIds = [PostgresUserTestFixture.RoleOneId],
            IsActive = true,
        });

        var firstUser = await dbContext.Users.AsNoTracking().SingleAsync(user => user.Id == firstResult.Id);
        var secondUser = await dbContext.Users.AsNoTracking().SingleAsync(user => user.Id == secondResult.Id);

        Assert.Equal("charan.singh", firstUser.Username);
        Assert.Equal($"charan.singh@{emailDomain}", firstUser.Email);
        Assert.Equal($"charan.singh@{emailDomain}", firstResult.Email);

        Assert.Equal("charan.singh1", secondUser.Username);
        Assert.Equal($"charan.singh1@{emailDomain}", secondUser.Email);
        Assert.Equal($"charan.singh1@{emailDomain}", secondResult.Email);
    }
}
