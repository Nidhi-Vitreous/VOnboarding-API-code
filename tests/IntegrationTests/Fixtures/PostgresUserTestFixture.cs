using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Vitreous.Onboarding.Application;
using Vitreous.Onboarding.Domain.Entities;
using Vitreous.Onboarding.Infrastructure;
using Vitreous.Onboarding.Infrastructure.Persistence;

namespace Vitreous.Onboarding.IntegrationTests.Fixtures;

public sealed class PostgresUserTestFixture : IAsyncLifetime
{
    public const string RequiredDatabaseName = "vonboarding_test";

    public const string SkipReason =
        "SKIPPED: set TEST_DB_CONNECTION or user-secrets to run Postgres integration tests";

    public static readonly Guid AdminDepartmentId = Guid.Parse("f1000001-0000-4000-8000-000000000001");
    public static readonly Guid SupportDepartmentId = Guid.Parse("f1000001-0000-4000-8000-000000000002");
    public static readonly Guid SalesDepartmentId = Guid.Parse("f1000001-0000-4000-8000-000000000003");

    public static readonly Guid RoleOneId = Guid.Parse("f2000001-0000-4000-8000-000000000001");
    public static readonly Guid RoleTwoId = Guid.Parse("f2000001-0000-4000-8000-000000000002");
    public static readonly Guid RoleThreeId = Guid.Parse("f2000001-0000-4000-8000-000000000003");

    public const string RoleOneName = "INTTEST Role One";
    public const string RoleTwoName = "INTTEST Role Two";
    public const string RoleThreeName = "INTTEST Role Three";

    public const string AdminDepartmentName = "INTTEST Admin";
    public const string SupportDepartmentName = "INTTEST Support";
    public const string SalesDepartmentName = "INTTEST Sales";

    public bool IsDatabaseAvailable { get; private set; }

    public IServiceProvider ServiceProvider { get; private set; } = null!;

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Testing.json", optional: false)
                .AddUserSecrets<PostgresUserTestFixture>(optional: true)
                .Build();

            ConnectionString = TestDatabaseConnection.Resolve(configuration);

            var databaseName = new NpgsqlConnectionStringBuilder(ConnectionString).Database
                ?? throw new InvalidOperationException("Database name is missing from the connection string.");

            if (!string.Equals(databaseName, RequiredDatabaseName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Integration tests must use database '{RequiredDatabaseName}', not '{databaseName}'.");
            }

            await EnsureDatabaseExistsAsync(ConnectionString);

            var testConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                    ["Jwt:Secret"] = configuration["Jwt:Secret"]
                        ?? "TestOnlySecretKey_ForIntegrationTests_Min32Chars!",
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(testConfiguration);
            services.AddInfrastructure(testConfiguration);
            services.AddApplication();
            ServiceProvider = services.BuildServiceProvider();

            await using var scope = ServiceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
            await SeedReferenceDataAsync(dbContext);

            IsDatabaseAvailable = true;
        }
        catch (Exception)
        {
            IsDatabaseAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (!IsDatabaseAvailable || ServiceProvider is null)
        {
            return;
        }

        try
        {
            await ResetUserDataAsync();
        }
        catch
        {
            // Best-effort cleanup; do not mask test failures during fixture teardown.
        }

        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public async Task ResetUserDataAsync(CancellationToken cancellationToken = default)
    {
        if (!IsDatabaseAvailable)
        {
            return;
        }

        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE user_roles, refresh_tokens, users CASCADE;",
            cancellationToken);
    }

    public AsyncServiceScope CreateScope() => ServiceProvider.CreateAsyncScope();

    private static async Task EnsureDatabaseExistsAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database
            ?? throw new InvalidOperationException("Database name is missing from the connection string.");

        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
        checkCommand.Parameters.AddWithValue("name", databaseName);
        var exists = await checkCommand.ExecuteScalarAsync() is not null;

        if (exists)
        {
            return;
        }

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"""CREATE DATABASE "{databaseName.Replace("\"", "\"\"")}" """;
        await createCommand.ExecuteNonQueryAsync();
    }

    private static async Task SeedReferenceDataAsync(ApplicationDbContext dbContext)
    {
        if (!await dbContext.Departments.AnyAsync(d => d.Id == AdminDepartmentId))
        {
            dbContext.Departments.AddRange(
                new Department { Id = AdminDepartmentId, Name = AdminDepartmentName },
                new Department { Id = SupportDepartmentId, Name = SupportDepartmentName },
                new Department { Id = SalesDepartmentId, Name = SalesDepartmentName });
        }

        var now = DateTime.UtcNow;
        var seedRoles = new[]
        {
            new Role
            {
                Id = RoleOneId,
                Name = RoleOneName,
                RoleType = "System",
                SortOrder = 1,
                IsSystemRole = false,
                IsActive = true,
                DepartmentId = AdminDepartmentId,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new Role
            {
                Id = RoleTwoId,
                Name = RoleTwoName,
                RoleType = "System",
                SortOrder = 2,
                IsSystemRole = false,
                IsActive = true,
                DepartmentId = SupportDepartmentId,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new Role
            {
                Id = RoleThreeId,
                Name = RoleThreeName,
                RoleType = "System",
                SortOrder = 3,
                IsSystemRole = false,
                IsActive = true,
                DepartmentId = SalesDepartmentId,
                CreatedAt = now,
                UpdatedAt = now,
            },
        };

        foreach (var seedRole in seedRoles)
        {
            if (!await dbContext.Roles.AnyAsync(r => r.Id == seedRole.Id))
            {
                dbContext.Roles.Add(seedRole);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

[CollectionDefinition(PostgresUserTestCollection.Name)]
public sealed class PostgresUserTestCollection : ICollectionFixture<PostgresUserTestFixture>
{
    public const string Name = "PostgresUserTests";
}
