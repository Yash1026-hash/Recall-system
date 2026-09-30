using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecallOperations.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedMockRecallRoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "RecallCustomers",
                columns: new[] { "CustomerId", "Email", "FullName" },
                values: new object[,]
                {
                    { 1, "arjun.sharma@example.com", "Arjun Sharma" },
                    { 2, "priya.reddy@example.com", "Priya Reddy" },
                    { 3, "rahul.verma@example.com", "Rahul Verma" },
                    { 4, "sneha.patel@example.com", "Sneha Patel" },
                    { 5, "vikram.nair@example.com", "Vikram Nair" },
                    { 6, "ananya.rao@example.com", "Ananya Rao" },
                    { 7, "rohit.kumar@example.com", "Rohit Kumar" },
                    { 8, "meera.iyer@example.com", "Meera Iyer" },
                    { 9, "aditya.singh@example.com", "Aditya Singh" },
                    { 10, "kavya.menon@example.com", "Kavya Menon" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Vehicles",
                columns: new[] { "Vin", "Make", "Model", "RecallStatus", "Year" },
                values: new object[,]
                {
                    { "1C4RJFAG5FC123458", "Jeep", "Grand Cherokee", "Open", 2018 },
                    { "1HGCM82633A123456", "Honda", "Accord", "Open", 2020 },
                    { "1N4AL3AP9HC123463", "Nissan", "Altima", "Open", 2019 },
                    { "2T1BURHE7JC123462", "Toyota", "Camry", "No Recall", 2020 },
                    { "3FA6P0H76KR123459", "Ford", "Fusion", "No Recall", 2021 },
                    { "5YFBURHE5FP123457", "Toyota", "Corolla", "Closed", 2019 },
                    { "JM1BL1SF8A1234560", "Mazda", "3", "Open", 2022 },
                    { "KMHD84LF5KU123465", "Hyundai", "Elantra", "No Recall", 2022 },
                    { "SALWR2RV7JA123464", "Land Rover", "Range Rover", "Closed", 2021 },
                    { "WVWZZZ3CZWE123461", "Volkswagen", "Jetta", "Closed", 2017 }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "CustomerVehicles",
                columns: new[] { "CustomerId", "Vin" },
                values: new object[,]
                {
                    { 1, "1HGCM82633A123456" },
                    { 2, "5YFBURHE5FP123457" },
                    { 3, "1C4RJFAG5FC123458" },
                    { 4, "3FA6P0H76KR123459" },
                    { 5, "JM1BL1SF8A1234560" },
                    { 6, "WVWZZZ3CZWE123461" },
                    { 7, "2T1BURHE7JC123462" },
                    { 8, "1N4AL3AP9HC123463" },
                    { 9, "SALWR2RV7JA123464" },
                    { 10, "KMHD84LF5KU123465" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 1, "1HGCM82633A123456" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 2, "5YFBURHE5FP123457" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 3, "1C4RJFAG5FC123458" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 4, "3FA6P0H76KR123459" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 5, "JM1BL1SF8A1234560" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 6, "WVWZZZ3CZWE123461" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 7, "2T1BURHE7JC123462" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 8, "1N4AL3AP9HC123463" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 9, "SALWR2RV7JA123464" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "CustomerVehicles",
                keyColumns: new[] { "CustomerId", "Vin" },
                keyValues: new object[] { 10, "KMHD84LF5KU123465" });

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "RecallCustomers",
                keyColumn: "CustomerId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "1C4RJFAG5FC123458");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "1HGCM82633A123456");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "1N4AL3AP9HC123463");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "2T1BURHE7JC123462");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "3FA6P0H76KR123459");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "5YFBURHE5FP123457");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "JM1BL1SF8A1234560");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "KMHD84LF5KU123465");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "SALWR2RV7JA123464");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Vin",
                keyValue: "WVWZZZ3CZWE123461");
        }
    }
}
