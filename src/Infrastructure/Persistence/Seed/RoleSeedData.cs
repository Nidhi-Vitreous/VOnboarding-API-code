namespace Vitreous.Onboarding.Infrastructure.Persistence.Seed;

internal static class RoleSeedData
{
    internal sealed record RoleSeedEntry(
        string Name,
        string RoleType,
        int SortOrder,
        bool IsSystemRole,
        string[] PermissionNames);

    internal static readonly RoleSeedEntry[] DefaultRoles =
    [
        new("SUPER ADMIN", "Super Admin", 1, true,
        [
            "users.read", "users.create", "users.update", "users.delete",
            "roles.read", "roles.create", "roles.update", "roles.delete",
            "dashboard.view",
        ]),
        new("Support", "Support", 2, false, ["users.read", "dashboard.view"]),
        new("Merchant", "Merchant", 3, false, ["dashboard.view"]),
        new("Sales Representative", "Sales Representative", 4, false, ["users.read", "dashboard.view"]),
        new("Admin", "Admin", 5, false,
        [
            "users.read", "users.create", "users.update", "users.delete",
            "roles.read", "roles.create", "roles.update", "roles.delete",
            "dashboard.view",
        ]),
        new("Sales Manager", "Sales Manager", 6, false, ["users.read", "users.update", "dashboard.view"]),
        new("Merchant One", "Merchant", 7, false, ["dashboard.view"]),
    ];
}
