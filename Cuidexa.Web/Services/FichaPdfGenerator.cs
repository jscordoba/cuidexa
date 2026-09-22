using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Cuidexa.Web.Services;

// Genera el PDF de la ficha completa de un residente. Función de
// renderizado pura (no toca la base de datos): recibe el Residente ya
// cargado con todas sus relaciones vía IResidenteService.ObtenerFichaCompletaAsync,
// igual que Details.cshtml — así el PDF y la pantalla nunca muestran datos
// distintos entre sí.
public static class FichaPdfGenerator
{
    public static byte[] Generar(Residente residente, Organizacion organizacion)
    {
        var colorAcento = string.IsNullOrWhiteSpace(organizacion.ColorAcento) ? "#4f46e5" : organizacion.ColorAcento;
        var dietaActual = residente.Dietas.FirstOrDefault(d => d.FechaFin == null)?.Dieta?.Nombre ?? "-";
        var alergias = string.Join(", ", residente.Alergias.Select(a => a.Alergia?.Nombre));
        var medicacionActiva = residente.Medicaciones.Where(m => m.FechaFin == null).OrderBy(m => m.Nombre).ToList();
        var sesionesProgramadas = residente.SesionesTerapia.Where(s => s.Estado == EstadoSesion.Programada).OrderBy(s => s.FechaHora).ToList();
        var sesionesPasadas = residente.SesionesTerapia.Where(s => s.Estado != EstadoSesion.Programada)
            .OrderByDescending(s => s.FechaHora).Take(10).ToList();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col => PdfHelpers.Cabecera(col, organizacion, "Ficha de residente", $"{residente.Nombre} {residente.Apellidos}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Element(c => PdfHelpers.Seccion(c, "Datos generales", colorAcento, datos =>
                    {
                        datos.Item().Text($"Tipo: {residente.TipoResidente}    Habitación: {residente.Habitacion?.Numero ?? "Sin asignar"}    Estado: {residente.Estado}");
                        datos.Item().Text($"Fecha de nacimiento: {residente.FechaNacimiento}    Fecha de ingreso: {residente.FechaIngreso}");
                    }));

                    col.Item().Element(c => PdfHelpers.Seccion(c, "Cocina", colorAcento, datos =>
                    {
                        datos.Item().Text($"Dieta actual: {dietaActual}");
                        datos.Item().Text($"Alergias: {(string.IsNullOrEmpty(alergias) ? "Ninguna registrada" : alergias)}");
                    }));

                    col.Item().Element(c => PdfHelpers.Seccion(c, "Auxiliares", colorAcento, datos =>
                    {
                        datos.Item().Text($"Movilidad: {residente.Movilidad}");
                        datos.Item().Text($"Necesita ayuda para levantarse: {(residente.NecesitaAyudaLevantarse ? "Sí" : "No")}");
                        datos.Item().Text($"Equipamiento especial: {(string.IsNullOrWhiteSpace(residente.EquipamientoEspecial) ? "Ninguno" : residente.EquipamientoEspecial)}");
                    }));

                    col.Item().Element(c => PdfHelpers.Seccion(c, "Enfermería", colorAcento, datos =>
                    {
                        datos.Item().Text("Patologías:").SemiBold();
                        if (!residente.Patologias.Any())
                        {
                            datos.Item().Text("Ninguna registrada.");
                        }
                        foreach (var p in residente.Patologias)
                        {
                            datos.Item().Text($"- {p.Patologia?.Nombre}{(string.IsNullOrWhiteSpace(p.Observaciones) ? "" : $": {p.Observaciones}")}");
                        }

                        datos.Item().PaddingTop(5).Text("Medicación activa:").SemiBold();
                        if (!medicacionActiva.Any())
                        {
                            datos.Item().Text("Ninguna.");
                        }
                        foreach (var m in medicacionActiva)
                        {
                            datos.Item().Text($"- {m.Nombre} ({m.Dosis}, {m.Horario}){(string.IsNullOrWhiteSpace(m.Instrucciones) ? "" : $" — {m.Instrucciones}")}");
                        }
                    }));

                    col.Item().Element(c => PdfHelpers.Seccion(c, "Profesionales", colorAcento, datos =>
                    {
                        datos.Item().Text("Próximas sesiones:").SemiBold();
                        if (!sesionesProgramadas.Any())
                        {
                            datos.Item().Text("Ninguna programada.");
                        }
                        foreach (var s in sesionesProgramadas)
                        {
                            datos.Item().Text($"- {s.FechaHora:dd/MM/yyyy HH:mm} · {s.Empleado?.Nombre} ({s.Empleado?.Especialidad?.Nombre ?? "Sin especialidad"})");
                        }

                        if (sesionesPasadas.Any())
                        {
                            datos.Item().PaddingTop(5).Text("Últimas sesiones:").SemiBold();
                            foreach (var s in sesionesPasadas)
                            {
                                datos.Item().Text($"- {s.FechaHora:dd/MM/yyyy HH:mm} · {s.Empleado?.Nombre} · {s.Estado}{(string.IsNullOrWhiteSpace(s.Observaciones) ? "" : $" — {s.Observaciones}")}");
                            }
                        }
                    }));
                });

                page.Footer().Element(c => PdfHelpers.Pie(c, organizacion));
            });
        });

        return documento.GeneratePdf();
    }
}
