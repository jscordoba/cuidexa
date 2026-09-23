using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class DocumentosFirmados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentosFirmados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CentroId = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    ResidenteId = table.Column<int>(type: "integer", nullable: true),
                    IncidenciaId = table.Column<int>(type: "integer", nullable: true),
                    FirmanteNombre = table.Column<string>(type: "text", nullable: false),
                    FirmanteRelacion = table.Column<string>(type: "text", nullable: false),
                    FirmaImagenBase64 = table.Column<string>(type: "text", nullable: false),
                    FechaFirma = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmpleadoRegistraId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosFirmados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosFirmados_Centros_CentroId",
                        column: x => x.CentroId,
                        principalTable: "Centros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosFirmados_Empleados_EmpleadoRegistraId",
                        column: x => x.EmpleadoRegistraId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosFirmados_Incidencias_IncidenciaId",
                        column: x => x.IncidenciaId,
                        principalTable: "Incidencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosFirmados_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFirmados_CentroId",
                table: "DocumentosFirmados",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFirmados_EmpleadoRegistraId",
                table: "DocumentosFirmados",
                column: "EmpleadoRegistraId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFirmados_IncidenciaId",
                table: "DocumentosFirmados",
                column: "IncidenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFirmados_ResidenteId",
                table: "DocumentosFirmados",
                column: "ResidenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosFirmados");
        }
    }
}
