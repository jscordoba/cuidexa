using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase9EliminarConfiguracionCentro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionCentro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionCentro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColorAcento = table.Column<string>(type: "text", nullable: true),
                    ComunidadAutonoma = table.Column<int>(type: "integer", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    LogoRuta = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    ResponsableCargo = table.Column<string>(type: "text", nullable: true),
                    ResponsableNombre = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionCentro", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ConfiguracionCentro",
                columns: new[] { "Id", "ColorAcento", "ComunidadAutonoma", "Direccion", "Email", "LogoRuta", "Nombre", "ResponsableCargo", "ResponsableNombre", "Telefono" },
                values: new object[] { 1, null, null, null, null, null, null, null, null, null });
        }
    }
}
