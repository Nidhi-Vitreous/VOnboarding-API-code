using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentsToRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_departments_name",
                table: "departments",
                column: "name",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO departments (id, name)
                VALUES
                    ('b1000001-0000-4000-8000-000000000001', 'Sales'),
                    ('b1000001-0000-4000-8000-000000000002', 'Filing'),
                    ('b1000001-0000-4000-8000-000000000003', 'Terminal'),
                    ('b1000001-0000-4000-8000-000000000004', 'Billing'),
                    ('b1000001-0000-4000-8000-000000000005', 'Shipping'),
                    ('b1000001-0000-4000-8000-000000000006', 'Support'),
                    ('b1000001-0000-4000-8000-000000000007', 'Installation'),
                    ('b1000001-0000-4000-8000-000000000008', 'Admin');
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO permissions (id, name, control_name)
                VALUES
                    ('a1000001-0000-4000-8000-00000000001b', 'merchant.read', 'MerchantRead'),
                    ('a1000001-0000-4000-8000-00000000001c', 'merchant.create', 'MerchantCreate'),
                    ('a1000001-0000-4000-8000-00000000001d', 'merchant.update', 'MerchantUpdate'),
                    ('a1000001-0000-4000-8000-00000000001e', 'merchant.delete', 'MerchantDelete')
                ON CONFLICT (name) DO NOTHING;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "department_id",
                table: "roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE roles r
                SET department_id = d.id
                FROM departments d
                WHERE LOWER(r.role_type) = LOWER(d.name);

                UPDATE roles
                SET department_id = (SELECT id FROM departments WHERE name = 'Sales')
                WHERE department_id IS NULL
                  AND LOWER(role_type) IN ('sales representative', 'sales manager', 'merchant', 'merchant one');

                UPDATE roles
                SET department_id = (SELECT id FROM departments WHERE name = 'Admin')
                WHERE department_id IS NULL
                  AND LOWER(role_type) IN ('admin', 'super admin', 'administration');

                UPDATE roles
                SET department_id = (SELECT id FROM departments WHERE name = 'Support')
                WHERE department_id IS NULL
                  AND LOWER(role_type) = 'support';

                UPDATE roles
                SET department_id = (SELECT id FROM departments WHERE name = 'Admin')
                WHERE department_id IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "department_id",
                table: "roles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_department_id",
                table: "roles",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_roles_departments_department_id",
                table: "roles",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_roles_departments_department_id",
                table: "roles");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropIndex(
                name: "IX_roles_department_id",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "roles");

            migrationBuilder.Sql(
                """
                DELETE FROM role_permissions
                WHERE permission_id IN (
                    SELECT id FROM permissions WHERE name LIKE 'merchant.%'
                );
                DELETE FROM permissions WHERE name LIKE 'merchant.%';
                """);
        }
    }
}
