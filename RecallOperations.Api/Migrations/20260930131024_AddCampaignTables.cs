using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KS_Campaigns",
                schema: "dbo",
                columns: table => new
                {
                    NhtsaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RemedyInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AffectedComponent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    AffectedVins = table.Column<int>(type: "int", nullable: false),
                    ImportedRecords = table.Column<int>(type: "int", nullable: false),
                    SuccessfulImports = table.Column<int>(type: "int", nullable: false),
                    FailedImports = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KS_Campaigns", x => x.NhtsaId);
                });

            migrationBuilder.CreateTable(
                name: "KS_CampaignVehicles",
                schema: "dbo",
                columns: table => new
                {
                    CampaignId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    AppointmentBooked = table.Column<bool>(type: "bit", nullable: false),
                    RepairCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KS_CampaignVehicles", x => new { x.CampaignId, x.Vin });
                    table.ForeignKey(
                        name: "FK_KS_CampaignVehicles_KS_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalSchema: "dbo",
                        principalTable: "KS_Campaigns",
                        principalColumn: "NhtsaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KS_CampaignVehicles_KS_Vehicles_Vin",
                        column: x => x.Vin,
                        principalSchema: "dbo",
                        principalTable: "KS_Vehicles",
                        principalColumn: "Vin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KS_CampaignVehicles_Vin",
                schema: "dbo",
                table: "KS_CampaignVehicles",
                column: "Vin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KS_CampaignVehicles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KS_Campaigns",
                schema: "dbo");
        }
    }
}
