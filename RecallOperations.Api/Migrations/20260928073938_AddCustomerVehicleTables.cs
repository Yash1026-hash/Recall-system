using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerVehicleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                schema: "dbo",
                table: "RecallUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecallCustomers",
                schema: "dbo",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecallCustomers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                schema: "dbo",
                columns: table => new
                {
                    Vin = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    Make = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    RecallStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Vin);
                });

            migrationBuilder.CreateTable(
                name: "CustomerVehicles",
                schema: "dbo",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(17)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerVehicles", x => new { x.CustomerId, x.Vin });
                    table.ForeignKey(
                        name: "FK_CustomerVehicles_RecallCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "RecallCustomers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerVehicles_Vehicles_Vin",
                        column: x => x.Vin,
                        principalSchema: "dbo",
                        principalTable: "Vehicles",
                        principalColumn: "Vin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecallUsers_CustomerId",
                schema: "dbo",
                table: "RecallUsers",
                column: "CustomerId",
                unique: true,
                filter: "[CustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_Vin",
                schema: "dbo",
                table: "CustomerVehicles",
                column: "Vin");

            migrationBuilder.CreateIndex(
                name: "IX_RecallCustomers_Email",
                schema: "dbo",
                table: "RecallCustomers",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecallUsers_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "RecallUsers",
                column: "CustomerId",
                principalSchema: "dbo",
                principalTable: "RecallCustomers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecallUsers_RecallCustomers_CustomerId",
                schema: "dbo",
                table: "RecallUsers");

            migrationBuilder.DropTable(
                name: "CustomerVehicles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RecallCustomers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Vehicles",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_RecallUsers_CustomerId",
                schema: "dbo",
                table: "RecallUsers");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "dbo",
                table: "RecallUsers");
        }
    }
}
