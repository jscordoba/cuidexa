using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class Fase8DatosCentroInformes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorAcento",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoRuta",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableCargo",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableNombre",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "ConfiguracionCentro",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ConfiguracionCentro",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ColorAcento", "Direccion", "Email", "LogoRuta", "Nombre", "ResponsableCargo", "ResponsableNombre", "Telefono" },
                values: new object[] { null, null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorAcento",
                table: "ConfiguracionCentro");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "ConfiguracionCentro");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ConfiguracionCentro");

            migrationBuilder.DropColumn(
                name: "LogoRuta",
                table: "ConfiguracionCentro");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "ConfiguracionCentro");

            migrationBuilder.DropColumn(
                name: "ResponsableCargo",
                table: "ConfiguracionCentro");

            migrationBuilder.DropColumn(
                name: "ResponsableNombre",
                table: "ConfiguracionCentro");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "ConfiguracionCentro");
        }
    }
}
