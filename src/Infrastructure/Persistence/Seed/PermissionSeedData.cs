namespace Vitreous.Onboarding.Infrastructure.Persistence.Seed;

/// <summary>
/// Default permissions seeded into the database (not used in business logic).
/// </summary>
internal static class PermissionSeedData
{
    internal static readonly (Guid Id, string Name)[] DefaultPermissions =
    [
        (Guid.Parse("a1000001-0000-4000-8000-000000000001"), "users.read"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000002"), "users.create"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000003"), "users.update"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000004"), "users.delete"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000005"), "roles.read"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000006"), "roles.create"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000007"), "roles.update"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000008"), "roles.delete"),
        (Guid.Parse("a1000001-0000-4000-8000-000000000009"), "dashboard.view"),
    ];
}
