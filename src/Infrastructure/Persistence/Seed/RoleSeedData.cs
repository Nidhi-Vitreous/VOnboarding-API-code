namespace Vitreous.Onboarding.Infrastructure.Persistence.Seed;

internal static class RoleSeedData
{
    internal sealed record RoleSeedEntry(
        string Name,
        string DepartmentName,
        int SortOrder,
        bool IsSystemRole,
        string[] PermissionNames);

    private static readonly string[] SuperAdminPermissions =
    [
        "merchant.read", "merchant.create", "merchant.update", "merchant.delete",
        "merchant.application.approve", "merchant.application.reject",
        "merchant.application.hold", "merchant.application.complete",
        "merchant.order.approve", "merchant.order.reject",
        "merchant.order.hold", "merchant.order.complete",
        "existing.merchant.order.read", "existing.merchant.order.create",
        "existing.merchant.order.update", "existing.merchant.order.delete",
        "users.read", "users.create", "users.update", "users.delete",
        "roles.read", "roles.create", "roles.update", "roles.delete",
        "dashboard.view",
    ];

    internal static readonly RoleSeedEntry[] DefaultRoles =
    [
        new("SUPER ADMIN", "Admin", 1, true, SuperAdminPermissions),
        new("Support", "Support", 2, false, ["merchant.read"]),
        new("Sales Rep", "Sales", 3, false,
        [
            "merchant.read", "merchant.create", "merchant.update",
        ]),
        new("Admin", "Admin", 4, false, SuperAdminPermissions),
        new("Filing Clerk", "Filing", 5, false,
        [
            "merchant.read", "merchant.create", "merchant.update", "merchant.delete",
            "merchant.application.approve", "merchant.application.reject",
            "merchant.application.hold", "merchant.application.complete",
            "merchant.order.approve", "merchant.order.reject",
            "merchant.order.hold", "merchant.order.complete",
        ]),
    ];
}
