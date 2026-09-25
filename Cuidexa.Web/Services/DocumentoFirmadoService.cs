using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class DocumentoFirmadoService : IDocumentoFirmadoService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;
    private readonly IPushNotificationService _push;

    public DocumentoFirmadoService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant, IPushNotificationService push)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
        _push = push;
    }

    public async Task<List<DocumentoFirmado>> ObtenerPorResidenteAsync(int residenteId)
    {
        return await _db.DocumentosFirmados
            .Include(d => d.EmpleadoRegistra)
            .Where(d => d.ResidenteId == residenteId)
            .OrderByDescending(d => d.FechaFirma)
            .ToListAsync();
    }

    public async Task<List<DocumentoFirmado>> ObtenerPorIncidenciaAsync(int incidenciaId)
    {
        return await _db.DocumentosFirmados
            .Include(d => d.EmpleadoRegistra)
            .Where(d => d.IncidenciaId == incidenciaId)
            .OrderByDescending(d => d.FechaFirma)
            .ToListAsync();
    }

    public async Task<List<DocumentoFirmado>> ObtenerTodosAsync()
    {
        return await _db.DocumentosFirmados
            .Include(d => d.Residente)
            .Include(d => d.Incidencia)
            .Include(d => d.EmpleadoRegistra)
            .OrderByDescending(d => d.FechaFirma)
            .ToListAsync();
    }

    public async Task CrearAsync(CategoriaDocumentoFirmado categoria, string titulo, string descripcion,
        int? residenteId, int? incidenciaId, string firmanteNombre, string firmanteRelacion,
        string firmaImagenBase64, int? empleadoRegistraId)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
        {
            throw new InvalidOperationException("Título y descripción son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(firmanteNombre))
        {
            throw new InvalidOperationException("Indica el nombre de quien firma.");
        }

        if (string.IsNullOrWhiteSpace(firmaImagenBase64))
        {
            throw new InvalidOperationException("Falta capturar la firma.");
        }

        if (residenteId.HasValue && !await _db.Residentes.AnyAsync(r => r.Id == residenteId.Value))
        {
            throw new InvalidOperationException("Residente no encontrado.");
        }

        if (incidenciaId.HasValue && !await _db.Incidencias.AnyAsync(i => i.Id == incidenciaId.Value))
        {
            throw new InvalidOperationException("Incidencia no encontrada.");
        }

        var documento = new DocumentoFirmado
        {
            CentroId = _tenant.CentroId,
            Categoria = categoria,
            Titulo = titulo,
            Descripcion = descripcion,
            ResidenteId = residenteId,
            IncidenciaId = incidenciaId,
            FirmanteNombre = firmanteNombre,
            FirmanteRelacion = firmanteRelacion,
            FirmaImagenBase64 = firmaImagenBase64,
            EmpleadoRegistraId = empleadoRegistraId
        };
        _db.DocumentosFirmados.Add(documento);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoRegistraId, "Alta", "DocumentoFirmado", documento.Id,
            $"Documento firmado ({categoria}): {titulo} — firmado por {firmanteNombre}" +
            (string.IsNullOrWhiteSpace(firmanteRelacion) ? "" : $" ({firmanteRelacion})"));

        // residenteId puede venir vacío si el documento se ligó solo a una
        // incidencia (ver CrearAsync) — en ese caso el residente real es el
        // de la propia incidencia, que es a quien hay que avisar.
        var residenteParaAvisar = residenteId
            ?? (incidenciaId.HasValue ? (await _db.Incidencias.FindAsync(incidenciaId.Value))?.ResidenteId : null);
        if (residenteParaAvisar.HasValue)
        {
            await _push.NotificarFamiliarAsync(residenteParaAvisar.Value,
                "Cuidexa · Nuevo documento", $"Se ha añadido un nuevo documento: {titulo}");
        }
    }
}
