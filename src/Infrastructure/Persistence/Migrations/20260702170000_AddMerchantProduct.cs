using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE merchants
                    ADD COLUMN IF NOT EXISTS product character varying(128);

                CREATE INDEX IF NOT EXISTS "IX_merchants_product"
                    ON merchants (product);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_merchants_product";

                ALTER TABLE merchants
                    DROP COLUMN IF EXISTS product;
                """);
        }
    }
}
