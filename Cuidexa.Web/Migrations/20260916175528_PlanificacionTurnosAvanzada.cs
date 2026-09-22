using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class PlanificacionTurnosAvanzada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsReemplazo",
                table: "Turnos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MotivoReemplazo",
                table: "Turnos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlantillaTurnoId",
                table: "Turnos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlantillasTurno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TipoDia = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasTurno", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vacaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpleadoId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResueltoPorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacaciones_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vacaciones_Empleados_ResueltoPorId",
                        column: x => x.ResueltoPorId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PlantillasTurno",
                columns: new[] { "Id", "Activo", "HoraFin", "HoraInicio", "Nombre", "TipoDia" },
                values: new object[,]
                {
                    { 1, true, new TimeOnly(15, 0, 0), new TimeOnly(7, 0, 0), "Mañana", 0 },
                    { 2, true, new TimeOnly(23, 0, 0), new TimeOnly(15, 0, 0), "Tarde", 0 },
                    { 3, true, new TimeOnly(7, 0, 0), new TimeOnly(23, 0, 0), "Noche", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_PlantillaTurnoId",
                table: "Turnos",
                column: "PlantillaTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacaciones_EmpleadoId",
                table: "Vacaciones",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacaciones_ResueltoPorId",
                table: "Vacaciones",
                column: "ResueltoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_PlantillasTurno_PlantillaTurnoId",
                table: "Turnos",
                column: "PlantillaTurnoId",
                principalTable: "PlantillasTurno",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_PlantillasTurno_PlantillaTurnoId",
                table: "Turnos");

            migrationBuilder.DropTable(
                name: "PlantillasTurno");

            migrationBuilder.DropTable(
                name: "Vacaciones");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_PlantillaTurnoId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "EsReemplazo",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "MotivoReemplazo",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "PlantillaTurnoId",
                table: "Turnos");
        }
    }
}
