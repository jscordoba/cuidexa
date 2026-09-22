using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase1Seguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpleadoId = table.Column<int>(type: "integer", nullable: true),
                    Accion = table.Column<string>(type: "text", nullable: false),
                    EntidadTipo = table.Column<string>(type: "text", nullable: false),
                    EntidadId = table.Column<int>(type: "integer", nullable: false),
                    Detalle = table.Column<string>(type: "text", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm");

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm");

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm");

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmpleadoId",
                table: "AuditLogs",
                column: "EmpleadoId");

            // Los residentes existentes tienen estos campos en texto plano (datos de
            // prueba de sesiones anteriores). A partir de esta migración la app los
            // lee como cifrados y fallaría al intentar descifrar texto plano, así que
            // se limpian aquí. No afecta a datos reales: este MVP nunca ha estado en
            // producción.
            migrationBuilder.Sql(@"
                UPDATE ""Residentes""
                SET ""DocumentoIdentidad"" = NULL,
                    ""Telefono"" = NULL,
                    ""Email"" = NULL,
                    ""ContactoEmergenciaTelefono"" = NULL,
                    ""ContactoEmergenciaEmail"" = NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3");

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3");

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3");

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3");
        }
    }
}
