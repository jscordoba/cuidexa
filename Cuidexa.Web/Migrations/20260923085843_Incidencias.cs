using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Incidencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Incidencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CentroId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ResidenteId = table.Column<int>(type: "integer", nullable: true),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Gravedad = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    RolOrigen = table.Column<int>(type: "integer", nullable: false),
                    EmpleadoOrigenId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotasResolucion = table.Column<string>(type: "text", nullable: true),
                    ResueltoPorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidencias_Centros_CentroId",
                        column: x => x.CentroId,
                        principalTable: "Centros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Empleados_EmpleadoOrigenId",
                        column: x => x.EmpleadoOrigenId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Empleados_ResueltoPorId",
                        column: x => x.ResueltoPorId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidencias_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_CentroId",
                table: "Incidencias",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_EmpleadoOrigenId",
                table: "Incidencias",
                column: "EmpleadoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_ResidenteId",
                table: "Incidencias",
                column: "ResidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_ResueltoPorId",
                table: "Incidencias",
                column: "ResueltoPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Incidencias");
        }
    }
}
