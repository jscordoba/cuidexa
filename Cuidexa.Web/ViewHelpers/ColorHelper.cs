using System.Globalization;

namespace Cuidexa.Web.ViewHelpers;

// Deriva un tono "hover" más oscuro a partir del color de acento que elige
// el centro (Fase 8) — mismo criterio visual que ya existía fijo en site.css
// (--enlace-color #4f46e5 → --enlace-color-hover #3730a3, ~75% de brillo).
public static class ColorHelper
{
    public static string Oscurecer(string hex, double factor = 0.75)
    {
        var limpio = hex.TrimStart('#');
        if (limpio.Length != 6 || !int.TryParse(limpio, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var valor))
        {
            return hex;
        }

        var r = (int)(((valor >> 16) & 0xFF) * factor);
        var g = (int)(((valor >> 8) & 0xFF) * factor);
        var b = (int)((valor & 0xFF) * factor);

        return $"#{r:x2}{g:x2}{b:x2}";
    }
}
