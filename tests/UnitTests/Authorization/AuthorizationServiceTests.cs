using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Domain.Entities;
using Vitreous.Onboarding.Domain.Enums;
using DepartmentEnum = Vitreous.Onboarding.Domain.Enums.Department;

namespace Vitreous.Onboarding.UnitTests;

public class DepartmentPermissionRegistryTests
{
    [Theory]
    [InlineData("Sales Representative", "Sales Representative", DepartmentEnum.Sales)]
    [InlineData("Support", "Support", DepartmentEnum.Support)]
    [InlineData("Filing Clerk", "Filing", DepartmentEnum.Filing)]
    [InlineData("Terminal Operator", "Terminal", DepartmentEnum.Terminal)]
    [InlineData("Admin", "Admin", DepartmentEnum.Admin)]
    public void ResolveDepartment_maps_role_type_to_department(string roleName, string roleType, DepartmentEnum expected) =>
        Assert.Equal(expected, DepartmentPermissionRegistry.ResolveDepartment(roleName, roleType));

    [Fact]
    public void Sales_department_allows_create_and_refile_but_not_approve()
    {
        Assert.True(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Sales, OnboardingPermission.Create));
        Assert.True(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Sales, OnboardingPermission.Refile));
        Assert.False(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Sales, OnboardingPermission.Approve));
    }

    [Fact]
    public void Filing_department_allows_full_workflow_permissions()
    {
        Assert.True(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Filing, OnboardingPermission.Approve));
        Assert.True(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Filing, OnboardingPermission.Block));
        Assert.True(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Filing, OnboardingPermission.Resume));
    }

    [Fact]
    public void Support_department_is_read_only()
    {
        Assert.True(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Support, OnboardingPermission.Read));
        Assert.False(DepartmentPermissionRegistry.HasPermission(DepartmentEnum.Support, OnboardingPermission.Create));
    }
}

public class OnboardingStatusPermissionsTests
{
    [Theory]
    [InlineData(OnboardingRequestStatus.Submitted, null, OnboardingPermission.Submit)]
    [InlineData(OnboardingRequestStatus.Rejected, null, OnboardingPermission.Reject)]
    [InlineData(OnboardingRequestStatus.Approved, OnboardingRequestStatus.OnHold, OnboardingPermission.Resume)]
    public void GetRequiredPermission_maps_status_to_permission(
        OnboardingRequestStatus status,
        OnboardingRequestStatus? previousStatus,
        OnboardingPermission expected) =>
        Assert.Equal(expected, OnboardingStatusPermissions.GetRequiredPermission(status, previousStatus));
}

public class AuthorizationServiceTests
{
    private readonly AuthorizationService _authorizationService = new(new StubDepartmentResolver());

    [Fact]
    public async Task Admin_user_bypasses_permission_checks()
    {
        var user = new User { Id = Guid.NewGuid(), Role = "Admin", IsActive = true };

        Assert.True(await _authorizationService.CanApproveAsync(user));
        Assert.True(await _authorizationService.CanCreateOnboardingAsync(user));
        Assert.True(await _authorizationService.IsAdminAsync(user));
    }

    [Fact]
    public async Task Inactive_user_is_denied()
    {
        var user = new User { Id = Guid.NewGuid(), Role = "Admin", IsActive = false };

        Assert.False(await _authorizationService.CanApproveAsync(user));
    }

    [Fact]
    public async Task CanSetStatusAsync_checks_status_permission()
    {
        var user = new User { Id = Guid.NewGuid(), Role = "Terminal", IsActive = true };

        Assert.True(await _authorizationService.CanSetStatusAsync(user, OnboardingRequestStatus.Submitted));
        Assert.False(await _authorizationService.CanSetStatusAsync(user, OnboardingRequestStatus.Approved));
    }

    private sealed class StubDepartmentResolver : Application.Interfaces.IDepartmentResolver
    {
        public Task<AuthorizationContext> ResolveAsync(User user, CancellationToken cancellationToken = default)
        {
            var isAdmin = DepartmentPermissionRegistry.IsAdminRoleName(user.Role);
            return Task.FromResult(new AuthorizationContext
            {
                Department = isAdmin ? DepartmentEnum.Admin : DepartmentPermissionRegistry.ResolveDepartment(user.Role, user.Role),
                IsAdmin = isAdmin,
                HasAdminOverrideFlag = false,
            });
        }
    }
}
