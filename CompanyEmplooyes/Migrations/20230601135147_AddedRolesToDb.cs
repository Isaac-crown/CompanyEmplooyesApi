using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyEmplooyes.Migrations
{
    public partial class AddedRolesToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "41b40ec3-4398-4a96-9945-1442afb72105", "0afd4563-464d-4f4b-9b28-f2c18884711a", "Manager", "MANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "5b93d8bf-dab9-481d-80d7-da27ee232ce6", "29a43063-1614-4910-aa95-e0e81e9699b0", "Administrator", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "41b40ec3-4398-4a96-9945-1442afb72105");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5b93d8bf-dab9-481d-80d7-da27ee232ce6");
        }
    }
}
