using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.ViewHelpers;

namespace Cuidexa.Web.Services;

// Consume calendariosnacionales.com: API pública y gratuita (sin clave) con
// el calendario laboral de España por comunidad autónoma, agregado a partir
// de fuentes oficiales (BOE y boletines autonómicos) — no es una API del
// gobierno, pero es la única opción gratuita que cubre las 19 comunidades
// en un formato consistente (ver decisión con el usuario, 2026-09-17).
public class FestivosApiService : IFestivosApiService
{
    private static readonly JsonSerializerOptions JsonOpciones = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _http;

    public FestivosApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<(DateOnly Fecha, string Nombre)>> ObtenerFestivosAsync(int anio, ComunidadAutonoma comunidad)
    {
        var slug = ComunidadPresentacion.Obtener(comunidad).SlugApi;
        var url = $"https://calendariosnacionales.com/es/v1/{anio}/regiones/{slug}.json";

        HttpResponseMessage respuesta;
        try
        {
            respuesta = await _http.GetAsync(url);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new InvalidOperationException("No se pudo conectar con el servicio de calendario laboral. Inténtalo de nuevo más tarde.", ex);
        }

        if (!respuesta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"El servicio de calendario laboral respondió con un error (código {(int)respuesta.StatusCode}).");
        }

        RespuestaFestivos? datos;
        try
        {
            datos = await respuesta.Content.ReadFromJsonAsync<RespuestaFestivos>(JsonOpciones);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("La respuesta del servicio de calendario laboral no tiene el formato esperado.", ex);
        }

        var items = (datos?.Holidays?.National ?? new())
            .Concat(datos?.Holidays?.Regional ?? new());

        // Dedupe por fecha: nacional y autonómico no deberían solaparse,
        // pero si lo hicieran, el nombre nacional prevalece.
        var porFecha = new Dictionary<DateOnly, string>();
        foreach (var item in items)
        {
            if (DateOnly.TryParseExact(item.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            {
                porFecha.TryAdd(fecha, item.Name);
            }
        }

        if (porFecha.Count == 0)
        {
            throw new InvalidOperationException("El servicio de calendario laboral no devolvió festivos para esa comunidad y año.");
        }

        return porFecha.Select(kv => (kv.Key, kv.Value)).OrderBy(x => x.Key).ToList();
    }

    private class RespuestaFestivos
    {
        public HolidaysWrapper? Holidays { get; set; }
    }

    private class HolidaysWrapper
    {
        public List<FestivoItem>? National { get; set; }
        public List<FestivoItem>? Regional { get; set; }
    }

    private class FestivoItem
    {
        public string Date { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
