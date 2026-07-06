using Vitreous.Onboarding.Application.Authorization;

namespace Vitreous.Onboarding.Application.Roles;

/// <summary>
/// Maps departments to assignable permission system names and UI groups.
/// </summary>
public static class DepartmentRolePermissionCatalog
{
    private static readonly string[] MerchantOnboardingPermissions =
    [
        PermissionSystemNames.MerchantRead,
        PermissionSystemNames.MerchantCreate,
        PermissionSystemNames.MerchantUpdate,
        PermissionSystemNames.MerchantDelete,
    ];

    private static readonly string[] RoleManagementPermissions =
    [
        PermissionSystemNames.RolesRead,
        PermissionSystemNames.RolesCreate,
        PermissionSystemNames.RolesUpdate,
        PermissionSystemNames.RolesDelete,
    ];

    private static readonly string[] UserManagementPermissions =
    [
        PermissionSystemNames.UsersRead,
        PermissionSystemNames.UsersCreate,
        PermissionSystemNames.UsersUpdate,
        PermissionSystemNames.UsersDelete,
    ];

    private static readonly string[] DashboardPermissions =
    [
        PermissionSystemNames.DashboardView,
    ];

    private static readonly string[] MerchantApplicationStatusPermissions =
    [
        PermissionSystemNames.MerchantApplicationApprove,
        PermissionSystemNames.MerchantApplicationReject,
        PermissionSystemNames.MerchantApplicationHold,
        PermissionSystemNames.MerchantApplicationComplete,
    ];

    private static readonly string[] MerchantOrderStatusPermissions =
    [
        PermissionSystemNames.MerchantOrderApprove,
        PermissionSystemNames.MerchantOrderReject,
        PermissionSystemNames.MerchantOrderHold,
        PermissionSystemNames.MerchantOrderComplete,
    ];

    private static readonly string[] ExistingMerchantOrderPermissions =
    [
        PermissionSystemNames.ExistingMerchantOrderRead,
        PermissionSystemNames.ExistingMerchantOrderCreate,
        PermissionSystemNames.ExistingMerchantOrderUpdate,
        PermissionSystemNames.ExistingMerchantOrderDelete,
    ];

    private static readonly IReadOnlyDictionary<string, string[]> DepartmentPermissionMap =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Sales"] = Combine(MerchantOnboardingPermissions, ExistingMerchantOrderPermissions),
            ["Filing"] = Combine(
                MerchantOnboardingPermissions,
                MerchantApplicationStatusPermissions,
                MerchantOrderStatusPermissions,
                ExistingMerchantOrderPermissions),
            ["Terminal"] = Combine(
                MerchantOnboardingPermissions,
                MerchantOrderStatusPermissions,
                ExistingMerchantOrderPermissions),
            ["Billing"] = MerchantOnboardingPermissions,
            ["Shipping"] = Combine(
                MerchantOnboardingPermissions,
                MerchantOrderStatusPermissions,
                ExistingMerchantOrderPermissions),
            ["Support"] = MerchantOnboardingPermissions,
            ["Installation"] = Combine(MerchantOnboardingPermissions, ExistingMerchantOrderPermissions),
            ["Admin"] = Combine(
                MerchantOnboardingPermissions,
                MerchantApplicationStatusPermissions,
                MerchantOrderStatusPermissions,
                ExistingMerchantOrderPermissions,
                RoleManagementPermissions,
                UserManagementPermissions,
                DashboardPermissions),
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

        AddGroup(groups, "merchant-onboarding", "Merchant Onboarding", MerchantOnboardingPermissions, allowed);
        AddGroup(groups, "merchant-application-status", "Merchant Application Status", MerchantApplicationStatusPermissions, allowed);
        AddGroup(groups, "merchant-order-status", "Merchant Order Status", MerchantOrderStatusPermissions, allowed);
        AddGroup(groups, "existing-merchant-order", "Existing Merchant Order", ExistingMerchantOrderPermissions, allowed);
        AddGroup(groups, "users", "Users", UserManagementPermissions, allowed);
        AddGroup(groups, "roles", "Roles", RoleManagementPermissions, allowed);
        AddGroup(groups, "dashboard", "Dashboard", DashboardPermissions, allowed);

        return groups;
    }

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
