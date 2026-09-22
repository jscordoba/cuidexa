using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class AvisosManualesConOrigen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpleadoOrigenId",
                table: "EventosDistribucion",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RolOrigen",
                table: "EventosDistribucion",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosDistribucion_EmpleadoOrigenId",
                table: "EventosDistribucion",
                column: "EmpleadoOrigenId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventosDistribucion_Empleados_EmpleadoOrigenId",
                table: "EventosDistribucion",
                column: "EmpleadoOrigenId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventosDistribucion_Empleados_EmpleadoOrigenId",
                table: "EventosDistribucion");

            migrationBuilder.DropIndex(
                name: "IX_EventosDistribucion_EmpleadoOrigenId",
                table: "EventosDistribucion");

            migrationBuilder.DropColumn(
                name: "EmpleadoOrigenId",
                table: "EventosDistribucion");

            migrationBuilder.DropColumn(
                name: "RolOrigen",
                table: "EventosDistribucion");
        }
    }
}
