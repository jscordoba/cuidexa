using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class TurnoService : ITurnoService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;

    public TurnoService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
    }

    // Si hay un empleado destino, su propio CentroId manda siempre (un turno
    // tiene que vivir en el mismo centro que quien lo cubre) — el centroId
    // explícito solo se usa para el caso "sin empleado" (externo), y solo
    // DirectorOrganizacion lo pasa de verdad (Admin nunca, cae a su propio
    // centro de sesión).
    private async Task<int> ResolverCentroAsync(int? empleadoId, int? centroId)
    {
        if (empleadoId.HasValue)
        {
            var empleado = await _db.Empleados.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == empleadoId.Value)
                ?? throw new InvalidOperationException("Empleado no encontrado.");
            return empleado.CentroId;
        }

        if (centroId.HasValue)
        {
            var perteneceAMiOrganizacion = await _db.Centros.AnyAsync(c => c.Id == centroId.Value && c.OrganizacionId == _tenant.OrganizacionId);
            if (!perteneceAMiOrganizacion)
            {
                throw new InvalidOperationException("El centro elegido no pertenece a tu organización.");
            }
            return centroId.Value;
        }

        return _tenant.CentroId;
    }

    public async Task<List<Turno>> ObtenerTodosAsync()
    {
        return await _db.Turnos
            .Include(t => t.Empleado)
            .Include(t => t.Centro)
            .OrderByDescending(t => t.FechaInicio)
            .ToListAsync();
    }

    public async Task<Turno> CrearTurnoAsync(RolEmpleado rol, int? empleadoId, string? nombreExterno, DateTime fechaInicio, DateTime fechaFin, int? plantillaTurnoId, string? notas, int? actorId, int? centroId = null)
    {
        if (empleadoId is null && string.IsNullOrWhiteSpace(nombreExterno))
        {
            throw new InvalidOperationException("Indica un empleado o el nombre de quién cubre el turno externamente.");
        }

        var centroIdEfectivo = await ResolverCentroAsync(empleadoId, centroId);

        var turno = new Turno
        {
            CentroId = centroIdEfectivo,
            Rol = rol,
            EmpleadoId = empleadoId,
            NombreExterno = empleadoId is null ? nombreExterno : null,
            // El <input type="datetime-local"> llega sin zona horaria; ver la
            // misma trampa (y la misma solución) en ProfesionalesService.
            FechaInicio = DateTime.SpecifyKind(fechaInicio, DateTimeKind.Utc),
            FechaFin = DateTime.SpecifyKind(fechaFin, DateTimeKind.Utc),
            PlantillaTurnoId = plantillaTurnoId,
            Notas = notas
        };

        _db.Turnos.Add(turno);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "TurnoCreado", "Turno", turno.Id,
            $"Turno de {rol} el {fechaInicio:dd/MM/yyyy HH:mm}.");

        return turno;
    }

    public async Task<Turno?> ObtenerPorIdAsync(int turnoId)
    {
        return await _db.Turnos.Include(t => t.Empleado).FirstOrDefaultAsync(t => t.Id == turnoId);
    }

    public async Task EditarTurnoAsync(int turnoId, RolEmpleado rol, int? empleadoId, string? nombreExterno, DateTime fechaInicio, DateTime fechaFin, int? plantillaTurnoId, string? notas, int? actorId)
    {
        if (empleadoId is null && string.IsNullOrWhiteSpace(nombreExterno))
        {
            throw new InvalidOperationException("Indica un empleado o el nombre de quién cubre el turno externamente.");
        }

        var turno = await _db.Turnos.FindAsync(turnoId)
            ?? throw new InvalidOperationException("Turno no encontrado.");

        var fechaInicioUtc = DateTime.SpecifyKind(fechaInicio, DateTimeKind.Utc);
        var fechaFinUtc = DateTime.SpecifyKind(fechaFin, DateTimeKind.Utc);

        // Un solo registro de auditoría por edición, con los campos que
        // realmente cambiaron — evita ruido de una fila por campo y sigue el
        // mismo estilo de detalle en texto libre que el resto de acciones.
        var cambios = new List<string>();
        if (turno.Rol != rol) cambios.Add($"rol: {turno.Rol} → {rol}");
        if (turno.EmpleadoId != empleadoId) cambios.Add($"empleado: #{turno.EmpleadoId?.ToString() ?? "externo"} → #{empleadoId?.ToString() ?? "externo"}");
        if (turno.FechaInicio != fechaInicioUtc) cambios.Add($"inicio: {turno.FechaInicio:dd/MM/yyyy HH:mm} → {fechaInicioUtc:dd/MM/yyyy HH:mm}");
        if (turno.FechaFin != fechaFinUtc) cambios.Add($"fin: {turno.FechaFin:dd/MM/yyyy HH:mm} → {fechaFinUtc:dd/MM/yyyy HH:mm}");
        if (turno.Notas != notas) cambios.Add("notas actualizadas");

        turno.Rol = rol;
        turno.EmpleadoId = empleadoId;
        turno.NombreExterno = empleadoId is null ? nombreExterno : null;
        turno.FechaInicio = fechaInicioUtc;
        turno.FechaFin = fechaFinUtc;
        turno.PlantillaTurnoId = plantillaTurnoId;
        turno.Notas = notas;

        await _db.SaveChangesAsync();

        if (cambios.Any())
        {
            await _auditoria.RegistrarAsync(actorId, "TurnoEditado", "Turno", turno.Id,
                $"Cambios: {string.Join("; ", cambios)}.");
        }
    }

    public async Task<List<TurnoEfectivo>> ObtenerTurnosEfectivosAsync(DateOnly desde, DateOnly hasta, RolEmpleado? filtroRol)
    {
        var empleadosQuery = _db.Empleados
            .Include(e => e.PlantillaTurnoDefecto)
            .Where(e => e.Activo && e.PlantillaTurnoDefectoId != null);
        if (filtroRol.HasValue)
        {
            empleadosQuery = empleadosQuery.Where(e => e.Rol == filtroRol.Value);
        }
        var empleados = await empleadosQuery.ToListAsync();

        var desdeUtc = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var hastaUtc = DateTime.SpecifyKind(hasta.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        var excepciones = await _db.Turnos
            .Include(t => t.Empleado)
            .Where(t => t.Estado != EstadoTurno.Cancelado && t.FechaInicio >= desdeUtc && t.FechaInicio <= hastaUtc)
            .ToListAsync();

        var resultado = new List<TurnoEfectivo>();

        for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
        {
            var excepcionesDelDia = excepciones.Where(t => DateOnly.FromDateTime(t.FechaInicio) == fecha).ToList();

            foreach (var empleado in empleados)
            {
                var comoTitular = excepcionesDelDia.FirstOrDefault(t => t.EmpleadoId == empleado.Id);
                if (comoTitular is not null)
                {
                    resultado.Add(new TurnoEfectivo
                    {
                        EmpleadoId = empleado.Id,
                        EmpleadoNombre = empleado.Nombre,
                        Rol = comoTitular.Rol,
                        Fecha = fecha,
                        HoraInicio = TimeOnly.FromDateTime(comoTitular.FechaInicio),
                        HoraFin = TimeOnly.FromDateTime(comoTitular.FechaFin),
                        EsExcepcion = true,
                        TipoExcepcion = comoTitular.EsReemplazo ? "Reemplazo" : comoTitular.EsCambio ? "Cambio" : "Turno especial",
                        Notas = comoTitular.MotivoReemplazo ?? comoTitular.Notas
                    });
                    continue;
                }

                var cedido = excepcionesDelDia.FirstOrDefault(t => t.EmpleadoOriginalId == empleado.Id);
                if (cedido is not null)
                {
                    resultado.Add(new TurnoEfectivo
                    {
                        EmpleadoId = empleado.Id,
                        EmpleadoNombre = empleado.Nombre,
                        Rol = empleado.Rol,
                        Fecha = fecha,
                        Cedido = true,
                        CedidoA = cedido.Empleado?.Nombre ?? cedido.NombreExterno ?? "otro empleado"
                    });
                    continue;
                }

                resultado.Add(new TurnoEfectivo
                {
                    EmpleadoId = empleado.Id,
                    EmpleadoNombre = empleado.Nombre,
                    Rol = empleado.Rol,
                    Fecha = fecha,
                    HoraInicio = empleado.PlantillaTurnoDefecto!.HoraInicio,
                    HoraFin = empleado.PlantillaTurnoDefecto!.HoraFin,
                    EsExcepcion = false
                });
            }

            // Turnos cubiertos por personal externo (sin ficha de empleado):
            // no tienen fila de "empleado" que recorrer arriba, se listan
            // aparte para no perder el dato.
            foreach (var externo in excepcionesDelDia.Where(t => t.EmpleadoId is null))
            {
                if (filtroRol.HasValue && externo.Rol != filtroRol.Value) continue;

                resultado.Add(new TurnoEfectivo
                {
                    EmpleadoId = null,
                    EmpleadoNombre = externo.NombreExterno ?? "Cubierto externamente",
                    Rol = externo.Rol,
                    Fecha = fecha,
                    HoraInicio = TimeOnly.FromDateTime(externo.FechaInicio),
                    HoraFin = TimeOnly.FromDateTime(externo.FechaFin),
                    EsExcepcion = true,
                    TipoExcepcion = externo.EsReemplazo ? "Reemplazo" : externo.EsCambio ? "Cambio" : "Turno especial",
                    Notas = externo.MotivoReemplazo ?? externo.Notas
                });
            }
        }

        return resultado;
    }

    public async Task<Turno> CrearReemplazoAsync(RolEmpleado rol, int empleadoDestinoId, DateTime fechaInicio, DateTime fechaFin, string motivo, string? ausente, int? actorId)
    {
        var centroIdEfectivo = await ResolverCentroAsync(empleadoDestinoId, null);

        var turno = new Turno
        {
            CentroId = centroIdEfectivo,
            Rol = rol,
            EmpleadoId = empleadoDestinoId,
            FechaInicio = DateTime.SpecifyKind(fechaInicio, DateTimeKind.Utc),
            FechaFin = DateTime.SpecifyKind(fechaFin, DateTimeKind.Utc),
            EsReemplazo = true,
            MotivoReemplazo = motivo,
            Notas = string.IsNullOrWhiteSpace(ausente) ? null : $"Cubre ausencia de: {ausente}"
        };

        _db.Turnos.Add(turno);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "ReemplazoAsignado", "Turno", turno.Id,
            $"Reemplazo de {rol} asignado a Empleado #{empleadoDestinoId} el {fechaInicio:dd/MM/yyyy HH:mm}. Motivo: {motivo}");

        return turno;
    }

    public async Task<List<Turno>> ObtenerReemplazosAsync()
    {
        return await _db.Turnos
            .Include(t => t.Empleado)
            .Where(t => t.EsReemplazo)
            .OrderByDescending(t => t.FechaInicio)
            .ToListAsync();
    }

    public async Task CancelarTurnoAsync(int turnoId, int? actorId)
    {
        var turno = await _db.Turnos.FindAsync(turnoId)
            ?? throw new InvalidOperationException("Turno no encontrado.");

        turno.Estado = EstadoTurno.Cancelado;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "TurnoCancelado", "Turno", turno.Id,
            $"Turno del {turno.FechaInicio:dd/MM/yyyy HH:mm} cancelado.");
    }

    public async Task<List<CambioTurno>> ObtenerCambiosPendientesAsync()
    {
        return await _db.CambiosTurno
            .Include(c => c.EmpleadoSolicitante)
            .Include(c => c.EmpleadoSustitutoPropuesto)
            .Where(c => c.Estado == EstadoCambioTurno.Pendiente)
            .OrderByDescending(c => c.FechaSolicitud)
            .ToListAsync();
    }

    public async Task AprobarCambioAsync(int cambioId, int empleadoSustitutoId, int? actorId)
    {
        var cambio = await _db.CambiosTurno
            .Include(c => c.EmpleadoSolicitante).ThenInclude(e => e!.PlantillaTurnoDefecto)
            .FirstOrDefaultAsync(c => c.Id == cambioId)
            ?? throw new InvalidOperationException("Solicitud no encontrada.");

        if (cambio.Estado != EstadoCambioTurno.Pendiente)
        {
            throw new InvalidOperationException("Esta solicitud ya fue resuelta.");
        }

        var sustituto = await _db.Empleados.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == empleadoSustitutoId)
            ?? throw new InvalidOperationException("Empleado sustituto no encontrado.");

        if (sustituto.Rol != cambio.Rol)
        {
            throw new InvalidOperationException("El sustituto debe ser del mismo departamento que el solicitante.");
        }

        if (sustituto.CentroId != cambio.EmpleadoSolicitante!.CentroId)
        {
            throw new InvalidOperationException("El sustituto debe ser del mismo centro que el solicitante.");
        }

        var plantilla = cambio.EmpleadoSolicitante?.PlantillaTurnoDefecto
            ?? throw new InvalidOperationException("El solicitante no tiene un turno por defecto configurado.");

        // Igual que con turnos nocturnos creados a mano: si la franja cruza
        // medianoche (hora fin <= hora inicio), la fecha de fin es el día
        // siguiente al solicitado.
        var fechaFinDia = plantilla.HoraFin <= plantilla.HoraInicio ? cambio.Fecha.AddDays(1) : cambio.Fecha;

        var turno = new Turno
        {
            CentroId = sustituto.CentroId,
            Rol = cambio.Rol,
            EmpleadoId = empleadoSustitutoId,
            FechaInicio = DateTime.SpecifyKind(cambio.Fecha.ToDateTime(plantilla.HoraInicio), DateTimeKind.Utc),
            FechaFin = DateTime.SpecifyKind(fechaFinDia.ToDateTime(plantilla.HoraFin), DateTimeKind.Utc),
            EsCambio = true,
            EmpleadoOriginalId = cambio.EmpleadoSolicitanteId,
            PlantillaTurnoId = plantilla.Id,
            MotivoReemplazo = cambio.Motivo
        };
        _db.Turnos.Add(turno);

        cambio.Estado = EstadoCambioTurno.Aprobado;
        cambio.FechaResolucion = DateTime.UtcNow;
        cambio.ResueltoPorId = actorId;

        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "CambioAprobado", "Turno", turno.Id,
            $"Cambio de turno del {cambio.Fecha:dd/MM/yyyy} aprobado: Empleado #{empleadoSustitutoId} cubre en lugar de Empleado #{cambio.EmpleadoSolicitanteId}.");
    }

    public async Task RechazarCambioAsync(int cambioId, int? actorId)
    {
        var cambio = await _db.CambiosTurno.FindAsync(cambioId)
            ?? throw new InvalidOperationException("Solicitud no encontrada.");

        cambio.Estado = EstadoCambioTurno.Rechazado;
        cambio.FechaResolucion = DateTime.UtcNow;
        cambio.ResueltoPorId = actorId;

        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "CambioRechazado", "CambioTurno", cambio.Id, "Cambio de turno rechazado.");
    }

    // El sustituto solo puede ser del mismo departamento (rol) — decisión
    // explícita del usuario, revierte una ampliación anterior a cualquier rol.
    public async Task<List<Empleado>> ObtenerCompanerosAsync(RolEmpleado rolPropio, int excluirEmpleadoId)
    {
        return await _db.Empleados
            .Where(e => e.Rol == rolPropio && e.Activo && e.Id != excluirEmpleadoId)
            .OrderBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task<List<Turno>> ObtenerTurnosEspecialesAsync(int empleadoId)
    {
        var hoyUtc = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
        return await _db.Turnos
            .Include(t => t.Empleado)
            .Include(t => t.EmpleadoOriginal)
            .Where(t => (t.EmpleadoId == empleadoId || t.EmpleadoOriginalId == empleadoId)
                && t.Estado != EstadoTurno.Cancelado && t.FechaInicio >= hoyUtc)
            .OrderBy(t => t.FechaInicio)
            .ToListAsync();
    }

    public async Task<List<CambioTurno>> ObtenerCambiosDeAsync(int empleadoId)
    {
        return await _db.CambiosTurno
            .Include(c => c.EmpleadoSustitutoPropuesto)
            .Where(c => c.EmpleadoSolicitanteId == empleadoId)
            .OrderByDescending(c => c.FechaSolicitud)
            .ToListAsync();
    }

    public async Task SolicitarCambioAsync(DateOnly fecha, int empleadoSolicitanteId, string motivo, int? empleadoSustitutoPropuestoId)
    {
        if (fecha < DateOnly.FromDateTime(DateTime.Today))
        {
            throw new InvalidOperationException("No puedes solicitar un cambio para una fecha pasada.");
        }

        var solicitante = await _db.Empleados.FindAsync(empleadoSolicitanteId)
            ?? throw new InvalidOperationException("Empleado no encontrado.");

        if (solicitante.PlantillaTurnoDefectoId is null)
        {
            throw new InvalidOperationException("No tienes un turno por defecto configurado; contacta con Admin.");
        }

        if (empleadoSustitutoPropuestoId.HasValue)
        {
            var sustituto = await _db.Empleados.FindAsync(empleadoSustitutoPropuestoId.Value);
            if (sustituto is null || sustituto.Rol != solicitante.Rol)
            {
                throw new InvalidOperationException("El sustituto propuesto debe ser de tu mismo departamento.");
            }
        }

        var yaExiste = await _db.CambiosTurno.AnyAsync(c =>
            c.EmpleadoSolicitanteId == empleadoSolicitanteId && c.Fecha == fecha && c.Estado == EstadoCambioTurno.Pendiente);
        if (yaExiste)
        {
            throw new InvalidOperationException("Ya tienes una solicitud pendiente para esa fecha.");
        }

        var cambio = new CambioTurno
        {
            CentroId = _tenant.CentroId,
            Fecha = fecha,
            Rol = solicitante.Rol,
            EmpleadoSolicitanteId = empleadoSolicitanteId,
            Motivo = motivo,
            EmpleadoSustitutoPropuestoId = empleadoSustitutoPropuestoId
        };
        _db.CambiosTurno.Add(cambio);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoSolicitanteId, "CambioSolicitado", "CambioTurno", cambio.Id,
            $"Cambio solicitado para el {fecha:dd/MM/yyyy}: {motivo}");
    }
}
