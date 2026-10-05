using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentAndUserActivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "KS_RecallUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                schema: "dbo",
                table: "KS_RecallUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "KS_RecallUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "KS_Departments",
                schema: "dbo",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KS_Departments", x => x.DepartmentId);
                });

            migrationBuilder.Sql("""
                INSERT INTO [dbo].[KS_Departments] ([Name], [Description])
                VALUES
                    (N'Operations Management', N'Management and operations'),
                    (N'Field Engineering & Service', N'Field engineering and vehicle service'),
                    (N'Customer Support & Warranty', N'Customer support and warranty services'),
                    (N'General Operations', N'General operations');

                UPDATE users
                SET [DepartmentId] = department.[DepartmentId]
                FROM [dbo].[KS_RecallUsers] AS users
                INNER JOIN [dbo].[KS_Departments] AS department
                    ON department.[Name] = CASE users.[Role]
                        WHEN N'Manager' THEN N'Operations Management'
                        WHEN N'Technician' THEN N'Field Engineering & Service'
                        WHEN N'Customer' THEN N'Customer Support & Warranty'
                        ELSE N'General Operations'
                    END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_KS_RecallUsers_DepartmentId",
                schema: "dbo",
                table: "KS_RecallUsers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_KS_Departments_Name",
                schema: "dbo",
                table: "KS_Departments",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KS_RecallUsers_KS_Departments_DepartmentId",
                schema: "dbo",
                table: "KS_RecallUsers",
                column: "DepartmentId",
                principalSchema: "dbo",
                principalTable: "KS_Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KS_RecallUsers_KS_Departments_DepartmentId",
                schema: "dbo",
                table: "KS_RecallUsers");

            migrationBuilder.DropTable(
                name: "KS_Departments",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_KS_RecallUsers_DepartmentId",
                schema: "dbo",
                table: "KS_RecallUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "KS_RecallUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "dbo",
                table: "KS_RecallUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "KS_RecallUsers");
        }
    }
}
