using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleListFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_system_role",
                table: "roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "role_type",
                table: "roles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_system_role",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "role_type",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "roles");
        }
    }
}
