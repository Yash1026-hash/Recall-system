using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class RecallCustomerVehiclesByEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.usp_RecallCustomers_GetVehiclesByEmail
                    @Email NVARCHAR(254)
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        vehicle.Vin,
                        vehicle.Make,
                        vehicle.Model,
                        vehicle.Year,
                        vehicle.RecallStatus
                    FROM dbo.RecallCustomers AS customer
                    INNER JOIN dbo.CustomerVehicles AS customerVehicle
                        ON customerVehicle.CustomerId = customer.CustomerId
                    INNER JOIN dbo.Vehicles AS vehicle
                        ON vehicle.Vin = customerVehicle.Vin
                    WHERE customer.Email = LTRIM(RTRIM(@Email));
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP PROCEDURE IF EXISTS dbo.usp_RecallCustomers_GetVehiclesByEmail;");
        }
    }
}
