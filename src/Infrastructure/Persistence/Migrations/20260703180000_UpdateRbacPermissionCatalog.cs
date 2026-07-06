using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdateRbacPermissionCatalog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE permissions SET
                name = CASE system_name
                    WHEN 'merchant.read' THEN 'View'
                    WHEN 'merchant.create' THEN 'Create'
                    WHEN 'merchant.update' THEN 'Update'
                    WHEN 'merchant.delete' THEN 'Delete'
                    WHEN 'merchant.application.approve' THEN 'Approve'
                    WHEN 'merchant.application.reject' THEN 'Reject'
                    WHEN 'merchant.application.hold' THEN 'On Hold'
                    WHEN 'merchant.application.complete' THEN 'Completed'
                    WHEN 'merchant.order.approve' THEN 'Approve'
                    WHEN 'merchant.order.reject' THEN 'Reject'
                    WHEN 'merchant.order.hold' THEN 'On Hold'
                    WHEN 'merchant.order.complete' THEN 'Completed'
                    ELSE name
                END,
                description = CASE system_name
                    WHEN 'merchant.read' THEN 'Users with this permission can view merchant onboarding records.'
                    WHEN 'merchant.create' THEN 'Users with this permission can create merchant onboarding records.'
                    WHEN 'merchant.update' THEN 'Users with this permission can update merchant onboarding records.'
                    WHEN 'merchant.delete' THEN 'Users with this permission can delete merchant onboarding records.'
                    WHEN 'merchant.application.approve' THEN 'Users with this permission can approve merchant application statuses.'
                    WHEN 'merchant.application.reject' THEN 'Users with this permission can reject merchant application statuses.'
                    WHEN 'merchant.application.hold' THEN 'Users with this permission can place merchant application statuses on hold.'
                    WHEN 'merchant.application.complete' THEN 'Users with this permission can mark merchant application statuses as completed.'
                    WHEN 'merchant.order.approve' THEN 'Users with this permission can approve merchant order statuses.'
                    WHEN 'merchant.order.reject' THEN 'Users with this permission can reject merchant order statuses.'
                    WHEN 'merchant.order.hold' THEN 'Users with this permission can place merchant order statuses on hold.'
                    WHEN 'merchant.order.complete' THEN 'Users with this permission can mark merchant order statuses as completed.'
                    ELSE description
                END
            WHERE system_name IN (
                'merchant.read', 'merchant.create', 'merchant.update', 'merchant.delete',
                'merchant.application.approve', 'merchant.application.reject',
                'merchant.application.hold', 'merchant.application.complete',
                'merchant.order.approve', 'merchant.order.reject',
                'merchant.order.hold', 'merchant.order.complete'
            );
            """);

        migrationBuilder.Sql(
            """
            INSERT INTO permissions (id, system_name, name, description)
            VALUES
                ('a1000001-0000-4000-8000-000000000027', 'existing.merchant.order.read', 'View',
                    'Users with this permission can view existing merchant orders.'),
                ('a1000001-0000-4000-8000-000000000028', 'existing.merchant.order.create', 'Create',
                    'Users with this permission can create existing merchant orders.'),
                ('a1000001-0000-4000-8000-000000000029', 'existing.merchant.order.update', 'Update',
                    'Users with this permission can update existing merchant orders.'),
                ('a1000001-0000-4000-8000-00000000002a', 'existing.merchant.order.delete', 'Delete',
                    'Users with this permission can delete existing merchant orders.')
            ON CONFLICT (system_name) DO UPDATE SET
                name = EXCLUDED.name,
                description = EXCLUDED.description;
            """);

        migrationBuilder.Sql(
            """
            DELETE FROM role_permissions
            WHERE permission_id IN (
                SELECT id FROM permissions WHERE system_name LIKE 'onboarding.%'
            );
            """);

        migrationBuilder.Sql(
            """
            DELETE FROM permissions WHERE system_name LIKE 'onboarding.%';
            """);

        migrationBuilder.Sql(
            """
            UPDATE role_permissions rp
            SET permission_name = p.name
            FROM permissions p
            WHERE rp.permission_id = p.id;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM role_permissions
            WHERE permission_id IN (
                SELECT id FROM permissions WHERE system_name LIKE 'existing.merchant.order.%'
            );
            """);

        migrationBuilder.Sql(
            """
            DELETE FROM permissions WHERE system_name LIKE 'existing.merchant.order.%';
            """);

        migrationBuilder.Sql(
            """
            INSERT INTO permissions (id, system_name, name, description)
            VALUES
                ('a1000001-0000-4000-8000-000000000010', 'onboarding.read', 'Read onboarding',
                    'Users with this permission can view onboarding requests.'),
                ('a1000001-0000-4000-8000-000000000011', 'onboarding.create', 'Create onboarding',
                    'Users with this permission can create onboarding requests.'),
                ('a1000001-0000-4000-8000-000000000012', 'onboarding.refile', 'Refile onboarding',
                    'Users with this permission can refile onboarding requests.'),
                ('a1000001-0000-4000-8000-000000000013', 'onboarding.approve', 'Approve onboarding',
                    'Users with this permission can approve onboarding requests.'),
                ('a1000001-0000-4000-8000-000000000014', 'onboarding.reject', 'Reject onboarding',
                    'Users with this permission can reject onboarding requests.'),
                ('a1000001-0000-4000-8000-000000000015', 'onboarding.hold', 'Hold onboarding',
                    'Users with this permission can place onboarding requests on hold.'),
                ('a1000001-0000-4000-8000-000000000016', 'onboarding.resume', 'Resume onboarding',
                    'Users with this permission can resume onboarding requests from hold.'),
                ('a1000001-0000-4000-8000-000000000017', 'onboarding.block', 'Block onboarding',
                    'Users with this permission can block onboarding requests.'),
                ('a1000001-0000-4000-8000-000000000018', 'onboarding.submit', 'Submit onboarding',
                    'Users with this permission can submit onboarding requests.'),
                ('a1000001-0000-4000-8000-000000000019', 'onboarding.resolve', 'Resolve onboarding',
                    'Users with this permission can resolve onboarding requests.'),
                ('a1000001-0000-4000-8000-00000000001a', 'onboarding.block.initiate', 'Initiate onboarding block',
                    'Users with this permission can initiate a block on onboarding requests.')
            ON CONFLICT (system_name) DO NOTHING;
            """);
    }
}
