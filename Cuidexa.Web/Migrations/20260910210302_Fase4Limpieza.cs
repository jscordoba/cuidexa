using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase4Limpieza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TareasLimpieza",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HabitacionId = table.Column<int>(type: "integer", nullable: true),
                    Zona = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCompletada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmpleadoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareasLimpieza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TareasLimpieza_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TareasLimpieza_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "Activo", "Email", "EspecialidadId", "Nombre", "PasswordHash", "Rol" },
                values: new object[] { 8, true, "limpieza@demo.local", null, "Limpieza Demo", "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm", 6 });

            migrationBuilder.CreateIndex(
                name: "IX_TareasLimpieza_EmpleadoId",
                table: "TareasLimpieza",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_TareasLimpieza_HabitacionId",
                table: "TareasLimpieza",
                column: "HabitacionId");

            // Mismo motivo que en Fase2Enfermeria/Fase3Profesionales.
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Empleados\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Empleados\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TareasLimpieza");

            migrationBuilder.DeleteData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
