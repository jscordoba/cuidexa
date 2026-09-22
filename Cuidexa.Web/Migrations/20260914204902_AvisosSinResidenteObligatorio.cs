using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class AvisosSinResidenteObligatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventosDistribucion_Residentes_ResidenteId",
                table: "EventosDistribucion");

            migrationBuilder.AlterColumn<int>(
                name: "ResidenteId",
                table: "EventosDistribucion",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_EventosDistribucion_Residentes_ResidenteId",
                table: "EventosDistribucion",
                column: "ResidenteId",
                principalTable: "Residentes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventosDistribucion_Residentes_ResidenteId",
                table: "EventosDistribucion");

            migrationBuilder.AlterColumn<int>(
                name: "ResidenteId",
                table: "EventosDistribucion",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosDistribucion_Residentes_ResidenteId",
                table: "EventosDistribucion",
                column: "ResidenteId",
                principalTable: "Residentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
