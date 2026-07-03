using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;
using Vitreous.Onboarding.Infrastructure.Persistence;
using Vitreous.Onboarding.IntegrationTests.Fixtures;

namespace Vitreous.Onboarding.IntegrationTests.Authorization;

[Collection(PostgresUserTestCollection.Name)]
public sealed class PermissionAuthorizationPostgresTests(PostgresUserTestFixture fixture) : IAsyncLifetime
{
    private static readonly Guid UsersReadPermissionId = Guid.Parse("a1000001-0000-4000-8000-000000000001");
    private static readonly Guid UsersUpdatePermissionId = Guid.Parse("a1000001-0000-4000-8000-000000000003");
    private static readonly Guid MerchantReadPermissionId = Guid.Parse("a1000001-0000-4000-8000-00000000001b");

    public Task InitializeAsync() => fixture.ResetUserDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private void SkipUnlessDatabaseAvailable() =>
        Skip.If(!fixture.IsDatabaseAvailable, PostgresUserTestFixture.SkipReason);

    [SkippableFact]
    public async Task HasSystemPermissionAsync_unions_permissions_across_multiple_roles()
    {
        SkipUnlessDatabaseAvailable();

        await using var scope = fixture.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var permissionAuthorizationService = scope.ServiceProvider.GetRequiredService<IPermissionAuthorizationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await ConfigureRolePermissionsAsync(dbContext);

        var createResult = await userService.CreateAsync(new UserCreateRequest
        {
            FirstName = "Permission",
            LastName = "Union",
            Email = $"inttest.permission.{Guid.NewGuid():N}@example.com",
            Password = "Strong@123",
            ConfirmPassword = "Strong@123",
            RoleIds = [PostgresUserTestFixture.RoleOneId, PostgresUserTestFixture.RoleTwoId],
            IsActive = true,
        });

        var user = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(existingUser => existingUser.Id == createResult.Id);

        var grantedBySecondRole = await permissionAuthorizationService.HasSystemPermissionAsync(
            user,
            PermissionSystemNames.UsersUpdate);
        var notGrantedByEitherRole = await permissionAuthorizationService.HasSystemPermissionAsync(
            user,
            PermissionSystemNames.MerchantRead);

        Assert.True(grantedBySecondRole);
        Assert.False(notGrantedByEitherRole);
    }

    private static async Task ConfigureRolePermissionsAsync(ApplicationDbContext dbContext)
    {
        await EnsurePermissionAsync(
            dbContext,
            UsersReadPermissionId,
            PermissionSystemNames.UsersRead,
            "Read users",
            "Users with this permission can view user records.");
        await EnsurePermissionAsync(
            dbContext,
            UsersUpdatePermissionId,
            PermissionSystemNames.UsersUpdate,
            "Update users",
            "Users with this permission can update existing user records.");
        await EnsurePermissionAsync(
            dbContext,
            MerchantReadPermissionId,
            PermissionSystemNames.MerchantRead,
            "Read merchants",
            "Users with this permission can view merchant records.");

        var existingAssignments = await dbContext.RolePermissions
            .Where(rolePermission =>
                rolePermission.RoleId == PostgresUserTestFixture.RoleOneId
                || rolePermission.RoleId == PostgresUserTestFixture.RoleTwoId)
            .ToListAsync();

        dbContext.RolePermissions.RemoveRange(existingAssignments);

        dbContext.RolePermissions.AddRange(
            CreateRolePermission(PostgresUserTestFixture.RoleOneId, PostgresUserTestFixture.RoleOneName, UsersReadPermissionId, PermissionSystemNames.UsersRead),
            CreateRolePermission(PostgresUserTestFixture.RoleTwoId, PostgresUserTestFixture.RoleTwoName, UsersUpdatePermissionId, PermissionSystemNames.UsersUpdate));

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsurePermissionAsync(
        ApplicationDbContext dbContext,
        Guid id,
        string systemName,
        string name,
        string description)
    {
        if (await dbContext.Permissions.AnyAsync(permission => permission.Id == id))
        {
            return;
        }

        dbContext.Permissions.Add(new Permission
        {
            Id = id,
            SystemName = systemName,
            Name = name,
            Description = description,
        });
    }

    private static RolePermission CreateRolePermission(
        Guid roleId,
        string roleName,
        Guid permissionId,
        string permissionName) =>
        new()
        {
            RoleId = roleId,
            PermissionId = permissionId,
            RoleName = roleName,
            PermissionName = permissionName,
        };
}
