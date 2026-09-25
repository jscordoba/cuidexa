using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class SuscripcionesPushSuperAdminFamiliar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuscripcionesPushFamiliar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FamiliarId = table.Column<int>(type: "integer", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    P256dh = table.Column<string>(type: "text", nullable: false),
                    Auth = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesPushFamiliar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuscripcionesPushFamiliar_Familiares_FamiliarId",
                        column: x => x.FamiliarId,
                        principalTable: "Familiares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuscripcionesPushSuperAdmin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SuperAdminId = table.Column<int>(type: "integer", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    P256dh = table.Column<string>(type: "text", nullable: false),
                    Auth = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesPushSuperAdmin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuscripcionesPushSuperAdmin_SuperAdmins_SuperAdminId",
                        column: x => x.SuperAdminId,
                        principalTable: "SuperAdmins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPushFamiliar_Endpoint",
                table: "SuscripcionesPushFamiliar",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPushFamiliar_FamiliarId",
                table: "SuscripcionesPushFamiliar",
                column: "FamiliarId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPushSuperAdmin_Endpoint",
                table: "SuscripcionesPushSuperAdmin",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPushSuperAdmin_SuperAdminId",
                table: "SuscripcionesPushSuperAdmin",
                column: "SuperAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuscripcionesPushFamiliar");

            migrationBuilder.DropTable(
                name: "SuscripcionesPushSuperAdmin");
        }
    }
}
