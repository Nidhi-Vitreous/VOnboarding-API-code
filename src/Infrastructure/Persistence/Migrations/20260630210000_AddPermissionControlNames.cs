using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionControlNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "control_name",
                table: "permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.Sql(
                """
                UPDATE permissions SET control_name = 'UsersRead' WHERE name = 'users.read';
                UPDATE permissions SET control_name = 'UsersCreate' WHERE name = 'users.create';
                UPDATE permissions SET control_name = 'UsersUpdate' WHERE name = 'users.update';
                UPDATE permissions SET control_name = 'UsersDelete' WHERE name = 'users.delete';
                UPDATE permissions SET control_name = 'RolesRead' WHERE name = 'roles.read';
                UPDATE permissions SET control_name = 'RolesCreate' WHERE name = 'roles.create';
                UPDATE permissions SET control_name = 'RolesUpdate' WHERE name = 'roles.update';
                UPDATE permissions SET control_name = 'RolesDelete' WHERE name = 'roles.delete';
                UPDATE permissions SET control_name = 'DashboardView' WHERE name = 'dashboard.view';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO permissions (id, name, control_name)
                VALUES
                    ('a1000001-0000-4000-8000-000000000010', 'onboarding.read', 'CanReadOnboarding'),
                    ('a1000001-0000-4000-8000-000000000011', 'onboarding.create', 'CanCreateOnboarding'),
                    ('a1000001-0000-4000-8000-000000000012', 'onboarding.refile', 'CanRefile'),
                    ('a1000001-0000-4000-8000-000000000013', 'onboarding.approve', 'CanApprove'),
                    ('a1000001-0000-4000-8000-000000000014', 'onboarding.reject', 'CanReject'),
                    ('a1000001-0000-4000-8000-000000000015', 'onboarding.hold', 'CanHold'),
                    ('a1000001-0000-4000-8000-000000000016', 'onboarding.resume', 'CanResume'),
                    ('a1000001-0000-4000-8000-000000000017', 'onboarding.block', 'CanBlock'),
                    ('a1000001-0000-4000-8000-000000000018', 'onboarding.submit', 'CanSubmit'),
                    ('a1000001-0000-4000-8000-000000000019', 'onboarding.resolve', 'CanResolve'),
                    ('a1000001-0000-4000-8000-00000000001a', 'onboarding.block.initiate', 'CanInitiateBlock')
                ON CONFLICT (name) DO NOTHING;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_control_name",
                table: "permissions",
                column: "control_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_permissions_control_name",
                table: "permissions");

            migrationBuilder.Sql(
                """
                DELETE FROM role_permissions
                WHERE permission_id IN (
                    SELECT id FROM permissions
                    WHERE name LIKE 'onboarding.%'
                );
                DELETE FROM permissions WHERE name LIKE 'onboarding.%';
                """);

            migrationBuilder.DropColumn(
                name: "control_name",
                table: "permissions");
        }
    }
}
