using ClosedXML.Excel;

namespace Cuidexa.Web.Services;

// Generadores de Excel de los 3 informes de la Fase 8 — funciones puras,
// una hoja por informe, misma agrupación por secciones que su PDF gemelo.
public static class InformesExcelGenerator
{
    private static void EscribirCabecera(IXLWorksheet hoja, int fila, params string[] columnas)
    {
        for (var i = 0; i < columnas.Length; i++)
        {
            var celda = hoja.Cell(fila, i + 1);
            celda.Value = columnas[i];
            celda.Style.Font.Bold = true;
        }
    }

    public static byte[] GenerarTurnosPersonal(InformeTurnosPersonal informe)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Turnos y personal");

        hoja.Cell(1, 1).Value = $"Turnos y personal · {informe.Desde:dd/MM/yyyy} - {informe.Hasta:dd/MM/yyyy}";
        hoja.Cell(1, 1).Style.Font.Bold = true;

        EscribirCabecera(hoja, 3, "Empleado", "Rol", "Horas trabajadas", "Días de ausencia", "Vacaciones disfrutadas", "Vacaciones solicitadas");

        var fila = 4;
        foreach (var f in informe.Filas)
        {
            hoja.Cell(fila, 1).Value = f.Nombre;
            hoja.Cell(fila, 2).Value = f.Rol.ToString();
            hoja.Cell(fila, 3).Value = Math.Round(f.HorasTrabajadas, 1);
            hoja.Cell(fila, 4).Value = f.DiasAusencia;
            hoja.Cell(fila, 5).Value = f.VacacionesDisfrutadas;
            hoja.Cell(fila, 6).Value = f.VacacionesSolicitadas;
            fila++;
        }

        hoja.Columns().AdjustToContents();
        return Guardar(workbook);
    }

    public static byte[] GenerarResidentesCuidados(InformeResidentesCuidados informe)
    {
        using var workbook = new XLWorkbook();

        var hojaDietas = workbook.Worksheets.Add("Dietas y patologías");
        hojaDietas.Cell(1, 1).Value = "Dietas activas (a fecha de hoy)";
        hojaDietas.Cell(1, 1).Style.Font.Bold = true;
        EscribirCabecera(hojaDietas, 2, "Dieta", "Cantidad");
        var f1 = 3;
        foreach (var (dieta, cantidad) in informe.DietasActivas)
        {
            hojaDietas.Cell(f1, 1).Value = dieta;
            hojaDietas.Cell(f1, 2).Value = cantidad;
            f1++;
        }

        f1 += 1;
        hojaDietas.Cell(f1, 1).Value = "Patologías más frecuentes (a fecha de hoy)";
        hojaDietas.Cell(f1, 1).Style.Font.Bold = true;
        f1++;
        EscribirCabecera(hojaDietas, f1, "Patología", "Cantidad");
        f1++;
        foreach (var (patologia, cantidad) in informe.PatologiasFrecuentes)
        {
            hojaDietas.Cell(f1, 1).Value = patologia;
            hojaDietas.Cell(f1, 2).Value = cantidad;
            f1++;
        }
        hojaDietas.Columns().AdjustToContents();

        var hojaCuidados = workbook.Worksheets.Add("Medicación y terapia");
        hojaCuidados.Cell(1, 1).Value = $"Medicación administrada · {informe.Desde:dd/MM/yyyy} - {informe.Hasta:dd/MM/yyyy}";
        hojaCuidados.Cell(1, 1).Style.Font.Bold = true;
        EscribirCabecera(hojaCuidados, 2, "Residente", "Medicamento", "Veces administrada");
        var f2 = 3;
        foreach (var m in informe.MedicacionAdministrada)
        {
            hojaCuidados.Cell(f2, 1).Value = m.Residente;
            hojaCuidados.Cell(f2, 2).Value = m.Medicamento;
            hojaCuidados.Cell(f2, 3).Value = m.VecesAdministrada;
            f2++;
        }

        f2 += 1;
        hojaCuidados.Cell(f2, 1).Value = "Sesiones de terapia realizadas";
        hojaCuidados.Cell(f2, 1).Style.Font.Bold = true;
        f2++;
        EscribirCabecera(hojaCuidados, f2, "Residente", "Profesional", "Especialidad", "Sesiones realizadas");
        f2++;
        foreach (var s in informe.SesionesRealizadas)
        {
            hojaCuidados.Cell(f2, 1).Value = s.Residente;
            hojaCuidados.Cell(f2, 2).Value = s.Profesional;
            hojaCuidados.Cell(f2, 3).Value = s.Especialidad;
            hojaCuidados.Cell(f2, 4).Value = s.SesionesRealizadas;
            f2++;
        }
        hojaCuidados.Columns().AdjustToContents();

        return Guardar(workbook);
    }

    public static byte[] GenerarActividadIncidencias(InformeActividadIncidencias informe)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Actividad e incidencias");

        hoja.Cell(1, 1).Value = $"Actividad e incidencias · {informe.Desde:dd/MM/yyyy} - {informe.Hasta:dd/MM/yyyy}";
        hoja.Cell(1, 1).Style.Font.Bold = true;

        EscribirCabecera(hoja, 3, "Departamento", "Avisos totales", "Urgentes");
        var fila = 4;
        foreach (var a in informe.AvisosPorDepartamento)
        {
            hoja.Cell(fila, 1).Value = a.Rol.ToString();
            hoja.Cell(fila, 2).Value = a.Total;
            hoja.Cell(fila, 3).Value = a.Urgentes;
            fila++;
        }

        fila += 1;
        hoja.Cell(fila, 1).Value = "Altas, bajas y traslados de residentes";
        hoja.Cell(fila, 1).Style.Font.Bold = true;
        fila++;
        EscribirCabecera(hoja, fila, "Acción", "Cantidad");
        fila++;
        foreach (var (accion, cantidad) in informe.TendenciaResidentes)
        {
            hoja.Cell(fila, 1).Value = accion;
            hoja.Cell(fila, 2).Value = cantidad;
            fila++;
        }

        hoja.Columns().AdjustToContents();
        return Guardar(workbook);
    }

    private static byte[] Guardar(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
