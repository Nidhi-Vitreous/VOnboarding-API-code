namespace Vitreous.Onboarding.Infrastructure.Persistence.Seed;

internal static class RoleSeedData
{
    internal sealed record RoleSeedEntry(
        string Name,
        string DepartmentName,
        int SortOrder,
        bool IsSystemRole,
        string[] PermissionNames);

    internal static readonly RoleSeedEntry[] DefaultRoles =
    [
        new("SUPER ADMIN", "Admin", 1, true,
        [
            "merchant.read", "merchant.create", "merchant.update", "merchant.delete",
            "users.read", "users.create", "users.update", "users.delete",
            "roles.read", "roles.create", "roles.update", "roles.delete",
            "dashboard.view",
            "onboarding.read", "onboarding.create", "onboarding.refile", "onboarding.approve",
            "onboarding.reject", "onboarding.hold", "onboarding.resume", "onboarding.block",
            "onboarding.submit", "onboarding.resolve", "onboarding.block.initiate",
        ]),
        new("Support", "Support", 2, false, ["onboarding.read"]),
        new("Sales Rep", "Sales", 3, false,
        [
            "merchant.read", "merchant.create", "merchant.update",
            "onboarding.read", "onboarding.create", "onboarding.refile",
        ]),
        new("Admin", "Admin", 4, false,
        [
            "merchant.read", "merchant.create", "merchant.update", "merchant.delete",
            "users.read", "users.create", "users.update", "users.delete",
            "roles.read", "roles.create", "roles.update", "roles.delete",
            "dashboard.view",
            "onboarding.read", "onboarding.create", "onboarding.refile", "onboarding.approve",
            "onboarding.reject", "onboarding.hold", "onboarding.resume", "onboarding.block",
            "onboarding.submit", "onboarding.resolve", "onboarding.block.initiate",
        ]),
        new("Filing Clerk", "Filing", 5, false,
        [
            "onboarding.read", "onboarding.create", "onboarding.refile", "onboarding.approve",
            "onboarding.reject", "onboarding.hold", "onboarding.resume", "onboarding.block",
        ]),
    ];
}
