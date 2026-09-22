using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase2Enfermeria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResidenteId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Dosis = table.Column<string>(type: "text", nullable: false),
                    Horario = table.Column<string>(type: "text", nullable: false),
                    Instrucciones = table.Column<string>(type: "text", nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicaciones_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patologias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patologias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAdministracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedicacionId = table.Column<int>(type: "integer", nullable: false),
                    EmpleadoId = table.Column<int>(type: "integer", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAdministracion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosAdministracion_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegistrosAdministracion_Medicaciones_MedicacionId",
                        column: x => x.MedicacionId,
                        principalTable: "Medicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResidentePatologias",
                columns: table => new
                {
                    ResidenteId = table.Column<int>(type: "integer", nullable: false),
                    PatologiaId = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentePatologias", x => new { x.ResidenteId, x.PatologiaId });
                    table.ForeignKey(
                        name: "FK_ResidentePatologias_Patologias_PatologiaId",
                        column: x => x.PatologiaId,
                        principalTable: "Patologias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResidentePatologias_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "Activo", "Email", "Nombre", "PasswordHash", "Rol" },
                values: new object[] { 6, true, "enfermeria@demo.local", "Enfermeria Demo", "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm", 4 });

            migrationBuilder.InsertData(
                table: "Patologias",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Presión arterial elevada", "Hipertensión" },
                    { 2, "Control de glucosa en sangre", "Diabetes tipo 2" },
                    { 3, "Deterioro cognitivo", "Alzheimer" },
                    { 4, "Desgaste articular", "Artrosis" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicaciones_ResidenteId",
                table: "Medicaciones",
                column: "ResidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAdministracion_EmpleadoId",
                table: "RegistrosAdministracion",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAdministracion_MedicacionId",
                table: "RegistrosAdministracion",
                column: "MedicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentePatologias_PatologiaId",
                table: "ResidentePatologias",
                column: "PatologiaId");

            // Postgres no avanza la secuencia de identidad cuando insertas un Id
            // explícito (como el Empleado Id=6 de arriba) — sin esto, el próximo
            // alta real de empleado por la app volvería a intentar el Id 6 y
            // chocaría con este. Ya nos pasó una vez con los residentes de prueba
            // de esta misma sesión, así que a partir de ahora se resincroniza la
            // secuencia después de cada seed con Id explícito.
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Empleados\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Empleados\"));");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Patologias\"', 'Id'), (SELECT MAX(\"Id\") FROM \"Patologias\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosAdministracion");

            migrationBuilder.DropTable(
                name: "ResidentePatologias");

            migrationBuilder.DropTable(
                name: "Medicaciones");

            migrationBuilder.DropTable(
                name: "Patologias");

            migrationBuilder.DeleteData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
