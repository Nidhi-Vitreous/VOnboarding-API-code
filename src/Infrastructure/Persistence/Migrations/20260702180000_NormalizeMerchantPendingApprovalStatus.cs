using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeMerchantPendingApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE merchants
                SET status = 'Pending Approval'
                WHERE lower(status) IN ('validating', 'waiting review', 'pending');

                UPDATE merchant_status_history
                SET old_status = 'Pending Approval'
                WHERE old_status IS NOT NULL
                  AND lower(old_status) IN ('validating', 'waiting review', 'pending');

                UPDATE merchant_status_history
                SET new_status = 'Pending Approval'
                WHERE lower(new_status) IN ('validating', 'waiting review', 'pending');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE merchants
                SET status = 'Validating'
                WHERE status = 'Pending Approval';

                UPDATE merchant_status_history
                SET old_status = 'Validating'
                WHERE old_status = 'Pending Approval';

                UPDATE merchant_status_history
                SET new_status = 'Validating'
                WHERE new_status = 'Pending Approval';
                """);
        }
    }
}
