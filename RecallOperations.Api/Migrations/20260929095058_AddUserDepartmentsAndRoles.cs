using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDepartmentsAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "KS_Roles",
                schema: "dbo",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KS_Roles", x => x.RoleId);
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

            migrationBuilder.CreateTable(
                name: "KS_UserRoles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KS_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_KS_UserRoles_KS_RecallUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "KS_RecallUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KS_UserRoles_KS_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "KS_Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "KS_Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[,]
                {
                    { 1, "Customer" },
                    { 2, "Manager" },
                    { 3, "Technician" }
                });

            migrationBuilder.Sql(
                "INSERT INTO [dbo].[KS_Roles] ([Name]) " +
                "SELECT DISTINCT LTRIM(RTRIM([Role])) " +
                "FROM [dbo].[KS_RecallUsers] " +
                "WHERE NULLIF(LTRIM(RTRIM([Role])), '') IS NOT NULL " +
                "AND NOT EXISTS (SELECT 1 FROM [dbo].[KS_Roles] r WHERE r.[Name] = LTRIM(RTRIM([KS_RecallUsers].[Role])));");

            migrationBuilder.Sql(
                "INSERT INTO [dbo].[KS_UserRoles] ([UserId], [RoleId]) " +
                "SELECT u.[UserId], r.[RoleId] " +
                "FROM [dbo].[KS_RecallUsers] u " +
                "INNER JOIN [dbo].[KS_Roles] r ON r.[Name] = LTRIM(RTRIM(u.[Role])) " +
                "WHERE NOT EXISTS (SELECT 1 FROM [dbo].[KS_UserRoles] ur WHERE ur.[UserId] = u.[UserId] AND ur.[RoleId] = r.[RoleId]);");

            migrationBuilder.CreateIndex(
                name: "IX_KS_Departments_Name",
                schema: "dbo",
                table: "KS_Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KS_Roles_Name",
                schema: "dbo",
                table: "KS_Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KS_UserDepartments_DepartmentId",
                schema: "dbo",
                table: "KS_UserDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_KS_UserRoles_RoleId",
                schema: "dbo",
                table: "KS_UserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KS_UserDepartments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KS_UserRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KS_Departments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KS_Roles",
                schema: "dbo");
        }
    }
}
