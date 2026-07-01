using Vitreous.Onboarding.Application.Authorization;

namespace Vitreous.Onboarding.Application.Roles;

/// <summary>
/// Maps departments to assignable permission system names and UI groups.
/// </summary>
public static class DepartmentRolePermissionCatalog
{
    private static readonly string[] MerchantPermissions =
    [
        "merchant.read",
        "merchant.create",
        "merchant.update",
        "merchant.delete",
    ];

    private static readonly string[] SalesMerchantPermissions =
    [
        "merchant.read",
        "merchant.create",
        "merchant.update",
    ];

    private static readonly string[] RoleManagementPermissions =
    [
        "roles.read",
        "roles.create",
        "roles.update",
        "roles.delete",
    ];

    private static readonly string[] UserManagementPermissions =
    [
        "users.read",
        "users.create",
        "users.update",
        "users.delete",
    ];

    private static readonly string[] DashboardPermissions =
    [
        PermissionSystemNames.DashboardView,
    ];

    private static readonly string[] AllOnboardingPermissions =
        DepartmentPermissionRegistry.GetAllOnboardingPermissionNames().ToArray();

    private static readonly IReadOnlyDictionary<string, string[]> DepartmentPermissionMap =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Sales"] = Combine(SalesMerchantPermissions, OnboardingFor("Sales")),
            ["Filing"] = OnboardingFor("Filing"),
            ["Terminal"] = OnboardingFor("Terminal"),
            ["Billing"] = OnboardingFor("Billing"),
            ["Shipping"] = OnboardingFor("Shipping"),
            ["Support"] = OnboardingFor("Support"),
            ["Installation"] = OnboardingFor("Installation"),
            ["Admin"] = Combine(
                MerchantPermissions,
                RoleManagementPermissions,
                UserManagementPermissions,
                DashboardPermissions,
                OnboardingFor("Admin")),
        };

    public static IReadOnlyList<string> GetAllowedSystemNames(string departmentName) =>
        DepartmentPermissionMap.TryGetValue(departmentName.Trim(), out var permissions)
            ? permissions
            : [];

    public static IReadOnlyList<DepartmentPermissionGroupDefinition> GetPermissionGroups(string departmentName)
    {
        var allowed = new HashSet<string>(GetAllowedSystemNames(departmentName), StringComparer.OrdinalIgnoreCase);
        if (allowed.Count == 0)
        {
            return [];
        }

        var groups = new List<DepartmentPermissionGroupDefinition>();

        AddGroup(groups, "merchant", "Merchant", MerchantPermissions, allowed);
        AddGroup(groups, "roles", "Roles", RoleManagementPermissions, allowed);
        AddGroup(groups, "users", "Users", UserManagementPermissions, allowed);
        AddGroup(groups, "dashboard", "Dashboard", DashboardPermissions, allowed);
        AddGroup(groups, "onboarding", "Onboarding", AllOnboardingPermissions, allowed);

        return groups;
    }

    private static string[] OnboardingFor(string departmentName) =>
        DepartmentPermissionRegistry.GetOnboardingPermissionNamesForDepartmentName(departmentName).ToArray();

    private static string[] Combine(params IEnumerable<string>[] segments) =>
        segments.SelectMany(segment => segment).ToArray();

    private static void AddGroup(
        List<DepartmentPermissionGroupDefinition> groups,
        string key,
        string title,
        IEnumerable<string> systemNames,
        HashSet<string> allowed)
    {
        var items = systemNames
            .Where(allowed.Contains)
            .Select(systemName => new DepartmentPermissionDefinition(systemName))
            .ToList();

        if (items.Count > 0)
        {
            groups.Add(new DepartmentPermissionGroupDefinition(key, title, items));
        }
    }
}

public sealed record DepartmentPermissionDefinition(string SystemName);

public sealed record DepartmentPermissionGroupDefinition(
    string Key,
    string Title,
    IReadOnlyList<DepartmentPermissionDefinition> Permissions);
