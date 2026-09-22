using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase7NotificacionesGrupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RolDestino",
                table: "EventosDistribucion",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "EmpleadoDestinoId",
                table: "EventosDistribucion",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Urgente",
                table: "EventosDistribucion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GruposNotificacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposNotificacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuscripcionesPush",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpleadoId = table.Column<int>(type: "integer", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    P256dh = table.Column<string>(type: "text", nullable: false),
                    Auth = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesPush", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuscripcionesPush_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GruposNotificacionEmpleados",
                columns: table => new
                {
                    GrupoNotificacionId = table.Column<int>(type: "integer", nullable: false),
                    EmpleadoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposNotificacionEmpleados", x => new { x.GrupoNotificacionId, x.EmpleadoId });
                    table.ForeignKey(
                        name: "FK_GruposNotificacionEmpleados_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GruposNotificacionEmpleados_GruposNotificacion_GrupoNotific~",
                        column: x => x.GrupoNotificacionId,
                        principalTable: "GruposNotificacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosDistribucion_EmpleadoDestinoId",
                table: "EventosDistribucion",
                column: "EmpleadoDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposNotificacionEmpleados_EmpleadoId",
                table: "GruposNotificacionEmpleados",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPush_EmpleadoId",
                table: "SuscripcionesPush",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPush_Endpoint",
                table: "SuscripcionesPush",
                column: "Endpoint",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosDistribucion_Empleados_EmpleadoDestinoId",
                table: "EventosDistribucion",
                column: "EmpleadoDestinoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventosDistribucion_Empleados_EmpleadoDestinoId",
                table: "EventosDistribucion");

            migrationBuilder.DropTable(
                name: "GruposNotificacionEmpleados");

            migrationBuilder.DropTable(
                name: "SuscripcionesPush");

            migrationBuilder.DropTable(
                name: "GruposNotificacion");

            migrationBuilder.DropIndex(
                name: "IX_EventosDistribucion_EmpleadoDestinoId",
                table: "EventosDistribucion");

            migrationBuilder.DropColumn(
                name: "EmpleadoDestinoId",
                table: "EventosDistribucion");

            migrationBuilder.DropColumn(
                name: "Urgente",
                table: "EventosDistribucion");

            migrationBuilder.AlterColumn<int>(
                name: "RolDestino",
                table: "EventosDistribucion",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
