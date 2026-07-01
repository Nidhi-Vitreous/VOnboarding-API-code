using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionSystemNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_permissions_control_name",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "IX_permissions_name",
                table: "permissions");

            migrationBuilder.AddColumn<string>(
                name: "system_name",
                table: "permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql("UPDATE permissions SET system_name = name;");

            migrationBuilder.AlterColumn<string>(
                name: "system_name",
                table: "permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "control_name",
                table: "permissions",
                newName: "description");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "permissions",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: false);

            migrationBuilder.Sql(
                """
                UPDATE permissions SET
                    name = CASE system_name
                        WHEN 'users.read' THEN 'Read users'
                        WHEN 'users.create' THEN 'Create users'
                        WHEN 'users.update' THEN 'Update users'
                        WHEN 'users.delete' THEN 'Delete users'
                        WHEN 'roles.read' THEN 'Read roles'
                        WHEN 'roles.create' THEN 'Create roles'
                        WHEN 'roles.update' THEN 'Update roles'
                        WHEN 'roles.delete' THEN 'Delete roles'
                        WHEN 'merchant.read' THEN 'Read merchants'
                        WHEN 'merchant.create' THEN 'Create merchants'
                        WHEN 'merchant.update' THEN 'Update merchants'
                        WHEN 'merchant.delete' THEN 'Delete merchants'
                        WHEN 'dashboard.view' THEN 'View dashboard'
                        WHEN 'onboarding.read' THEN 'Read onboarding'
                        WHEN 'onboarding.create' THEN 'Create onboarding'
                        WHEN 'onboarding.refile' THEN 'Refile onboarding'
                        WHEN 'onboarding.approve' THEN 'Approve onboarding'
                        WHEN 'onboarding.reject' THEN 'Reject onboarding'
                        WHEN 'onboarding.hold' THEN 'Hold onboarding'
                        WHEN 'onboarding.resume' THEN 'Resume onboarding'
                        WHEN 'onboarding.block' THEN 'Block onboarding'
                        WHEN 'onboarding.submit' THEN 'Submit onboarding'
                        WHEN 'onboarding.resolve' THEN 'Resolve onboarding'
                        WHEN 'onboarding.block.initiate' THEN 'Initiate onboarding block'
                        ELSE name
                    END,
                    description = CASE system_name
                        WHEN 'users.read' THEN 'Users with this permission can view user records.'
                        WHEN 'users.create' THEN 'Users with this permission can create new user accounts.'
                        WHEN 'users.update' THEN 'Users with this permission can update existing user records.'
                        WHEN 'users.delete' THEN 'Users with this permission can delete user accounts.'
                        WHEN 'roles.read' THEN 'Users with this permission can view role definitions.'
                        WHEN 'roles.create' THEN 'Users with this permission can create new roles.'
                        WHEN 'roles.update' THEN 'Users with this permission can update existing roles.'
                        WHEN 'roles.delete' THEN 'Users with this permission can delete roles.'
                        WHEN 'merchant.read' THEN 'Users with this permission can view merchant records.'
                        WHEN 'merchant.create' THEN 'Users with this permission can create merchant records.'
                        WHEN 'merchant.update' THEN 'Users with this permission can update merchant records.'
                        WHEN 'merchant.delete' THEN 'Users with this permission can delete merchant records.'
                        WHEN 'dashboard.view' THEN 'Users with this permission can access the dashboard.'
                        WHEN 'onboarding.read' THEN 'Users with this permission can view onboarding requests.'
                        WHEN 'onboarding.create' THEN 'Users with this permission can create onboarding requests.'
                        WHEN 'onboarding.refile' THEN 'Users with this permission can refile onboarding requests.'
                        WHEN 'onboarding.approve' THEN 'Users with this permission can approve onboarding requests.'
                        WHEN 'onboarding.reject' THEN 'Users with this permission can reject onboarding requests.'
                        WHEN 'onboarding.hold' THEN 'Users with this permission can place onboarding requests on hold.'
                        WHEN 'onboarding.resume' THEN 'Users with this permission can resume onboarding requests from hold.'
                        WHEN 'onboarding.block' THEN 'Users with this permission can block onboarding requests.'
                        WHEN 'onboarding.submit' THEN 'Users with this permission can submit onboarding requests.'
                        WHEN 'onboarding.resolve' THEN 'Users with this permission can resolve onboarding requests.'
                        WHEN 'onboarding.block.initiate' THEN 'Users with this permission can initiate a block on onboarding requests.'
                        ELSE COALESCE(description, '')
                    END;
                """);

            migrationBuilder.Sql("UPDATE permissions SET description = '' WHERE description IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "permissions",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_system_name",
                table: "permissions",
                column: "system_name",
                unique: true);

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
                UPDATE role_permissions rp
                SET permission_name = p.system_name
                FROM permissions p
                WHERE rp.permission_id = p.id;
                """);

            migrationBuilder.DropIndex(
                name: "IX_permissions_system_name",
                table: "permissions");

            migrationBuilder.Sql("UPDATE permissions SET name = system_name;");

            migrationBuilder.DropColumn(
                name: "system_name",
                table: "permissions");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "permissions",
                newName: "control_name");

            migrationBuilder.AlterColumn<string>(
                name: "control_name",
                table: "permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_control_name",
                table: "permissions",
                column: "control_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_name",
                table: "permissions",
                column: "name",
                unique: true);
        }
    }
}
