using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class TurnoPorDefectoYCambiosPorFecha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CambiosTurno_Turnos_TurnoId",
                table: "CambiosTurno");

            migrationBuilder.DropIndex(
                name: "IX_CambiosTurno_TurnoId",
                table: "CambiosTurno");

            migrationBuilder.DropColumn(
                name: "TurnoId",
                table: "CambiosTurno");

            migrationBuilder.AddColumn<int>(
                name: "Rol",
                table: "CambiosTurno",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmpleadoOriginalId",
                table: "Turnos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsCambio",
                table: "Turnos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PlantillaTurnoDefectoId",
                table: "Empleados",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Fecha",
                table: "CambiosTurno",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            // Todo empleado de un rol operativo (Cocina/Auxiliar/Enfermeria/
            // Profesional/Limpieza — enum 1,2,4,5,6; excluye Admin=0 y
            // Direccion=3) queda con la plantilla "Mañana" (Id 1, sembrada en
            // PlanificacionTurnosAvanzada) como turno por defecto de partida —
            // Admin puede cambiarla luego desde Empleados/Edit.
            migrationBuilder.Sql(@"UPDATE ""Empleados"" SET ""PlantillaTurnoDefectoId"" = 1 WHERE ""Rol"" IN (1,2,4,5,6);");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_EmpleadoOriginalId",
                table: "Turnos",
                column: "EmpleadoOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_PlantillaTurnoDefectoId",
                table: "Empleados",
                column: "PlantillaTurnoDefectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_PlantillasTurno_PlantillaTurnoDefectoId",
                table: "Empleados",
                column: "PlantillaTurnoDefectoId",
                principalTable: "PlantillasTurno",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_Empleados_EmpleadoOriginalId",
                table: "Turnos",
                column: "EmpleadoOriginalId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_PlantillasTurno_PlantillaTurnoDefectoId",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_Empleados_EmpleadoOriginalId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_EmpleadoOriginalId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_PlantillaTurnoDefectoId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "EmpleadoOriginalId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "EsCambio",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "PlantillaTurnoDefectoId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "CambiosTurno");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "CambiosTurno");

            migrationBuilder.AddColumn<int>(
                name: "TurnoId",
                table: "CambiosTurno",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CambiosTurno_TurnoId",
                table: "CambiosTurno",
                column: "TurnoId");

            migrationBuilder.AddForeignKey(
                name: "FK_CambiosTurno_Turnos_TurnoId",
                table: "CambiosTurno",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
