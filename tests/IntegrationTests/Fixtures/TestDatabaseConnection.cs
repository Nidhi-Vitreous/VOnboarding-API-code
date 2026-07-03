using Microsoft.Extensions.Configuration;

namespace Vitreous.Onboarding.IntegrationTests.Fixtures;

internal static class TestDatabaseConnection
{
    internal const string EnvironmentVariableName = "TEST_DB_CONNECTION";

    internal const string PlaceholderConnectionString =
        "Host=localhost;Port=5432;Database=vonboarding_test;Username=postgres;Password=postgres";

    internal static string Resolve(IConfiguration configuration)
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment.Trim();
        }

        var fromConfiguration = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromConfiguration))
        {
            return fromConfiguration;
        }

        return PlaceholderConnectionString;
    }
}
