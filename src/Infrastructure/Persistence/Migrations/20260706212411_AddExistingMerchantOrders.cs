using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExistingMerchantOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "existing_merchant_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    product = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_existing_merchant_orders", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_existing_merchant_orders_merchant_name",
                table: "existing_merchant_orders",
                column: "merchant_name");

            migrationBuilder.CreateIndex(
                name: "IX_existing_merchant_orders_status",
                table: "existing_merchant_orders",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "existing_merchant_orders");
        }
    }
}
