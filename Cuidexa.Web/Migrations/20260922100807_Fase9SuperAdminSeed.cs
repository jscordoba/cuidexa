using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase9SuperAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SuperAdmins",
                columns: new[] { "Id", "Activo", "Email", "Nombre", "PasswordHash" },
                values: new object[] { 1, true, "superadmin@cuidexa.local", "SuperAdmin", "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm" });

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdmins_Email",
                table: "SuperAdmins",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SuperAdmins_Email",
                table: "SuperAdmins");

            migrationBuilder.DeleteData(
                table: "SuperAdmins",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
