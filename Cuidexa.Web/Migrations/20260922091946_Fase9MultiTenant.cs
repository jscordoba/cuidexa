using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase9MultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Antes de la Fase 9 el email de Empleado no tenía ninguna
            // restricción de unicidad (ni siquiera global) — con datos de
            // pruebas reales de sesiones anteriores es posible que existan
            // duplicados entre una fila inactiva de prueba y una activa. Se
            // renombra el email de la copia inactiva antes de crear el
            // índice único (CentroId, Email) más abajo, para no bloquear la
            // migración; la fila activa nunca se toca.
            migrationBuilder.Sql(@"
                UPDATE ""Empleados"" e
                SET ""Email"" = e.""Email"" || '.duplicado-' || e.""Id""
                WHERE e.""Activo"" = false
                  AND EXISTS (
                    SELECT 1 FROM ""Empleados"" e2
                    WHERE e2.""Email"" = e.""Email"" AND e2.""Id"" <> e.""Id""
                  );
            ");

            migrationBuilder.DropIndex(
                name: "IX_Feriados_Fecha",
                table: "Feriados");

            migrationBuilder.DropIndex(
                name: "IX_CuentasDispositivo_Nombre",
                table: "CuentasDispositivo");

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "Vacaciones",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "Turnos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "TareasLimpieza",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "SuscripcionesPush",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "SesionesTerapia",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "Residentes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "ResidentePatologias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "ResidenteDietas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "ResidenteAlergias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "RegistrosAdministracion",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrganizacionId",
                table: "PlantillasTurno",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrganizacionId",
                table: "Patologias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "Medicaciones",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "Habitaciones",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "GruposNotificacionEmpleados",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "GruposNotificacion",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "Feriados",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "EventosDistribucion",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrganizacionId",
                table: "Especialidades",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "Empleados",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrganizacionId",
                table: "Dietas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "CuentasDispositivo",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "CambiosTurno",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CentroId",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrganizacionId",
                table: "Alergias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // El valor por defecto 1 de arriba solo sirve para rellenar filas ya
            // existentes en la migración (backfill a Centro/Organización #1); se
            // retira justo después para que cualquier inserción futura que olvide
            // fijar CentroId/OrganizacionId falle alto y claro, en vez de colar
            // datos silenciosamente en el centro demo.
            migrationBuilder.Sql("ALTER TABLE \"Vacaciones\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Turnos\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"TareasLimpieza\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"SuscripcionesPush\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"SesionesTerapia\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Residentes\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"ResidentePatologias\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"ResidenteDietas\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"ResidenteAlergias\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"RegistrosAdministracion\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"PlantillasTurno\" ALTER COLUMN \"OrganizacionId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Patologias\" ALTER COLUMN \"OrganizacionId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Medicaciones\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Habitaciones\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"GruposNotificacionEmpleados\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"GruposNotificacion\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Feriados\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"EventosDistribucion\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Especialidades\" ALTER COLUMN \"OrganizacionId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Empleados\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Dietas\" ALTER COLUMN \"OrganizacionId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"CuentasDispositivo\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"CambiosTurno\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"AuditLogs\" ALTER COLUMN \"CentroId\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Alergias\" ALTER COLUMN \"OrganizacionId\" DROP DEFAULT;");

            migrationBuilder.CreateTable(
                name: "Organizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    LogoRuta = table.Column<string>(type: "text", nullable: true),
                    ColorAcento = table.Column<string>(type: "text", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdmins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdmins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Centros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizacionId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    ComunidadAutonoma = table.Column<int>(type: "integer", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ResponsableNombre = table.Column<string>(type: "text", nullable: true),
                    ResponsableCargo = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Centros_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Alergias",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Alergias",
                keyColumn: "Id",
                keyValue: 2,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Alergias",
                keyColumn: "Id",
                keyValue: 3,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Dietas",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Dietas",
                keyColumn: "Id",
                keyValue: 2,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Dietas",
                keyColumn: "Id",
                keyValue: 3,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 1,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 2,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 3,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 4,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 6,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 7,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 8,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Especialidades",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Especialidades",
                keyColumn: "Id",
                keyValue: 2,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Especialidades",
                keyColumn: "Id",
                keyValue: 3,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Especialidades",
                keyColumn: "Id",
                keyValue: 4,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 1,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 2,
                column: "CentroId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 3,
                column: "CentroId",
                value: 1);

            migrationBuilder.InsertData(
                table: "Organizaciones",
                columns: new[] { "Id", "Activa", "ColorAcento", "FechaCreacion", "LogoRuta", "Nombre" },
                values: new object[] { 1, true, null, new DateTime(2026, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cuidexa" });

            migrationBuilder.UpdateData(
                table: "Patologias",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Patologias",
                keyColumn: "Id",
                keyValue: 2,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Patologias",
                keyColumn: "Id",
                keyValue: 3,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Patologias",
                keyColumn: "Id",
                keyValue: 4,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PlantillasTurno",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PlantillasTurno",
                keyColumn: "Id",
                keyValue: 2,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PlantillasTurno",
                keyColumn: "Id",
                keyValue: 3,
                column: "OrganizacionId",
                value: 1);

            migrationBuilder.InsertData(
                table: "Centros",
                columns: new[] { "Id", "Activo", "Codigo", "ComunidadAutonoma", "Direccion", "Email", "FechaCreacion", "Nombre", "OrganizacionId", "ResponsableCargo", "ResponsableNombre", "Telefono" },
                values: new object[] { 1, true, "DEMO", null, null, null, new DateTime(2026, 9, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Centro Demo", 1, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Vacaciones_CentroId",
                table: "Vacaciones",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_CentroId",
                table: "Turnos",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_TareasLimpieza_CentroId",
                table: "TareasLimpieza",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPush_CentroId",
                table: "SuscripcionesPush",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesTerapia_CentroId",
                table: "SesionesTerapia",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Residentes_CentroId",
                table: "Residentes",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentePatologias_CentroId",
                table: "ResidentePatologias",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidenteDietas_CentroId",
                table: "ResidenteDietas",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidenteAlergias_CentroId",
                table: "ResidenteAlergias",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAdministracion_CentroId",
                table: "RegistrosAdministracion",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasTurno_OrganizacionId",
                table: "PlantillasTurno",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Patologias_OrganizacionId",
                table: "Patologias",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicaciones_CentroId",
                table: "Medicaciones",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_CentroId",
                table: "Habitaciones",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposNotificacionEmpleados_CentroId",
                table: "GruposNotificacionEmpleados",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposNotificacion_CentroId",
                table: "GruposNotificacion",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Feriados_CentroId_Fecha",
                table: "Feriados",
                columns: new[] { "CentroId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosDistribucion_CentroId",
                table: "EventosDistribucion",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Especialidades_OrganizacionId",
                table: "Especialidades",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_CentroId_Email",
                table: "Empleados",
                columns: new[] { "CentroId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dietas_OrganizacionId",
                table: "Dietas",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasDispositivo_CentroId_Nombre",
                table: "CuentasDispositivo",
                columns: new[] { "CentroId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CambiosTurno_CentroId",
                table: "CambiosTurno",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CentroId",
                table: "AuditLogs",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Alergias_OrganizacionId",
                table: "Alergias",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Centros_Codigo",
                table: "Centros",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Centros_OrganizacionId",
                table: "Centros",
                column: "OrganizacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alergias_Organizaciones_OrganizacionId",
                table: "Alergias",
                column: "OrganizacionId",
                principalTable: "Organizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Centros_CentroId",
                table: "AuditLogs",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CambiosTurno_Centros_CentroId",
                table: "CambiosTurno",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasDispositivo_Centros_CentroId",
                table: "CuentasDispositivo",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dietas_Organizaciones_OrganizacionId",
                table: "Dietas",
                column: "OrganizacionId",
                principalTable: "Organizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Centros_CentroId",
                table: "Empleados",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Especialidades_Organizaciones_OrganizacionId",
                table: "Especialidades",
                column: "OrganizacionId",
                principalTable: "Organizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosDistribucion_Centros_CentroId",
                table: "EventosDistribucion",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feriados_Centros_CentroId",
                table: "Feriados",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GruposNotificacion_Centros_CentroId",
                table: "GruposNotificacion",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GruposNotificacionEmpleados_Centros_CentroId",
                table: "GruposNotificacionEmpleados",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Habitaciones_Centros_CentroId",
                table: "Habitaciones",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicaciones_Centros_CentroId",
                table: "Medicaciones",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patologias_Organizaciones_OrganizacionId",
                table: "Patologias",
                column: "OrganizacionId",
                principalTable: "Organizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantillasTurno_Organizaciones_OrganizacionId",
                table: "PlantillasTurno",
                column: "OrganizacionId",
                principalTable: "Organizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosAdministracion_Centros_CentroId",
                table: "RegistrosAdministracion",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResidenteAlergias_Centros_CentroId",
                table: "ResidenteAlergias",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResidenteDietas_Centros_CentroId",
                table: "ResidenteDietas",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResidentePatologias_Centros_CentroId",
                table: "ResidentePatologias",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Residentes_Centros_CentroId",
                table: "Residentes",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SesionesTerapia_Centros_CentroId",
                table: "SesionesTerapia",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuscripcionesPush_Centros_CentroId",
                table: "SuscripcionesPush",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TareasLimpieza_Centros_CentroId",
                table: "TareasLimpieza",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_Centros_CentroId",
                table: "Turnos",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vacaciones_Centros_CentroId",
                table: "Vacaciones",
                column: "CentroId",
                principalTable: "Centros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alergias_Organizaciones_OrganizacionId",
                table: "Alergias");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Centros_CentroId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CambiosTurno_Centros_CentroId",
                table: "CambiosTurno");

            migrationBuilder.DropForeignKey(
                name: "FK_CuentasDispositivo_Centros_CentroId",
                table: "CuentasDispositivo");

            migrationBuilder.DropForeignKey(
                name: "FK_Dietas_Organizaciones_OrganizacionId",
                table: "Dietas");

            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Centros_CentroId",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_Especialidades_Organizaciones_OrganizacionId",
                table: "Especialidades");

            migrationBuilder.DropForeignKey(
                name: "FK_EventosDistribucion_Centros_CentroId",
                table: "EventosDistribucion");

            migrationBuilder.DropForeignKey(
                name: "FK_Feriados_Centros_CentroId",
                table: "Feriados");

            migrationBuilder.DropForeignKey(
                name: "FK_GruposNotificacion_Centros_CentroId",
                table: "GruposNotificacion");

            migrationBuilder.DropForeignKey(
                name: "FK_GruposNotificacionEmpleados_Centros_CentroId",
                table: "GruposNotificacionEmpleados");

            migrationBuilder.DropForeignKey(
                name: "FK_Habitaciones_Centros_CentroId",
                table: "Habitaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicaciones_Centros_CentroId",
                table: "Medicaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Patologias_Organizaciones_OrganizacionId",
                table: "Patologias");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantillasTurno_Organizaciones_OrganizacionId",
                table: "PlantillasTurno");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosAdministracion_Centros_CentroId",
                table: "RegistrosAdministracion");

            migrationBuilder.DropForeignKey(
                name: "FK_ResidenteAlergias_Centros_CentroId",
                table: "ResidenteAlergias");

            migrationBuilder.DropForeignKey(
                name: "FK_ResidenteDietas_Centros_CentroId",
                table: "ResidenteDietas");

            migrationBuilder.DropForeignKey(
                name: "FK_ResidentePatologias_Centros_CentroId",
                table: "ResidentePatologias");

            migrationBuilder.DropForeignKey(
                name: "FK_Residentes_Centros_CentroId",
                table: "Residentes");

            migrationBuilder.DropForeignKey(
                name: "FK_SesionesTerapia_Centros_CentroId",
                table: "SesionesTerapia");

            migrationBuilder.DropForeignKey(
                name: "FK_SuscripcionesPush_Centros_CentroId",
                table: "SuscripcionesPush");

            migrationBuilder.DropForeignKey(
                name: "FK_TareasLimpieza_Centros_CentroId",
                table: "TareasLimpieza");

            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_Centros_CentroId",
                table: "Turnos");

            migrationBuilder.DropForeignKey(
                name: "FK_Vacaciones_Centros_CentroId",
                table: "Vacaciones");

            migrationBuilder.DropTable(
                name: "Centros");

            migrationBuilder.DropTable(
                name: "SuperAdmins");

            migrationBuilder.DropTable(
                name: "Organizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Vacaciones_CentroId",
                table: "Vacaciones");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_CentroId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_TareasLimpieza_CentroId",
                table: "TareasLimpieza");

            migrationBuilder.DropIndex(
                name: "IX_SuscripcionesPush_CentroId",
                table: "SuscripcionesPush");

            migrationBuilder.DropIndex(
                name: "IX_SesionesTerapia_CentroId",
                table: "SesionesTerapia");

            migrationBuilder.DropIndex(
                name: "IX_Residentes_CentroId",
                table: "Residentes");

            migrationBuilder.DropIndex(
                name: "IX_ResidentePatologias_CentroId",
                table: "ResidentePatologias");

            migrationBuilder.DropIndex(
                name: "IX_ResidenteDietas_CentroId",
                table: "ResidenteDietas");

            migrationBuilder.DropIndex(
                name: "IX_ResidenteAlergias_CentroId",
                table: "ResidenteAlergias");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosAdministracion_CentroId",
                table: "RegistrosAdministracion");

            migrationBuilder.DropIndex(
                name: "IX_PlantillasTurno_OrganizacionId",
                table: "PlantillasTurno");

            migrationBuilder.DropIndex(
                name: "IX_Patologias_OrganizacionId",
                table: "Patologias");

            migrationBuilder.DropIndex(
                name: "IX_Medicaciones_CentroId",
                table: "Medicaciones");

            migrationBuilder.DropIndex(
                name: "IX_Habitaciones_CentroId",
                table: "Habitaciones");

            migrationBuilder.DropIndex(
                name: "IX_GruposNotificacionEmpleados_CentroId",
                table: "GruposNotificacionEmpleados");

            migrationBuilder.DropIndex(
                name: "IX_GruposNotificacion_CentroId",
                table: "GruposNotificacion");

            migrationBuilder.DropIndex(
                name: "IX_Feriados_CentroId_Fecha",
                table: "Feriados");

            migrationBuilder.DropIndex(
                name: "IX_EventosDistribucion_CentroId",
                table: "EventosDistribucion");

            migrationBuilder.DropIndex(
                name: "IX_Especialidades_OrganizacionId",
                table: "Especialidades");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_CentroId_Email",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Dietas_OrganizacionId",
                table: "Dietas");

            migrationBuilder.DropIndex(
                name: "IX_CuentasDispositivo_CentroId_Nombre",
                table: "CuentasDispositivo");

            migrationBuilder.DropIndex(
                name: "IX_CambiosTurno_CentroId",
                table: "CambiosTurno");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CentroId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Alergias_OrganizacionId",
                table: "Alergias");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "Vacaciones");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "TareasLimpieza");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "SuscripcionesPush");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "SesionesTerapia");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "ResidentePatologias");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "ResidenteDietas");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "ResidenteAlergias");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "RegistrosAdministracion");

            migrationBuilder.DropColumn(
                name: "OrganizacionId",
                table: "PlantillasTurno");

            migrationBuilder.DropColumn(
                name: "OrganizacionId",
                table: "Patologias");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "Medicaciones");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "Habitaciones");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "GruposNotificacionEmpleados");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "GruposNotificacion");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "Feriados");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "EventosDistribucion");

            migrationBuilder.DropColumn(
                name: "OrganizacionId",
                table: "Especialidades");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "OrganizacionId",
                table: "Dietas");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "CuentasDispositivo");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "CambiosTurno");

            migrationBuilder.DropColumn(
                name: "CentroId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OrganizacionId",
                table: "Alergias");

            migrationBuilder.CreateIndex(
                name: "IX_Feriados_Fecha",
                table: "Feriados",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasDispositivo_Nombre",
                table: "CuentasDispositivo",
                column: "Nombre",
                unique: true);
        }
    }
}
