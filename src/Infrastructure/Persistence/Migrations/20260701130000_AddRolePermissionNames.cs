using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissionNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role_name",
                table: "role_permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "permission_name",
                table: "role_permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE role_permissions rp
                SET role_name = r.name
                FROM roles r
                WHERE rp.role_id = r.id;
                """);

            migrationBuilder.Sql(
                """
                UPDATE role_permissions rp
                SET permission_name = p.name
                FROM permissions p
                WHERE rp.permission_id = p.id;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "role_name",
                table: "role_permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "permission_name",
                table: "role_permissions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role_name",
                table: "role_permissions");

            migrationBuilder.DropColumn(
                name: "permission_name",
                table: "role_permissions");
        }
    }
}
