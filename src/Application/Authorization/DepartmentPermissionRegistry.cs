using Vitreous.Onboarding.Domain.Enums;

namespace Vitreous.Onboarding.Application.Authorization;

/// <summary>
/// RBAC matrix mapping departments to onboarding permissions.
/// </summary>
public static class DepartmentPermissionRegistry
{
    private static readonly IReadOnlyDictionary<OnboardingPermission, string> OnboardingPermissionNames =
        new Dictionary<OnboardingPermission, string>
        {
            [OnboardingPermission.Read] = "onboarding.read",
            [OnboardingPermission.Create] = "onboarding.create",
            [OnboardingPermission.Refile] = "onboarding.refile",
            [OnboardingPermission.Approve] = "onboarding.approve",
            [OnboardingPermission.Reject] = "onboarding.reject",
            [OnboardingPermission.Hold] = "onboarding.hold",
            [OnboardingPermission.Resume] = "onboarding.resume",
            [OnboardingPermission.Block] = "onboarding.block",
            [OnboardingPermission.Submit] = "onboarding.submit",
            [OnboardingPermission.Resolve] = "onboarding.resolve",
            [OnboardingPermission.InitiateBlock] = "onboarding.block.initiate",
        };

    private static readonly IReadOnlyDictionary<Department, HashSet<OnboardingPermission>> PermissionMap =
        new Dictionary<Department, HashSet<OnboardingPermission>>
        {
            [Department.Sales] =
            [
                OnboardingPermission.Read,
                OnboardingPermission.Create,
                OnboardingPermission.Refile,
            ],
            [Department.Filing] =
            [
                OnboardingPermission.Read,
                OnboardingPermission.Create,
                OnboardingPermission.Refile,
                OnboardingPermission.Approve,
                OnboardingPermission.Reject,
                OnboardingPermission.Hold,
                OnboardingPermission.Resume,
                OnboardingPermission.Block,
            ],
            [Department.Terminal] =
            [
                OnboardingPermission.Read,
                OnboardingPermission.Submit,
                OnboardingPermission.InitiateBlock,
            ],
            [Department.Billing] =
            [
                OnboardingPermission.Read,
                OnboardingPermission.Resolve,
                OnboardingPermission.Reject,
            ],
            [Department.Shipping] = [OnboardingPermission.Read],
            [Department.Support] = [OnboardingPermission.Read],
            [Department.Installation] = [OnboardingPermission.Read],
            [Department.Admin] = Enum.GetValues<OnboardingPermission>().ToHashSet(),
        };

    private static readonly IReadOnlyDictionary<string, Department> RoleTypeDepartmentMap =
        new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase)
        {
            ["Sales"] = Department.Sales,
            ["Sales Representative"] = Department.Sales,
            ["Sales Manager"] = Department.Sales,
            ["Filing"] = Department.Filing,
            ["Terminal"] = Department.Terminal,
            ["Billing"] = Department.Billing,
            ["Shipping"] = Department.Shipping,
            ["Support"] = Department.Support,
            ["Installation"] = Department.Installation,
            ["Admin"] = Department.Admin,
            ["Super Admin"] = Department.Admin,
            ["Administration"] = Department.Admin,
        };

    private static readonly HashSet<string> AdminRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "SUPER ADMIN",
        "Super Admin",
    };

    public static Department ResolveDepartmentByName(string? departmentName)
    {
        if (string.IsNullOrWhiteSpace(departmentName))
        {
            return Department.Unknown;
        }

        return RoleTypeDepartmentMap.TryGetValue(departmentName.Trim(), out var department)
            ? department
            : Department.Unknown;
    }

    public static Department ResolveDepartment(string? roleName, string? roleType)
    {
        if (!string.IsNullOrWhiteSpace(roleType)
            && RoleTypeDepartmentMap.TryGetValue(roleType.Trim(), out var departmentFromType))
        {
            return departmentFromType;
        }

        if (!string.IsNullOrWhiteSpace(roleName)
            && RoleTypeDepartmentMap.TryGetValue(roleName.Trim(), out var departmentFromRoleName))
        {
            return departmentFromRoleName;
        }

        return Department.Unknown;
    }

    public static bool IsAdminRoleName(string? roleName) =>
        !string.IsNullOrWhiteSpace(roleName) && AdminRoleNames.Contains(roleName.Trim());

    public static bool IsAdminDepartment(Department department) => department == Department.Admin;

    public static bool HasPermission(Department department, OnboardingPermission permission)
    {
        if (department == Department.Admin)
        {
            return true;
        }

        return PermissionMap.TryGetValue(department, out var permissions) && permissions.Contains(permission);
    }

    public static IReadOnlyList<string> GetAllOnboardingPermissionNames() =>
        Enum.GetValues<OnboardingPermission>()
            .Select(permission => OnboardingPermissionNames[permission])
            .ToList();

    public static IReadOnlyList<string> GetOnboardingPermissionNames(Department department)
    {
        if (department == Department.Admin)
        {
            return GetAllOnboardingPermissionNames();
        }

        if (!PermissionMap.TryGetValue(department, out var permissions))
        {
            return [];
        }

        return permissions
            .Select(permission => OnboardingPermissionNames[permission])
            .ToList();
    }

    public static IReadOnlyList<string> GetOnboardingPermissionNamesForDepartmentName(string departmentName) =>
        GetOnboardingPermissionNames(ResolveDepartmentByName(departmentName));
}
