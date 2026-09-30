using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KS_UserDepartments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KS_Departments",
                schema: "dbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KS_Departments",
                schema: "dbo",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KS_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "KS_UserDepartments",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KS_UserDepartments", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_KS_UserDepartments_KS_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "dbo",
                        principalTable: "KS_Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KS_UserDepartments_KS_RecallUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "KS_RecallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KS_Departments_Name",
                schema: "dbo",
                table: "KS_Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KS_UserDepartments_DepartmentId",
                schema: "dbo",
                table: "KS_UserDepartments",
                column: "DepartmentId");
        }
    }
}
