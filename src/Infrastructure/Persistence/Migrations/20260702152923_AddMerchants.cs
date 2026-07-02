using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vitreous.Onboarding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS audit_log (
                    id uuid NOT NULL,
                    entity_type character varying(64) NOT NULL,
                    entity_id uuid NOT NULL,
                    action character varying(64) NOT NULL,
                    old_value jsonb,
                    new_value jsonb,
                    created_at timestamp with time zone NOT NULL,
                    user_id uuid NOT NULL,
                    CONSTRAINT "PK_audit_log" PRIMARY KEY (id)
                );

                DO $migrate_legacy_merchants$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'merchants'
                          AND column_name = 'Id')
                    THEN
                        ALTER TABLE merchants RENAME COLUMN "Id" TO id;
                        ALTER TABLE merchants RENAME COLUMN "BusinessName" TO merchant_name;
                        ALTER TABLE merchants RENAME COLUMN "ContactName" TO legal_name;
                        ALTER TABLE merchants RENAME COLUMN "Email" TO email;
                        ALTER TABLE merchants RENAME COLUMN "Phone" TO phone;
                        ALTER TABLE merchants RENAME COLUMN "Address" TO address;
                        ALTER TABLE merchants RENAME COLUMN "Status" TO status;
                        ALTER TABLE merchants RENAME COLUMN "CreatedAt" TO created_at;
                        ALTER TABLE merchants RENAME COLUMN "UpdatedAt" TO updated_at;

                        UPDATE merchants
                        SET address = TRIM(BOTH ', ' FROM CONCAT_WS(
                            ', ',
                            NULLIF(address, ''),
                            NULLIF("City", ''),
                            NULLIF("State", ''),
                            NULLIF("ZipCode", '')))
                        WHERE "City" IS NOT NULL
                           OR "State" IS NOT NULL
                           OR "ZipCode" IS NOT NULL;

                        ALTER TABLE merchants DROP COLUMN IF EXISTS "City";
                        ALTER TABLE merchants DROP COLUMN IF EXISTS "State";
                        ALTER TABLE merchants DROP COLUMN IF EXISTS "ZipCode";
                    END IF;
                END $migrate_legacy_merchants$;

                CREATE TABLE IF NOT EXISTS merchants (
                    id uuid NOT NULL,
                    merchant_name character varying(256) NOT NULL,
                    legal_name character varying(256),
                    email character varying(256),
                    phone character varying(32),
                    address character varying(512),
                    notes character varying(2000),
                    status character varying(64) NOT NULL,
                    role character varying(128) NOT NULL,
                    created_by uuid NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_merchants" PRIMARY KEY (id)
                );

                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS merchant_name character varying(256);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS legal_name character varying(256);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS email character varying(256);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS phone character varying(32);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS address character varying(512);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS notes character varying(2000);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS status character varying(64);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS role character varying(128);
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS created_by uuid;
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS created_at timestamp with time zone;
                ALTER TABLE merchants ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone;

                UPDATE merchants
                SET merchant_name = COALESCE(merchant_name, 'Unknown Merchant')
                WHERE merchant_name IS NULL;

                UPDATE merchants
                SET status = COALESCE(status, 'Draft')
                WHERE status IS NULL;

                UPDATE merchants
                SET role = COALESCE(role, 'Sales')
                WHERE role IS NULL;

                UPDATE merchants
                SET created_by = COALESCE(
                    created_by,
                    (SELECT id FROM users WHERE LOWER(username) = 'admin' LIMIT 1),
                    '00000000-0000-0000-0000-000000000001'::uuid)
                WHERE created_by IS NULL;

                UPDATE merchants
                SET created_at = COALESCE(created_at, NOW() AT TIME ZONE 'UTC')
                WHERE created_at IS NULL;

                UPDATE merchants
                SET updated_at = COALESCE(updated_at, NOW() AT TIME ZONE 'UTC')
                WHERE updated_at IS NULL;

                ALTER TABLE merchants ALTER COLUMN merchant_name SET NOT NULL;
                ALTER TABLE merchants ALTER COLUMN status SET NOT NULL;
                ALTER TABLE merchants ALTER COLUMN role SET NOT NULL;
                ALTER TABLE merchants ALTER COLUMN created_by SET NOT NULL;
                ALTER TABLE merchants ALTER COLUMN created_at SET NOT NULL;
                ALTER TABLE merchants ALTER COLUMN updated_at SET NOT NULL;

                CREATE TABLE IF NOT EXISTS merchant_status_history (
                    id uuid NOT NULL,
                    merchant_id uuid NOT NULL,
                    old_status character varying(64),
                    new_status character varying(64) NOT NULL,
                    changed_by uuid NOT NULL,
                    changed_at timestamp with time zone NOT NULL,
                    comment character varying(1000),
                    CONSTRAINT "PK_merchant_status_history" PRIMARY KEY (id)
                );

                DO $add_merchant_fk$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'merchants'
                          AND column_name = 'id')
                    AND NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_merchant_status_history_merchants_merchant_id')
                    THEN
                        ALTER TABLE merchant_status_history
                            ADD CONSTRAINT "FK_merchant_status_history_merchants_merchant_id"
                            FOREIGN KEY (merchant_id)
                            REFERENCES merchants (id)
                            ON DELETE CASCADE;
                    END IF;
                END $add_merchant_fk$;

                CREATE INDEX IF NOT EXISTS "IX_audit_log_entity_type_entity_id"
                    ON audit_log (entity_type, entity_id);

                CREATE INDEX IF NOT EXISTS "IX_merchant_status_history_merchant_id"
                    ON merchant_status_history (merchant_id);

                CREATE INDEX IF NOT EXISTS "IX_merchants_merchant_name"
                    ON merchants (merchant_name);

                CREATE INDEX IF NOT EXISTS "IX_merchants_role"
                    ON merchants (role);

                CREATE INDEX IF NOT EXISTS "IX_merchants_status"
                    ON merchants (status);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "merchant_status_history");

            migrationBuilder.DropTable(
                name: "merchants");
        }
    }
}
