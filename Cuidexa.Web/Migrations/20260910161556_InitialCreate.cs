using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alergias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alergias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dietas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dietas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    Planta = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Residentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    HabitacionId = table.Column<int>(type: "integer", nullable: false),
                    Movilidad = table.Column<int>(type: "integer", nullable: false),
                    NecesitaAyudaLevantarse = table.Column<bool>(type: "boolean", nullable: false),
                    EquipamientoEspecial = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Residentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Residentes_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventosDistribucion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResidenteId = table.Column<int>(type: "integer", nullable: false),
                    TipoEvento = table.Column<int>(type: "integer", nullable: false),
                    RolDestino = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Leido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosDistribucion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosDistribucion_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResidenteAlergias",
                columns: table => new
                {
                    ResidenteId = table.Column<int>(type: "integer", nullable: false),
                    AlergiaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidenteAlergias", x => new { x.ResidenteId, x.AlergiaId });
                    table.ForeignKey(
                        name: "FK_ResidenteAlergias_Alergias_AlergiaId",
                        column: x => x.AlergiaId,
                        principalTable: "Alergias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResidenteAlergias_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResidenteDietas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResidenteId = table.Column<int>(type: "integer", nullable: false),
                    DietaId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    Motivo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidenteDietas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidenteDietas_Dietas_DietaId",
                        column: x => x.DietaId,
                        principalTable: "Dietas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResidenteDietas_Residentes_ResidenteId",
                        column: x => x.ResidenteId,
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Alergias",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Frutos secos" },
                    { 2, "Lactosa" },
                    { 3, "Gluten" }
                });

            migrationBuilder.InsertData(
                table: "Dietas",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Dieta estándar", "Normal" },
                    { 2, "Textura modificada", "Triturada" },
                    { 3, "Control de azúcares", "Diabética" }
                });

            migrationBuilder.InsertData(
                table: "Empleados",
                columns: new[] { "Id", "Activo", "Email", "Nombre", "PasswordHash", "Rol" },
                values: new object[,]
                {
                    { 1, true, "admin@demo.local", "Admin Demo", "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3", 0 },
                    { 2, true, "cocina@demo.local", "Cocina Demo", "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3", 1 },
                    { 3, true, "auxiliar@demo.local", "Auxiliar Demo", "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3", 2 },
                    { 4, true, "direccion@demo.local", "Direccion Demo", "7DBB7F051B44D7D54584A7BC6C32F00DA5A3B5E6973485B9FB176FE56346B3E3", 3 }
                });

            migrationBuilder.InsertData(
                table: "Habitaciones",
                columns: new[] { "Id", "Numero", "Planta" },
                values: new object[,]
                {
                    { 1, "101", "Planta 1" },
                    { 2, "102", "Planta 1" },
                    { 3, "201", "Planta 2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosDistribucion_ResidenteId",
                table: "EventosDistribucion",
                column: "ResidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidenteAlergias_AlergiaId",
                table: "ResidenteAlergias",
                column: "AlergiaId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidenteDietas_DietaId",
                table: "ResidenteDietas",
                column: "DietaId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidenteDietas_ResidenteId",
                table: "ResidenteDietas",
                column: "ResidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Residentes_HabitacionId",
                table: "Residentes",
                column: "HabitacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "EventosDistribucion");

            migrationBuilder.DropTable(
                name: "ResidenteAlergias");

            migrationBuilder.DropTable(
                name: "ResidenteDietas");

            migrationBuilder.DropTable(
                name: "Alergias");

            migrationBuilder.DropTable(
                name: "Dietas");

            migrationBuilder.DropTable(
                name: "Residentes");

            migrationBuilder.DropTable(
                name: "Habitaciones");
        }
    }
}
