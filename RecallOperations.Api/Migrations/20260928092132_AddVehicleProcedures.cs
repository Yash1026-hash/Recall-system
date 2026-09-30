using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_Vehicles_GetByVin;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_Vehicles_GetAll;");
        }
    }
}
