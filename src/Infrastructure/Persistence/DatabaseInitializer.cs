using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;
using Vitreous.Onboarding.Infrastructure.Persistence.Seed;

namespace Vitreous.Onboarding.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await dbContext.Database.MigrateAsync(cancellationToken);
        await SeedDepartmentsAsync(dbContext, logger, cancellationToken);
        await SeedPermissionsAsync(dbContext, logger, cancellationToken);

        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"];
        if (string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Skipping database seed in Production.");
            return;
        }

        if (!configuration.GetValue("Seed:Enabled", false))
        {
            return;
        }

        await SeedRolesAsync(dbContext, logger, cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var adminUsername = configuration["Seed:AdminUsername"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Seed:Enabled is true but Seed:AdminUsername or Seed:AdminPassword is missing. Skipping admin seed.");
            return;
        }

        var now = DateTime.UtcNow;
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = adminUsername,
            Email = "admin@vitreous.local",
            FullName = "System Administrator",
            PasswordHash = passwordHasher.Hash(adminPassword),
            Role = "Admin",
            Department = "Admin",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded default admin user (username: {Username}).", adminUsername);
    }

    private static async Task SeedDepartmentsAsync(
        ApplicationDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var existingDepartments = await dbContext.Departments.ToListAsync(cancellationToken);
        var departmentsById = existingDepartments.ToDictionary(d => d.Id);
        var departmentsByName = existingDepartments.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var seedDepartment in DepartmentSeedData.DefaultDepartments)
        {
            if (departmentsById.ContainsKey(seedDepartment.Id)
                || departmentsByName.ContainsKey(seedDepartment.Name))
            {
                continue;
            }

            dbContext.Departments.Add(new Department
            {
                Id = seedDepartment.Id,
                Name = seedDepartment.Name,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Synchronized {Count} department definitions.", DepartmentSeedData.DefaultDepartments.Length);
    }

    private static async Task SeedPermissionsAsync(
        ApplicationDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var existingPermissions = await dbContext.Permissions.ToListAsync(cancellationToken);
        var permissionsById = existingPermissions.ToDictionary(p => p.Id);
        var permissionsBySystemName = existingPermissions.ToDictionary(p => p.SystemName, StringComparer.OrdinalIgnoreCase);

        foreach (var seedPermission in PermissionSeedData.DefaultPermissions)
        {
            if (permissionsById.TryGetValue(seedPermission.Id, out var existingById))
            {
                SyncPermission(existingById, seedPermission);
                continue;
            }

            if (permissionsBySystemName.TryGetValue(seedPermission.SystemName, out var existingBySystemName))
            {
                SyncPermission(existingBySystemName, seedPermission);
                continue;
            }

            dbContext.Permissions.Add(new Permission
            {
                Id = seedPermission.Id,
                SystemName = seedPermission.SystemName,
                Name = seedPermission.Name,
                Description = seedPermission.Description,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Synchronized {Count} permission definitions.", PermissionSeedData.DefaultPermissions.Length);
    }

    private static void SyncPermission(Permission permission, PermissionSeedData.PermissionSeedEntry seedPermission)
    {
        if (string.IsNullOrWhiteSpace(permission.SystemName))
        {
            permission.SystemName = seedPermission.SystemName;
        }

        permission.Name = seedPermission.Name;
        permission.Description = seedPermission.Description;
    }

    private static async Task SeedRolesAsync(
        ApplicationDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Roles.AnyAsync(cancellationToken))
        {
            return;
        }

        var permissionsBySystemName = await dbContext.Permissions
            .AsNoTracking()
            .ToDictionaryAsync(p => p.SystemName, p => p, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var departmentsByName = await dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(d => d.Name, d => d.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var seedRole in RoleSeedData.DefaultRoles)
        {
            if (!departmentsByName.TryGetValue(seedRole.DepartmentName, out var departmentId))
            {
                logger.LogWarning(
                    "Skipping role '{RoleName}' because department '{DepartmentName}' was not found.",
                    seedRole.Name,
                    seedRole.DepartmentName);
                continue;
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = seedRole.Name,
                RoleType = seedRole.DepartmentName,
                DepartmentId = departmentId,
                SortOrder = seedRole.SortOrder,
                IsSystemRole = seedRole.IsSystemRole,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            };

            dbContext.Roles.Add(role);

            foreach (var permissionSystemName in seedRole.PermissionNames.Distinct())
            {
                if (!permissionsBySystemName.TryGetValue(permissionSystemName, out var permission))
                {
                    logger.LogWarning(
                        "Skipping unknown permission '{PermissionSystemName}' while seeding role '{RoleName}'.",
                        permissionSystemName,
                        seedRole.Name);
                    continue;
                }

                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    RoleName = role.Name,
                    PermissionName = permission.Name,
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} default roles.", RoleSeedData.DefaultRoles.Length);
    }
}
