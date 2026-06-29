using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

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
}
