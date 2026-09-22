using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class PlantillaTurnoService : IPlantillaTurnoService
{
    private readonly CuidexaDbContext _db;
    private readonly ITenantContext _tenant;

    public PlantillaTurnoService(CuidexaDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<List<PlantillaTurno>> ObtenerActivasAsync()
    {
        return await _db.PlantillasTurno
            .Where(p => p.Activo)
            .OrderBy(p => p.TipoDia).ThenBy(p => p.HoraInicio)
            .ToListAsync();
    }

    public async Task<List<PlantillaTurno>> ObtenerTodasAsync()
    {
        return await _db.PlantillasTurno
            .OrderBy(p => p.TipoDia).ThenBy(p => p.HoraInicio)
            .ToListAsync();
    }

    public async Task<PlantillaTurno?> ObtenerPorIdAsync(int id)
    {
        return await _db.PlantillasTurno.FindAsync(id);
    }

    public async Task CrearAsync(string nombre, TimeOnly horaInicio, TimeOnly horaFin, TipoDia tipoDia)
    {
        _db.PlantillasTurno.Add(new PlantillaTurno
        {
            OrganizacionId = _tenant.OrganizacionId,
            Nombre = nombre,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            TipoDia = tipoDia
        });
        await _db.SaveChangesAsync();
    }

    public async Task EditarAsync(int id, string nombre, TimeOnly horaInicio, TimeOnly horaFin, TipoDia tipoDia)
    {
        var plantilla = await _db.PlantillasTurno.FindAsync(id)
            ?? throw new InvalidOperationException("Plantilla no encontrada.");

        plantilla.Nombre = nombre;
        plantilla.HoraInicio = horaInicio;
        plantilla.HoraFin = horaFin;
        plantilla.TipoDia = tipoDia;
        await _db.SaveChangesAsync();
    }

    public async Task CambiarActivaAsync(int id, bool activo)
    {
        var plantilla = await _db.PlantillasTurno.FindAsync(id)
            ?? throw new InvalidOperationException("Plantilla no encontrada.");

        plantilla.Activo = activo;
        await _db.SaveChangesAsync();
    }
}
