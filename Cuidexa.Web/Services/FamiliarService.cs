using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class FamiliarService : IFamiliarService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;

    public FamiliarService(CuidexaDbContext db, IAuditService auditoria)
    {
        _db = db;
        _auditoria = auditoria;
    }

    public async Task<List<Familiar>> ObtenerPorResidenteAsync(int residenteId) =>
        await _db.Familiares.Where(f => f.ResidenteId == residenteId)
            .OrderBy(f => f.Nombre)
            .ToListAsync();

    public async Task CrearAsync(int residenteId, string nombre, string relacion, string email, string password, int? actorEmpleadoId)
    {
        var residente = await _db.Residentes.FindAsync(residenteId)
            ?? throw new InvalidOperationException("Residente no encontrado.");

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("El nombre y el email son obligatorios.");
        }

        // Email único a nivel global (sin filtro de tenant en Familiar): el
        // login del familiar no pide código de centro, así que el email es
        // el único discriminador posible al autenticar.
        if (await _db.Familiares.AnyAsync(f => f.Email == email))
        {
            throw new InvalidOperationException("Ya existe un familiar registrado con ese email.");
        }

        PasswordHasher.Validar(password);

        var familiar = new Familiar
        {
            ResidenteId = residenteId,
            Nombre = nombre.Trim(),
            Relacion = relacion?.Trim() ?? string.Empty,
            Email = email.Trim(),
            PasswordHash = PasswordHasher.Hash(password)
        };

        _db.Familiares.Add(familiar);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorEmpleadoId, "FamiliarInvitado", "Residente", residenteId,
            $"Acceso de portal creado para {familiar.Nombre} ({familiar.Relacion}), email {familiar.Email}.");
    }

    // IgnoreQueryFilters + Where(ResidenteId == ...) explícito: el esquema
    // "Familiar" no tiene CentroId de sesión (igual que SuperAdmin/
    // Dispositivo), así que el filtro global de estas dos entidades
    // (pensado para el personal, por Centro) no aplica aquí — el
    // aislamiento real de un familiar es más estrecho que un Centro: un
    // único Residente.
    public async Task<List<DocumentoFirmado>> ObtenerDocumentosAsync(int residenteId) =>
        await _db.DocumentosFirmados.IgnoreQueryFilters()
            .Where(d => d.ResidenteId == residenteId)
            .OrderByDescending(d => d.FechaFirma)
            .ToListAsync();

    public async Task<List<Incidencia>> ObtenerIncidenciasResueltasAsync(int residenteId) =>
        await _db.Incidencias.IgnoreQueryFilters()
            .Where(i => i.ResidenteId == residenteId && i.Estado == EstadoIncidencia.Resuelta && i.Caracter == CaracterIncidencia.RequiereSeguimiento)
            .OrderByDescending(i => i.FechaResolucion)
            .ToListAsync();
}
