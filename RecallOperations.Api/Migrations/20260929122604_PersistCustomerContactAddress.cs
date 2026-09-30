using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class PersistCustomerContactAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.KS_RecallCustomers', 'Address') IS NULL
                    ALTER TABLE dbo.KS_RecallCustomers ADD Address varchar(250) NULL;
                IF COL_LENGTH('dbo.KS_RecallCustomers', 'City') IS NULL
                    ALTER TABLE dbo.KS_RecallCustomers ADD City varchar(50) NULL;
                IF COL_LENGTH('dbo.KS_RecallCustomers', 'State') IS NULL
                    ALTER TABLE dbo.KS_RecallCustomers ADD State varchar(50) NULL;
                IF COL_LENGTH('dbo.KS_RecallCustomers', 'PostalCode') IS NULL
                    ALTER TABLE dbo.KS_RecallCustomers ADD PostalCode varchar(10) NULL;
                IF COL_LENGTH('dbo.KS_RecallCustomers', 'UpdatedAt') IS NULL
                    ALTER TABLE dbo.KS_RecallCustomers ADD UpdatedAt datetime NOT NULL
                        CONSTRAINT DF_KS_RecallCustomers_UpdatedAt DEFAULT (GETDATE()) WITH VALUES;
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.default_constraints dc
                    JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('dbo.KS_RecallCustomers')
                      AND c.name = 'UpdatedAt')
                    ALTER TABLE dbo.KS_RecallCustomers
                        ADD CONSTRAINT DF_KS_RecallCustomers_UpdatedAt DEFAULT (GETDATE()) FOR UpdatedAt;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER dbo.TR_KS_RecallCustomers_UpdatedAt
                ON dbo.KS_RecallCustomers
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF TRIGGER_NESTLEVEL() > 1 RETURN;

                    UPDATE profile
                    SET UpdatedAt = GETDATE()
                    FROM dbo.KS_RecallCustomers AS profile
                    INNER JOIN inserted AS changed ON changed.CustomerId = profile.CustomerId;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
