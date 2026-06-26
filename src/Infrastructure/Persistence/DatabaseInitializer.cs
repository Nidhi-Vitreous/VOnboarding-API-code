using Microsoft.EntityFrameworkCore;
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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@vitreous.local",
            FullName = "System Administrator",
            PasswordHash = passwordHasher.Hash("ChangeMe123!"),
            Role = "Admin",
            Department = "Administration",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded default admin user (username: admin).");
    }
}
