using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Roles;
using Vitreous.Onboarding.Domain.Enums;
using DepartmentEnum = Vitreous.Onboarding.Domain.Enums.Department;

namespace Vitreous.Onboarding.UnitTests;

public class DepartmentRolePermissionCatalogTests
{
    [Fact]
    public void Filing_department_includes_merchant_application_and_order_permissions()
    {
        var allowed = DepartmentRolePermissionCatalog.GetAllowedSystemNames("Filing");

        Assert.Contains(PermissionSystemNames.MerchantApplicationApprove, allowed);
        Assert.Contains(PermissionSystemNames.MerchantApplicationReject, allowed);
        Assert.Contains(PermissionSystemNames.MerchantOrderApprove, allowed);
        Assert.Contains(PermissionSystemNames.MerchantOrderReject, allowed);
    }

    [Theory]
    [InlineData("Terminal")]
    [InlineData("Shipping")]
    public void Terminal_and_shipping_departments_include_merchant_order_permissions_only(string departmentName)
    {
        var allowed = DepartmentRolePermissionCatalog.GetAllowedSystemNames(departmentName);

        Assert.DoesNotContain(PermissionSystemNames.MerchantApplicationApprove, allowed);
        Assert.DoesNotContain(PermissionSystemNames.MerchantApplicationReject, allowed);
        Assert.Contains(PermissionSystemNames.MerchantOrderApprove, allowed);
        Assert.Contains(PermissionSystemNames.MerchantOrderReject, allowed);
    }

    [Fact]
    public void Sales_department_does_not_include_merchant_application_or_order_permissions()
    {
        var allowed = DepartmentRolePermissionCatalog.GetAllowedSystemNames("Sales");

        Assert.DoesNotContain(PermissionSystemNames.MerchantApplicationApprove, allowed);
        Assert.DoesNotContain(PermissionSystemNames.MerchantApplicationReject, allowed);
        Assert.DoesNotContain(PermissionSystemNames.MerchantOrderApprove, allowed);
        Assert.DoesNotContain(PermissionSystemNames.MerchantOrderReject, allowed);
    }

    [Fact]
    public void Filing_permission_groups_include_merchant_application_and_order_sections()
    {
        var groups = DepartmentRolePermissionCatalog.GetPermissionGroups("Filing");
        var groupKeys = groups.Select(group => group.Key).ToList();

        Assert.Contains("merchant-application", groupKeys);
        Assert.Contains("merchant-order", groupKeys);

        var applicationNames = groups
            .Single(group => group.Key == "merchant-application")
            .Permissions
            .Select(permission => permission.SystemName);
        var orderNames = groups
            .Single(group => group.Key == "merchant-order")
            .Permissions
            .Select(permission => permission.SystemName);

        Assert.Equal(
            [
                PermissionSystemNames.MerchantApplicationApprove,
                PermissionSystemNames.MerchantApplicationReject,
                PermissionSystemNames.MerchantApplicationHold,
                PermissionSystemNames.MerchantApplicationComplete,
            ],
            applicationNames);
        Assert.Equal(
            [
                PermissionSystemNames.MerchantOrderApprove,
                PermissionSystemNames.MerchantOrderReject,
                PermissionSystemNames.MerchantOrderHold,
                PermissionSystemNames.MerchantOrderComplete,
            ],
            orderNames);
    }
}

public class MerchantWorkflowPermissionRegistryTests
{
    [Fact]
    public void Filing_department_allows_merchant_application_and_order_permissions()
    {
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            DepartmentEnum.Filing,
            MerchantWorkflowPermission.ApproveApplication));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            DepartmentEnum.Filing,
            MerchantWorkflowPermission.RejectApplication));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            DepartmentEnum.Filing,
            MerchantWorkflowPermission.ApproveOrder));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            DepartmentEnum.Filing,
            MerchantWorkflowPermission.RejectOrder));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            DepartmentEnum.Filing,
            MerchantWorkflowPermission.HoldApplication));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            DepartmentEnum.Filing,
            MerchantWorkflowPermission.CompleteOrder));
    }

    [Theory]
    [InlineData(DepartmentEnum.Terminal)]
    [InlineData(DepartmentEnum.Shipping)]
    public void Terminal_and_shipping_allow_merchant_order_permissions_only(DepartmentEnum department)
    {
        Assert.False(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            department,
            MerchantWorkflowPermission.ApproveApplication));
        Assert.False(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            department,
            MerchantWorkflowPermission.RejectApplication));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            department,
            MerchantWorkflowPermission.ApproveOrder));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            department,
            MerchantWorkflowPermission.RejectOrder));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            department,
            MerchantWorkflowPermission.HoldOrder));
        Assert.True(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            department,
            MerchantWorkflowPermission.CompleteOrder));
    }

    [Fact]
    public void Sales_department_does_not_allow_merchant_workflow_permissions()
    {
        Assert.False(DepartmentPermissionRegistry.HasMerchantWorkflowPermission(
            DepartmentEnum.Sales,
            MerchantWorkflowPermission.ApproveOrder));
    }
}
