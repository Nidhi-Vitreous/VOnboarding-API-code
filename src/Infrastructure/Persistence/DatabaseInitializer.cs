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
            Department = "Administration",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded default admin user (username: {Username}).", adminUsername);
    }

    private static async Task SeedPermissionsAsync(
        ApplicationDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Permissions.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var (permissionId, permissionName) in PermissionSeedData.DefaultPermissions)
        {
            dbContext.Permissions.Add(new Permission
            {
                Id = permissionId,
                Name = permissionName,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} default permissions.", PermissionSeedData.DefaultPermissions.Length);
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

        var permissionsByName = await dbContext.Permissions
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Name, p => p.Id, cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var seedRole in RoleSeedData.DefaultRoles)
        {
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = seedRole.Name,
                RoleType = seedRole.RoleType,
                SortOrder = seedRole.SortOrder,
                IsSystemRole = seedRole.IsSystemRole,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            };

            dbContext.Roles.Add(role);

            foreach (var permissionName in seedRole.PermissionNames.Distinct())
            {
                if (!permissionsByName.TryGetValue(permissionName, out var permissionId))
                {
                    logger.LogWarning("Skipping unknown permission '{PermissionName}' while seeding role '{RoleName}'.",
                        permissionName, seedRole.Name);
                    continue;
                }

                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId,
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} default roles.", RoleSeedData.DefaultRoles.Length);
    }
}
