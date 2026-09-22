using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cuidexa.Web.Migrations
{
    /// <inheritdoc />
    public partial class BloqueoCuentaFuerzaBruta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoHasta",
                table: "SuperAdmins",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "SuperAdmins",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoHasta",
                table: "Empleados",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "Empleados",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoHasta",
                table: "CuentasDispositivo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "CuentasDispositivo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Empleados",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "SuperAdmins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BloqueadoHasta", "IntentosFallidos" },
                values: new object[] { null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloqueadoHasta",
                table: "SuperAdmins");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "SuperAdmins");

            migrationBuilder.DropColumn(
                name: "BloqueadoHasta",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "BloqueadoHasta",
                table: "CuentasDispositivo");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "CuentasDispositivo");
        }
    }
}
