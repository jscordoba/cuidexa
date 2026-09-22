using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.ViewHelpers;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class CentroService : ICentroService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;

    public CentroService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
    }

    // Centro nunca lleva filtro global (ver CuidexaDbContext) — se acota
    // explícitamente al de la sesión actual, nunca a uno pedido por parámetro,
    // para que un Admin no pueda editar el centro de otro adivinando un id.
    public async Task<Centro> ObtenerAsync() =>
        await _db.Centros.FindAsync(_tenant.CentroId)
            ?? throw new InvalidOperationException("Centro no encontrado.");

    public async Task<List<Centro>> ObtenerCentrosDeMiOrganizacionAsync() =>
        await _db.Centros
            .Where(c => c.OrganizacionId == _tenant.OrganizacionId && c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

    public async Task ActualizarContactoAsync(string nombre, string? direccion, string? telefono, string? email,
        string? responsableNombre, string? responsableCargo, int? actorId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre del centro es obligatorio.");
        }

        var centro = await ObtenerAsync();
        centro.Nombre = nombre;
        centro.Direccion = direccion;
        centro.Telefono = telefono;
        centro.Email = email;
        centro.ResponsableNombre = responsableNombre;
        centro.ResponsableCargo = responsableCargo;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "ConfiguracionActualizada", "Centro", centro.Id,
            "Datos de contacto del centro actualizados.");
    }

    public async Task ActualizarComunidadAsync(ComunidadAutonoma? comunidad, int? actorId)
    {
        var centro = await ObtenerAsync();
        centro.ComunidadAutonoma = comunidad;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "ConfiguracionActualizada", "Centro", centro.Id,
            $"Comunidad autónoma del centro: {(comunidad.HasValue ? ComunidadPresentacion.Obtener(comunidad.Value).Etiqueta : "sin definir")}.");
    }
}
