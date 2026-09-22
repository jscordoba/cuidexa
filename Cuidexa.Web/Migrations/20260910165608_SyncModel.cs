using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residentes_Habitaciones_HabitacionId",
                table: "Residentes");

            migrationBuilder.AlterColumn<int>(
                name: "HabitacionId",
                table: "Residentes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Residentes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactoEmergenciaEmail",
                table: "Residentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoEmergenciaNombre",
                table: "Residentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoEmergenciaRelacion",
                table: "Residentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoEmergenciaTelefono",
                table: "Residentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoIdentidad",
                table: "Residentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Residentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Residentes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoResidente",
                table: "Residentes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Residentes_Habitaciones_HabitacionId",
                table: "Residentes",
                column: "HabitacionId",
                principalTable: "Habitaciones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residentes_Habitaciones_HabitacionId",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "ContactoEmergenciaEmail",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "ContactoEmergenciaNombre",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "ContactoEmergenciaRelacion",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "ContactoEmergenciaTelefono",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "DocumentoIdentidad",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "TipoResidente",
                table: "Residentes");

            migrationBuilder.AlterColumn<int>(
                name: "HabitacionId",
                table: "Residentes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Residentes_Habitaciones_HabitacionId",
                table: "Residentes",
                column: "HabitacionId",
                principalTable: "Habitaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
