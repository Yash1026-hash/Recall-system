using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerVehicles_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "CustomerVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerVehicles_Vehicles_Vin",
                schema: "dbo",
                table: "CustomerVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_RecallUsers_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "RecallUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vehicles",
                schema: "dbo",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecallUsers",
                schema: "dbo",
                table: "RecallUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecallCustomers",
                schema: "dbo",
                table: "RecallCustomers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerVehicles",
                schema: "dbo",
                table: "CustomerVehicles");

            migrationBuilder.RenameTable(
                name: "Vehicles",
                schema: "dbo",
                newName: "KS_Vehicles",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RecallUsers",
                schema: "dbo",
                newName: "KS_RecallUsers",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RecallCustomers",
                schema: "dbo",
                newName: "KS_RecallCustomers",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CustomerVehicles",
                schema: "dbo",
                newName: "KS_CustomerVehicles",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_RecallUsers_Username",
                schema: "dbo",
                table: "KS_RecallUsers",
                newName: "IX_KS_RecallUsers_Username");

            migrationBuilder.RenameIndex(
                name: "IX_RecallUsers_CustomerId",
                schema: "dbo",
                table: "KS_RecallUsers",
                newName: "IX_KS_RecallUsers_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_RecallCustomers_Email",
                schema: "dbo",
                table: "KS_RecallCustomers",
                newName: "IX_KS_RecallCustomers_Email");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerVehicles_Vin",
                schema: "dbo",
                table: "KS_CustomerVehicles",
                newName: "IX_KS_CustomerVehicles_Vin");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KS_Vehicles",
                schema: "dbo",
                table: "KS_Vehicles",
                column: "Vin");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KS_RecallUsers",
                schema: "dbo",
                table: "KS_RecallUsers",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KS_RecallCustomers",
                schema: "dbo",
                table: "KS_RecallCustomers",
                column: "CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KS_CustomerVehicles",
                schema: "dbo",
                table: "KS_CustomerVehicles",
                columns: new[] { "CustomerId", "Vin" });

            migrationBuilder.AddForeignKey(
                name: "FK_KS_CustomerVehicles_KS_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "KS_CustomerVehicles",
                column: "CustomerId",
                principalSchema: "dbo",
                principalTable: "KS_RecallCustomers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KS_CustomerVehicles_KS_Vehicles_Vin",
                schema: "dbo",
                table: "KS_CustomerVehicles",
                column: "Vin",
                principalSchema: "dbo",
                principalTable: "KS_Vehicles",
                principalColumn: "Vin",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KS_RecallUsers_KS_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "KS_RecallUsers",
                column: "CustomerId",
                principalSchema: "dbo",
                principalTable: "KS_RecallCustomers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.usp_Vehicles_GetAll
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT Vin, Make, Model, Year, RecallStatus
                    FROM dbo.KS_Vehicles
                    ORDER BY Vin;
                END;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.usp_Vehicles_GetByVin
                    @Vin NVARCHAR(17)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT Vin, Make, Model, Year, RecallStatus
                    FROM dbo.KS_Vehicles
                    WHERE Vin = @Vin;
                END;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.usp_RecallCustomers_GetVehiclesByEmail
                    @Email NVARCHAR(254)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT vehicle.Vin, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.RecallStatus
                    FROM dbo.KS_RecallCustomers AS customer
                    INNER JOIN dbo.KS_CustomerVehicles AS customerVehicle
                        ON customerVehicle.CustomerId = customer.CustomerId
                    INNER JOIN dbo.KS_Vehicles AS vehicle
                        ON vehicle.Vin = customerVehicle.Vin
                    WHERE customer.Email = LTRIM(RTRIM(@Email));
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KS_CustomerVehicles_KS_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "KS_CustomerVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_KS_CustomerVehicles_KS_Vehicles_Vin",
                schema: "dbo",
                table: "KS_CustomerVehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_KS_RecallUsers_KS_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "KS_RecallUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KS_Vehicles",
                schema: "dbo",
                table: "KS_Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KS_RecallUsers",
                schema: "dbo",
                table: "KS_RecallUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KS_RecallCustomers",
                schema: "dbo",
                table: "KS_RecallCustomers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KS_CustomerVehicles",
                schema: "dbo",
                table: "KS_CustomerVehicles");

            migrationBuilder.RenameTable(
                name: "KS_Vehicles",
                schema: "dbo",
                newName: "Vehicles",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "KS_RecallUsers",
                schema: "dbo",
                newName: "RecallUsers",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "KS_RecallCustomers",
                schema: "dbo",
                newName: "RecallCustomers",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "KS_CustomerVehicles",
                schema: "dbo",
                newName: "CustomerVehicles",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_KS_RecallUsers_Username",
                schema: "dbo",
                table: "RecallUsers",
                newName: "IX_RecallUsers_Username");

            migrationBuilder.RenameIndex(
                name: "IX_KS_RecallUsers_CustomerId",
                schema: "dbo",
                table: "RecallUsers",
                newName: "IX_RecallUsers_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_KS_RecallCustomers_Email",
                schema: "dbo",
                table: "RecallCustomers",
                newName: "IX_RecallCustomers_Email");

            migrationBuilder.RenameIndex(
                name: "IX_KS_CustomerVehicles_Vin",
                schema: "dbo",
                table: "CustomerVehicles",
                newName: "IX_CustomerVehicles_Vin");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vehicles",
                schema: "dbo",
                table: "Vehicles",
                column: "Vin");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecallUsers",
                schema: "dbo",
                table: "RecallUsers",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecallCustomers",
                schema: "dbo",
                table: "RecallCustomers",
                column: "CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerVehicles",
                schema: "dbo",
                table: "CustomerVehicles",
                columns: new[] { "CustomerId", "Vin" });

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerVehicles_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "CustomerVehicles",
                column: "CustomerId",
                principalSchema: "dbo",
                principalTable: "RecallCustomers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerVehicles_Vehicles_Vin",
                schema: "dbo",
                table: "CustomerVehicles",
                column: "Vin",
                principalSchema: "dbo",
                principalTable: "Vehicles",
                principalColumn: "Vin",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecallUsers_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "RecallUsers",
                column: "CustomerId",
                principalSchema: "dbo",
                principalTable: "RecallCustomers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.usp_Vehicles_GetAll
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT Vin, Make, Model, Year, RecallStatus
                    FROM dbo.Vehicles
                    ORDER BY Vin;
                END;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.usp_Vehicles_GetByVin
                    @Vin NVARCHAR(17)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT Vin, Make, Model, Year, RecallStatus
                    FROM dbo.Vehicles
                    WHERE Vin = @Vin;
                END;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.usp_RecallCustomers_GetVehiclesByEmail
                    @Email NVARCHAR(254)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT vehicle.Vin, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.RecallStatus
                    FROM dbo.RecallCustomers AS customer
                    INNER JOIN dbo.CustomerVehicles AS customerVehicle
                        ON customerVehicle.CustomerId = customer.CustomerId
                    INNER JOIN dbo.Vehicles AS vehicle
                        ON vehicle.Vin = customerVehicle.Vin
                    WHERE customer.Email = LTRIM(RTRIM(@Email));
                END;
                """);
        }
    }
}
