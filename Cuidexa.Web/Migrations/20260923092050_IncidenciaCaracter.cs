using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class IncidenciaCaracter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Caracter",
                table: "Incidencias",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Caracter",
                table: "Incidencias");
        }
    }
}
