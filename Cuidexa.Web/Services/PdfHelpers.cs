using Cuidexa.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Cuidexa.Web.Services;

// Piezas de PDF compartidas por todos los generadores (ficha de residente,
// informes) — así la marca de la organización (nombre/logo/color, Fase 8,
// pasada a Organizacion en la Fase 9) se aplica en un único sitio en vez de
// repetirse en cada documento.
public static class PdfHelpers
{
    public static void Cabecera(ColumnDescriptor col, Organizacion organizacion, string titulo, string? subtitulo)
    {
        var nombreCentro = string.IsNullOrWhiteSpace(organizacion.Nombre) ? "Cuidexa" : organizacion.Nombre;
        var colorAcento = string.IsNullOrWhiteSpace(organizacion.ColorAcento) ? "#4f46e5" : organizacion.ColorAcento;

        col.Item().Row(row =>
        {
            if (!string.IsNullOrWhiteSpace(organizacion.LogoRuta))
            {
                var rutaLogo = Path.Combine(AppContext.BaseDirectory, "wwwroot", organizacion.LogoRuta.TrimStart('/'));
                if (File.Exists(rutaLogo) && Path.GetExtension(rutaLogo).ToLowerInvariant() != ".svg")
                {
                    row.ConstantItem(40).Image(File.ReadAllBytes(rutaLogo)).FitArea();
                    row.ConstantItem(8);
                }
            }

            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"{nombreCentro} · {titulo}").FontSize(18).Bold().FontColor("#101a30");
                if (!string.IsNullOrWhiteSpace(subtitulo))
                {
                    c.Item().Text(subtitulo).FontSize(14).SemiBold();
                }
                c.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });

        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
    }

    public static void Seccion(IContainer container, string titulo, string colorAcento, Action<ColumnDescriptor> contenido)
    {
        container.Column(col =>
        {
            col.Item().Text(titulo).FontSize(12).Bold().FontColor(colorAcento);
            col.Item().PaddingBottom(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);
            col.Item().PaddingLeft(5).Column(contenido);
        });
    }

    public static void Pie(IContainer container, Organizacion organizacion)
    {
        var nombreCentro = string.IsNullOrWhiteSpace(organizacion.Nombre) ? "Cuidexa" : organizacion.Nombre;
        container.AlignCenter().Text(x =>
        {
            x.Span($"Documento generado por {nombreCentro} — confidencial, contiene datos de salud.").FontSize(7).FontColor(Colors.Grey.Medium);
        });
    }
}
