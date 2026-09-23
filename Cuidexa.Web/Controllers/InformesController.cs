using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuidexa.Web.Controllers;

[Authorize(Roles = "Admin,DirectorOrganizacion")]
public class InformesController : Controller
{
    private const string ContentTypeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IInformesService _informes;
    private readonly IOrganizacionService _organizacion;
    private readonly IAnalisisService _analisis;

    public InformesController(IInformesService informes, IOrganizacionService organizacion, IAnalisisService analisis)
    {
        _informes = informes;
        _organizacion = organizacion;
        _analisis = analisis;
    }

    // Rango por defecto: el mes en curso, si no se pide uno explícito.
    private static (DateOnly Desde, DateOnly Hasta) RangoODefecto(DateOnly? desde, DateOnly? hasta)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
        return (desde ?? inicioMes, hasta ?? hoy);
    }

    public async Task<IActionResult> Index(DateOnly? desde, DateOnly? hasta)
    {
        var (d, h) = RangoODefecto(desde, hasta);

        ViewBag.Desde = d;
        ViewBag.Hasta = h;
        var turnosPersonal = await _informes.ObtenerTurnosPersonalAsync(d, h);
        var residentesCuidados = await _informes.ObtenerResidentesCuidadosAsync(d, h);
        var actividadIncidencias = await _informes.ObtenerActividadIncidenciasAsync(d, h);
        ViewBag.TurnosPersonal = turnosPersonal;
        ViewBag.ResidentesCuidados = residentesCuidados;
        ViewBag.ActividadIncidencias = actividadIncidencias;

        ViewBag.ResumenEjecutivo = await _analisis.ObtenerResumenEjecutivoAsync(turnosPersonal, residentesCuidados, actividadIncidencias);
        ViewBag.PatronesIncidencias = await _analisis.DetectarPatronesIncidenciasAsync(d, h);
        ViewBag.PrediccionPersonal = await _analisis.PredecirPersonalNecesarioAsync();
        return View();
    }

    private string NombreArchivo(string informe, string extension, DateOnly desde, DateOnly hasta) =>
        $"{informe}-{desde:yyyyMMdd}-{hasta:yyyyMMdd}.{extension}";

    public async Task<IActionResult> TurnosPdf(DateOnly? desde, DateOnly? hasta)
    {
        var (d, h) = RangoODefecto(desde, hasta);
        var informe = await _informes.ObtenerTurnosPersonalAsync(d, h);
        var pdf = InformesPdfGenerator.GenerarTurnosPersonal(informe, await _organizacion.ObtenerAsync());
        return File(pdf, "application/pdf", NombreArchivo("turnos-personal", "pdf", d, h));
    }

    public async Task<IActionResult> TurnosExcel(DateOnly? desde, DateOnly? hasta)
    {
        var (d, h) = RangoODefecto(desde, hasta);
        var informe = await _informes.ObtenerTurnosPersonalAsync(d, h);
        var excel = InformesExcelGenerator.GenerarTurnosPersonal(informe);
        return File(excel, ContentTypeExcel, NombreArchivo("turnos-personal", "xlsx", d, h));
    }

    public async Task<IActionResult> ResidentesPdf(DateOnly? desde, DateOnly? hasta)
    {
        var (d, h) = RangoODefecto(desde, hasta);
        var informe = await _informes.ObtenerResidentesCuidadosAsync(d, h);
        var pdf = InformesPdfGenerator.GenerarResidentesCuidados(informe, await _organizacion.ObtenerAsync());
        return File(pdf, "application/pdf", NombreArchivo("residentes-cuidados", "pdf", d, h));
    }

    public async Task<IActionResult> ResidentesExcel(DateOnly? desde, DateOnly? hasta)
    {
        var (d, h) = RangoODefecto(desde, hasta);
        var informe = await _informes.ObtenerResidentesCuidadosAsync(d, h);
        var excel = InformesExcelGenerator.GenerarResidentesCuidados(informe);
        return File(excel, ContentTypeExcel, NombreArchivo("residentes-cuidados", "xlsx", d, h));
    }

    public async Task<IActionResult> ActividadPdf(DateOnly? desde, DateOnly? hasta)
    {
        var (d, h) = RangoODefecto(desde, hasta);
        var informe = await _informes.ObtenerActividadIncidenciasAsync(d, h);
        var pdf = InformesPdfGenerator.GenerarActividadIncidencias(informe, await _organizacion.ObtenerAsync());
        return File(pdf, "application/pdf", NombreArchivo("actividad-incidencias", "pdf", d, h));
    }

    public async Task<IActionResult> ActividadExcel(DateOnly? desde, DateOnly? hasta)
    {
        var (d, h) = RangoODefecto(desde, hasta);
        var informe = await _informes.ObtenerActividadIncidenciasAsync(d, h);
        var excel = InformesExcelGenerator.GenerarActividadIncidencias(informe);
        return File(excel, ContentTypeExcel, NombreArchivo("actividad-incidencias", "xlsx", d, h));
    }
}
