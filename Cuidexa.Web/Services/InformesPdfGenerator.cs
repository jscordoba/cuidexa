using Cuidexa.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Cuidexa.Web.Services;

// Generadores de PDF de los 3 informes de la Fase 8 — mismo patrón que
// FichaPdfGenerator (funciones puras de renderizado, sin acceso a BD),
// reutilizando PdfHelpers para que la marca del centro sea consistente en
// todos los documentos de la app.
public static class InformesPdfGenerator
{
    private static byte[] Documento(Organizacion organizacion, string titulo, DateOnly desde, DateOnly hasta, Action<ColumnDescriptor> contenido)
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().Column(col => PdfHelpers.Cabecera(col, organizacion, titulo, $"Del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}"));
                page.Content().PaddingVertical(10).Column(contenido);
                page.Footer().Element(c => PdfHelpers.Pie(c, organizacion));
            });
        });

        return documento.GeneratePdf();
    }

    public static byte[] GenerarTurnosPersonal(InformeTurnosPersonal informe, Organizacion organizacion)
    {
        var colorAcento = string.IsNullOrWhiteSpace(organizacion.ColorAcento) ? "#4f46e5" : organizacion.ColorAcento;

        return Documento(organizacion, "Turnos y personal", informe.Desde, informe.Hasta, col =>
        {
            col.Spacing(12);
            col.Item().Element(c => PdfHelpers.Seccion(c, "Horas, ausencias y vacaciones por empleado", colorAcento, datos =>
            {
                if (!informe.Filas.Any())
                {
                    datos.Item().Text("Sin turnos en el rango.");
                    return;
                }
                datos.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Empleado").Bold();
                        h.Cell().Text("Rol").Bold();
                        h.Cell().Text("Horas").Bold();
                        h.Cell().Text("Ausencias").Bold();
                        h.Cell().Text("Vac. disfrutadas").Bold();
                        h.Cell().Text("Vac. solicitadas").Bold();
                    });
                    foreach (var f in informe.Filas)
                    {
                        table.Cell().Text(f.Nombre);
                        table.Cell().Text(f.Rol.ToString());
                        table.Cell().Text(f.HorasTrabajadas.ToString("0.0"));
                        table.Cell().Text(f.DiasAusencia.ToString());
                        table.Cell().Text(f.VacacionesDisfrutadas.ToString());
                        table.Cell().Text(f.VacacionesSolicitadas.ToString());
                    }
                });
            }));
        });
    }

    public static byte[] GenerarResidentesCuidados(InformeResidentesCuidados informe, Organizacion organizacion)
    {
        var colorAcento = string.IsNullOrWhiteSpace(organizacion.ColorAcento) ? "#4f46e5" : organizacion.ColorAcento;

        return Documento(organizacion, "Residentes y cuidados", informe.Desde, informe.Hasta, col =>
        {
            col.Spacing(12);

            col.Item().Element(c => PdfHelpers.Seccion(c, "Dietas activas (a fecha de hoy)", colorAcento, datos =>
            {
                foreach (var (dieta, cantidad) in informe.DietasActivas)
                {
                    datos.Item().Text($"{dieta}: {cantidad}");
                }
            }));

            col.Item().Element(c => PdfHelpers.Seccion(c, "Patologías más frecuentes (a fecha de hoy)", colorAcento, datos =>
            {
                foreach (var (patologia, cantidad) in informe.PatologiasFrecuentes)
                {
                    datos.Item().Text($"{patologia}: {cantidad}");
                }
            }));

            col.Item().Element(c => PdfHelpers.Seccion(c, "Medicación administrada en el rango", colorAcento, datos =>
            {
                if (!informe.MedicacionAdministrada.Any())
                {
                    datos.Item().Text("Sin registros en el rango.");
                }
                foreach (var m in informe.MedicacionAdministrada)
                {
                    datos.Item().Text($"{m.Residente} — {m.Medicamento}: {m.VecesAdministrada} veces");
                }
            }));

            col.Item().Element(c => PdfHelpers.Seccion(c, "Sesiones de terapia realizadas en el rango", colorAcento, datos =>
            {
                if (!informe.SesionesRealizadas.Any())
                {
                    datos.Item().Text("Sin registros en el rango.");
                }
                foreach (var s in informe.SesionesRealizadas)
                {
                    datos.Item().Text($"{s.Residente} — {s.Profesional} ({s.Especialidad}): {s.SesionesRealizadas} sesiones");
                }
            }));
        });
    }

    public static byte[] GenerarActividadIncidencias(InformeActividadIncidencias informe, Organizacion organizacion)
    {
        var colorAcento = string.IsNullOrWhiteSpace(organizacion.ColorAcento) ? "#4f46e5" : organizacion.ColorAcento;

        return Documento(organizacion, "Actividad e incidencias", informe.Desde, informe.Hasta, col =>
        {
            col.Spacing(12);

            col.Item().Element(c => PdfHelpers.Seccion(c, "Avisos por departamento", colorAcento, datos =>
            {
                if (!informe.AvisosPorDepartamento.Any())
                {
                    datos.Item().Text("Sin avisos en el rango.");
                }
                foreach (var f in informe.AvisosPorDepartamento)
                {
                    datos.Item().Text($"{f.Rol}: {f.Total} avisos ({f.Urgentes} urgentes)");
                }
            }));

            col.Item().Element(c => PdfHelpers.Seccion(c, "Altas, bajas y traslados de residentes", colorAcento, datos =>
            {
                if (!informe.TendenciaResidentes.Any())
                {
                    datos.Item().Text("Sin movimientos en el rango.");
                }
                foreach (var (accion, cantidad) in informe.TendenciaResidentes)
                {
                    datos.Item().Text($"{accion}: {cantidad}");
                }
            }));
        });
    }
}
