using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase3Profesionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EspecialidadId",
                table: "Empleados",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SesionesTerapia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResidenteId = table.Column<int>(type: "integer", nullable: false),
                    EmpleadoId = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesTerapia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SesionesTerapia_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SesionesTerapia_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 1,
                column: "EspecialidadId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 2,
                column: "EspecialidadId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 3,
                column: "EspecialidadId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 4,
                column: "EspecialidadId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 6,
                column: "EspecialidadId",
                value: null);

            migrationBuilder.InsertData(
                table: "Especialidades",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Fisioterapia" },
                    { 2, "Logopedia" },
                    { 3, "Terapia ocupacional" },
                    { 4, "Psicología" }
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "Activo", "Email", "EspecialidadId", "Nombre", "PasswordHash", "Rol" },
                values: new object[] { 7, true, "profesional@demo.local", 1, "Fisioterapia Demo", "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm", 5 });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EspecialidadId",
                table: "Empleados",
                column: "EspecialidadId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesTerapia_EmpleadoId",
                table: "SesionesTerapia",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesTerapia_ResidenteId",
                table: "SesionesTerapia",
                column: "ResidenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Especialidades_EspecialidadId",
                table: "Empleados",
                column: "EspecialidadId",
                principalTable: "Especialidades",
                principalColumn: "Id");

            // Mismo motivo que en Fase2Enfermeria: sin esto, el próximo alta real
            // de empleado (Id automático) volvería a intentar el Id 7 y chocaría
            // con el Empleado sembrado aquí.
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Empleados\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Empleados\"));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Especialidades\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Especialidades\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Especialidades_EspecialidadId",
                table: "Empleados");

            migrationBuilder.DropTable(
                name: "Especialidades");

            migrationBuilder.DropTable(
                name: "SesionesTerapia");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_EspecialidadId",
                table: "Empleados");

            migrationBuilder.DeleteData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "EspecialidadId",
                table: "Empleados");
        }
    }
}
