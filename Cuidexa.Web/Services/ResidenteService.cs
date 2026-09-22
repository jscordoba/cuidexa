using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class ResidenteService : IResidenteService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly IPushNotificationService _push;
    private readonly ITenantContext _tenant;

    public ResidenteService(CuidexaDbContext db, IAuditService auditoria, IPushNotificationService push, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _push = push;
        _tenant = tenant;
    }

    public async Task<List<Residente>> ObtenerTodosAsync()
    {
        return await _db.Residentes
            .Include(r => r.Habitacion)
            .Include(r => r.Centro)
            .Include(r => r.Dietas).ThenInclude(d => d.Dieta)
            .Include(r => r.Alergias).ThenInclude(a => a.Alergia)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<Residente?> ObtenerPorIdAsync(int id)
    {
        return await _db.Residentes
            .Include(r => r.Habitacion)
            .Include(r => r.Dietas).ThenInclude(d => d.Dieta)
            .Include(r => r.Alergias).ThenInclude(a => a.Alergia)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Residente?> ObtenerFichaCompletaAsync(int id)
    {
        return await _db.Residentes
            .Include(r => r.Habitacion)
            .Include(r => r.Dietas).ThenInclude(d => d.Dieta)
            .Include(r => r.Alergias).ThenInclude(a => a.Alergia)
            .Include(r => r.Patologias).ThenInclude(p => p.Patologia)
            .Include(r => r.Medicaciones).ThenInclude(m => m.Registros).ThenInclude(reg => reg.Empleado)
            .Include(r => r.SesionesTerapia).ThenInclude(s => s.Empleado).ThenInclude(e => e!.Especialidad)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    // Este es el flujo central del MVP: dar de alta a un residente y,
    // en la misma operación, generar los eventos que Cocina y Auxiliares
    // necesitan ver. Todo dentro de una única transacción implícita de EF Core.
    public async Task<Residente> DarDeAltaAsync(ResidenteCreateDto dto, int? empleadoId, int? centroId = null)
    {
        if (!dto.EsValido(out var error))
        {
            throw new InvalidOperationException(error);
        }

        int centroIdEfectivo;
        if (centroId.HasValue)
        {
            var perteneceAMiOrganizacion = await _db.Centros.AnyAsync(c => c.Id == centroId.Value && c.OrganizacionId == _tenant.OrganizacionId);
            if (!perteneceAMiOrganizacion)
            {
                throw new InvalidOperationException("El centro elegido no pertenece a tu organización.");
            }
            centroIdEfectivo = centroId.Value;
        }
        else
        {
            centroIdEfectivo = _tenant.CentroId;
        }

        if (dto.HabitacionId.HasValue)
        {
            var habitacionValida = await _db.Habitaciones.IgnoreQueryFilters()
                .AnyAsync(h => h.Id == dto.HabitacionId.Value && h.CentroId == centroIdEfectivo);
            if (!habitacionValida)
            {
                throw new InvalidOperationException("La habitación elegida no pertenece a ese centro.");
            }
        }

        var residente = new Residente
        {
            CentroId = centroIdEfectivo,
            Nombre = dto.Nombre,
            Apellidos = dto.Apellidos,
            DocumentoIdentidad = dto.DocumentoIdentidad,
            FechaNacimiento = dto.FechaNacimiento,
            TipoResidente = dto.TipoResidente,
            HabitacionId = dto.HabitacionId,
            Movilidad = dto.Movilidad,
            NecesitaAyudaLevantarse = dto.NecesitaAyudaLevantarse,
            EquipamientoEspecial = dto.EquipamientoEspecial,
            Telefono = dto.Telefono,
            Email = dto.Email,
            ContactoEmergenciaNombre = dto.ContactoEmergenciaNombre,
            ContactoEmergenciaRelacion = dto.ContactoEmergenciaRelacion,
            ContactoEmergenciaTelefono = dto.ContactoEmergenciaTelefono,
            ContactoEmergenciaEmail = dto.ContactoEmergenciaEmail,
            Estado = EstadoResidente.Activo,
            FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _db.Residentes.Add(residente);
        await _db.SaveChangesAsync(); // necesitamos el Id generado antes de seguir

        _db.ResidenteDietas.Add(new ResidenteDieta
        {
            CentroId = centroIdEfectivo,
            ResidenteId = residente.Id,
            DietaId = dto.DietaId,
            FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),
            FechaFin = null,
            Motivo = "Alta inicial"
        });

        foreach (var alergiaId in dto.AlergiaIds)
        {
            _db.ResidenteAlergias.Add(new ResidenteAlergia
            {
                CentroId = centroIdEfectivo,
                ResidenteId = residente.Id,
                AlergiaId = alergiaId
            });
        }

        // --- Distribución automática de información ---
        var dieta = await _db.Dietas.FindAsync(dto.DietaId);
        var alergias = await _db.Alergias
            .Where(a => dto.AlergiaIds.Contains(a.Id))
            .Select(a => a.Nombre)
            .ToListAsync();

        var descripcionCocina = $"Nuevo residente: {residente.Nombre}. Dieta: {dieta?.Nombre}." +
            (alergias.Count > 0 ? $" Alergias: {string.Join(", ", alergias)}." : " Sin alergias registradas.");

        var descripcionAuxiliar = $"Nuevo residente: {residente.Nombre}. Movilidad: {residente.Movilidad}." +
            (residente.NecesitaAyudaLevantarse ? " Necesita ayuda para levantarse." : "") +
            (string.IsNullOrWhiteSpace(residente.EquipamientoEspecial) ? "" : $" Equipamiento: {residente.EquipamientoEspecial}.");

        var eventosCreados = new List<EventoDistribucion>();
        void AgregarEvento(EventoDistribucion evento)
        {
            _db.EventosDistribucion.Add(evento);
            eventosCreados.Add(evento);
        }

        AgregarEvento(new EventoDistribucion
        {
            CentroId = centroIdEfectivo,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Alta,
            RolDestino = RolEmpleado.Cocina,
            Descripcion = descripcionCocina
        });

        AgregarEvento(new EventoDistribucion
        {
            CentroId = centroIdEfectivo,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Alta,
            RolDestino = RolEmpleado.Auxiliar,
            Descripcion = descripcionAuxiliar
        });

        // A Enfermería no se le manda dieta/movilidad (eso no es de su ámbito) ni
        // patologías (Admin no las recoge en el alta a propósito — las gestiona
        // Enfermería directamente, ver Fase 2 del roadmap). Solo el aviso de que
        // hay un residente nuevo que revisar clínicamente.
        AgregarEvento(new EventoDistribucion
        {
            CentroId = centroIdEfectivo,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Alta,
            RolDestino = RolEmpleado.Enfermeria,
            Descripcion = $"Nuevo residente: {residente.Nombre} {residente.Apellidos}. Revisar patologías y medicación."
        });

        // Igual que con Enfermería: Admin no decide qué terapias necesita el
        // residente, solo avisa de que hay uno nuevo que valorar.
        AgregarEvento(new EventoDistribucion
        {
            CentroId = centroIdEfectivo,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Alta,
            RolDestino = RolEmpleado.Profesional,
            Descripcion = $"Nuevo residente: {residente.Nombre} {residente.Apellidos}. Valorar si necesita terapia."
        });

        // Si entra con habitación ya asignada, Limpieza necesita prepararla
        // antes de que llegue — no depende de que Admin lo pida explícitamente.
        if (dto.HabitacionId.HasValue)
        {
            var habitacionAlta = await _db.Habitaciones.IgnoreQueryFilters().FirstOrDefaultAsync(h => h.Id == dto.HabitacionId.Value);
            _db.TareasLimpieza.Add(new TareaLimpieza
            {
                CentroId = centroIdEfectivo,
                HabitacionId = dto.HabitacionId,
                Descripcion = $"Preparar habitación {habitacionAlta?.Numero} para nuevo residente ({residente.Nombre} {residente.Apellidos})."
            });

            AgregarEvento(new EventoDistribucion
            {
                CentroId = centroIdEfectivo,
                ResidenteId = residente.Id,
                TipoEvento = TipoEvento.Alta,
                RolDestino = RolEmpleado.Limpieza,
                Descripcion = $"Preparar habitación {habitacionAlta?.Numero} para nuevo residente."
            });
        }

        await _db.SaveChangesAsync();
        await _push.NotificarAsync(eventosCreados);

        await _auditoria.RegistrarAsync(empleadoId, "Alta", "Residente", residente.Id,
            $"Alta de {residente.Nombre} {residente.Apellidos}.");

        return residente;
    }

    public async Task DarDeBajaAsync(int residenteId, int? empleadoId, string? motivo)
    {
        var residente = await _db.Residentes.FindAsync(residenteId)
            ?? throw new InvalidOperationException("Residente no encontrado.");

        residente.Estado = EstadoResidente.Baja;

        var detalle = string.IsNullOrWhiteSpace(motivo)
            ? "Baja sin motivo especificado."
            : $"Motivo: {motivo}";

        var eventosCreados = new List<EventoDistribucion>();
        void AgregarEvento(EventoDistribucion evento)
        {
            _db.EventosDistribucion.Add(evento);
            eventosCreados.Add(evento);
        }

        AgregarEvento(new EventoDistribucion
        {
            CentroId = residente.CentroId,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Baja,
            RolDestino = RolEmpleado.Cocina,
            Descripcion = $"{residente.Nombre} {residente.Apellidos} ha sido dado de baja. Dejar de contar en el servicio."
        });

        AgregarEvento(new EventoDistribucion
        {
            CentroId = residente.CentroId,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Baja,
            RolDestino = RolEmpleado.Auxiliar,
            Descripcion = $"{residente.Nombre} {residente.Apellidos} ha sido dado de baja. Ya no requiere atención."
        });

        AgregarEvento(new EventoDistribucion
        {
            CentroId = residente.CentroId,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Baja,
            RolDestino = RolEmpleado.Enfermeria,
            Descripcion = $"{residente.Nombre} {residente.Apellidos} ha sido dado de baja. Revisar medicación pendiente de cerrar."
        });

        AgregarEvento(new EventoDistribucion
        {
            CentroId = residente.CentroId,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Baja,
            RolDestino = RolEmpleado.Profesional,
            Descripcion = $"{residente.Nombre} {residente.Apellidos} ha sido dado de baja. Cancelar sesiones pendientes."
        });

        if (residente.HabitacionId.HasValue)
        {
            var habitacionBaja = await _db.Habitaciones.IgnoreQueryFilters().FirstOrDefaultAsync(h => h.Id == residente.HabitacionId.Value);
            _db.TareasLimpieza.Add(new TareaLimpieza
            {
                CentroId = residente.CentroId,
                HabitacionId = residente.HabitacionId,
                Descripcion = $"Limpiar habitación {habitacionBaja?.Numero} (vacada tras baja de {residente.Nombre} {residente.Apellidos})."
            });

            AgregarEvento(new EventoDistribucion
            {
                CentroId = residente.CentroId,
                ResidenteId = residente.Id,
                TipoEvento = TipoEvento.Baja,
                RolDestino = RolEmpleado.Limpieza,
                Descripcion = $"Habitación {habitacionBaja?.Numero} vacada, lista para limpieza a fondo."
            });
        }

        await _db.SaveChangesAsync();
        await _push.NotificarAsync(eventosCreados);

        await _auditoria.RegistrarAsync(empleadoId, "Baja", "Residente", residente.Id, detalle);
    }

    public async Task TrasladarAsync(int residenteId, int nuevaHabitacionId, int? empleadoId)
    {
        var residente = await _db.Residentes.Include(r => r.Habitacion).FirstOrDefaultAsync(r => r.Id == residenteId)
            ?? throw new InvalidOperationException("Residente no encontrado.");

        var nuevaHabitacion = await _db.Habitaciones.IgnoreQueryFilters().FirstOrDefaultAsync(h => h.Id == nuevaHabitacionId)
            ?? throw new InvalidOperationException("Habitación no encontrada.");

        if (nuevaHabitacion.CentroId != residente.CentroId)
        {
            throw new InvalidOperationException("La habitación elegida no pertenece al centro del residente.");
        }

        var habitacionAnteriorId = residente.HabitacionId;
        var habitacionAnterior = residente.Habitacion?.Numero ?? "sin asignar";
        residente.HabitacionId = nuevaHabitacionId;

        var eventosCreados = new List<EventoDistribucion>();
        void AgregarEvento(EventoDistribucion evento)
        {
            _db.EventosDistribucion.Add(evento);
            eventosCreados.Add(evento);
        }

        AgregarEvento(new EventoDistribucion
        {
            CentroId = residente.CentroId,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Traslado,
            RolDestino = RolEmpleado.Auxiliar,
            Descripcion = $"{residente.Nombre} {residente.Apellidos} se ha trasladado de la habitación {habitacionAnterior} a la {nuevaHabitacion.Numero}."
        });

        // La habitación que deja vacía necesita limpieza antes de que entre
        // otro residente; la nueva no genera tarea, no se ensucia por sí sola.
        if (habitacionAnteriorId.HasValue)
        {
            _db.TareasLimpieza.Add(new TareaLimpieza
            {
                CentroId = residente.CentroId,
                HabitacionId = habitacionAnteriorId,
                Descripcion = $"Limpiar habitación {habitacionAnterior} (vacada tras traslado de {residente.Nombre} {residente.Apellidos})."
            });
        }

        AgregarEvento(new EventoDistribucion
        {
            CentroId = residente.CentroId,
            ResidenteId = residente.Id,
            TipoEvento = TipoEvento.Traslado,
            RolDestino = RolEmpleado.Limpieza,
            Descripcion = $"{residente.Nombre} {residente.Apellidos} se ha trasladado de la habitación {habitacionAnterior} a la {nuevaHabitacion.Numero}."
        });

        await _db.SaveChangesAsync();
        await _push.NotificarAsync(eventosCreados);

        await _auditoria.RegistrarAsync(empleadoId, "Traslado", "Residente", residente.Id,
            $"De habitación {habitacionAnterior} a {nuevaHabitacion.Numero}.");
    }

    public async Task ActualizarAsync(int residenteId, ResidenteEditDto dto, int? empleadoId)
    {
        var residente = await _db.Residentes
            .Include(r => r.Dietas).ThenInclude(d => d.Dieta)
            .Include(r => r.Alergias).ThenInclude(a => a.Alergia)
            .FirstOrDefaultAsync(r => r.Id == residenteId)
            ?? throw new InvalidOperationException("Residente no encontrado.");

        var cambios = new List<string>();
        void RegistrarCambio(string campo, string? antes, string? despues)
        {
            if (antes == despues) return;
            cambios.Add($"{campo}: {(string.IsNullOrEmpty(antes) ? "-" : antes)} → {(string.IsNullOrEmpty(despues) ? "-" : despues)}");
        }

        RegistrarCambio("Nombre", residente.Nombre, dto.Nombre);
        RegistrarCambio("Apellidos", residente.Apellidos, dto.Apellidos);
        RegistrarCambio("Documento", residente.DocumentoIdentidad, dto.DocumentoIdentidad);
        RegistrarCambio("Fecha nacimiento", residente.FechaNacimiento.ToString(), dto.FechaNacimiento.ToString());
        RegistrarCambio("Tipo", residente.TipoResidente.ToString(), dto.TipoResidente.ToString());
        RegistrarCambio("Teléfono", residente.Telefono, dto.Telefono);
        RegistrarCambio("Email", residente.Email, dto.Email);
        RegistrarCambio("Contacto emergencia", residente.ContactoEmergenciaNombre, dto.ContactoEmergenciaNombre);

        // Cambios que además interesan a Auxiliares (movilidad/asistencia/equipamiento).
        var cambiaAtencion = residente.Movilidad != dto.Movilidad
            || residente.NecesitaAyudaLevantarse != dto.NecesitaAyudaLevantarse
            || residente.EquipamientoEspecial != dto.EquipamientoEspecial;

        RegistrarCambio("Movilidad", residente.Movilidad.ToString(), dto.Movilidad.ToString());
        RegistrarCambio("Ayuda para levantarse", residente.NecesitaAyudaLevantarse.ToString(), dto.NecesitaAyudaLevantarse.ToString());
        RegistrarCambio("Equipamiento especial", residente.EquipamientoEspecial, dto.EquipamientoEspecial);

        residente.Nombre = dto.Nombre;
        residente.Apellidos = dto.Apellidos;
        residente.DocumentoIdentidad = dto.DocumentoIdentidad;
        residente.FechaNacimiento = dto.FechaNacimiento;
        residente.TipoResidente = dto.TipoResidente;
        residente.Movilidad = dto.Movilidad;
        residente.NecesitaAyudaLevantarse = dto.NecesitaAyudaLevantarse;
        residente.EquipamientoEspecial = dto.EquipamientoEspecial;
        residente.Telefono = dto.Telefono;
        residente.Email = dto.Email;
        residente.ContactoEmergenciaNombre = dto.ContactoEmergenciaNombre;
        residente.ContactoEmergenciaRelacion = dto.ContactoEmergenciaRelacion;
        residente.ContactoEmergenciaTelefono = dto.ContactoEmergenciaTelefono;
        residente.ContactoEmergenciaEmail = dto.ContactoEmergenciaEmail;

        // Dieta: se mantiene el historial cerrando la vigente y abriendo una nueva,
        // igual que si fuera un alta — así ResidenteDietas sigue siendo un historial real.
        var dietaVigente = residente.Dietas.FirstOrDefault(d => d.FechaFin == null);
        var cambiaDieta = dietaVigente?.DietaId != dto.DietaId;
        if (cambiaDieta)
        {
            if (dietaVigente != null) dietaVigente.FechaFin = DateOnly.FromDateTime(DateTime.UtcNow);
            _db.ResidenteDietas.Add(new ResidenteDieta
            {
                CentroId = residente.CentroId,
                ResidenteId = residente.Id,
                DietaId = dto.DietaId,
                FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                Motivo = "Modificación de datos"
            });
            var dietaNueva = await _db.Dietas.FindAsync(dto.DietaId);
            RegistrarCambio("Dieta", dietaVigente?.Dieta?.Nombre, dietaNueva?.Nombre);
        }

        // Alergias: no tienen historial (solo el conjunto vigente), se sustituye tal cual.
        var alergiasActuales = residente.Alergias.Select(a => a.AlergiaId).ToHashSet();
        var alergiasNuevas = dto.AlergiaIds.ToHashSet();
        var cambianAlergias = !alergiasActuales.SetEquals(alergiasNuevas);
        var alergiasAntesTexto = string.Join(", ", residente.Alergias.Select(a => a.Alergia?.Nombre));
        if (cambianAlergias)
        {
            _db.ResidenteAlergias.RemoveRange(residente.Alergias.Where(a => !alergiasNuevas.Contains(a.AlergiaId)));
            foreach (var id in alergiasNuevas.Except(alergiasActuales))
            {
                _db.ResidenteAlergias.Add(new ResidenteAlergia { CentroId = residente.CentroId, ResidenteId = residente.Id, AlergiaId = id });
            }
        }

        var eventosCreados = new List<EventoDistribucion>();

        if (cambiaDieta || cambianAlergias)
        {
            var alergiasTexto = await _db.Alergias.Where(a => dto.AlergiaIds.Contains(a.Id)).Select(a => a.Nombre).ToListAsync();
            var dietaTexto = await _db.Dietas.FindAsync(dto.DietaId);
            RegistrarCambio("Alergias", alergiasAntesTexto, string.Join(", ", alergiasTexto));

            var eventoCocina = new EventoDistribucion
            {
                CentroId = residente.CentroId,
                ResidenteId = residente.Id,
                TipoEvento = TipoEvento.Modificacion,
                RolDestino = RolEmpleado.Cocina,
                Descripcion = $"Datos actualizados de {residente.Nombre} {residente.Apellidos}. Dieta: {dietaTexto?.Nombre}." +
                    (alergiasTexto.Count > 0 ? $" Alergias: {string.Join(", ", alergiasTexto)}." : " Sin alergias registradas.")
            };
            _db.EventosDistribucion.Add(eventoCocina);
            eventosCreados.Add(eventoCocina);
        }

        if (cambiaAtencion)
        {
            var eventoAuxiliar = new EventoDistribucion
            {
                CentroId = residente.CentroId,
                ResidenteId = residente.Id,
                TipoEvento = TipoEvento.Modificacion,
                RolDestino = RolEmpleado.Auxiliar,
                Descripcion = $"Datos actualizados de {residente.Nombre} {residente.Apellidos}. Movilidad: {dto.Movilidad}." +
                    (dto.NecesitaAyudaLevantarse ? " Necesita ayuda para levantarse." : "") +
                    (string.IsNullOrWhiteSpace(dto.EquipamientoEspecial) ? "" : $" Equipamiento: {dto.EquipamientoEspecial}.")
            };
            _db.EventosDistribucion.Add(eventoAuxiliar);
            eventosCreados.Add(eventoAuxiliar);
        }

        await _db.SaveChangesAsync();
        await _push.NotificarAsync(eventosCreados);

        if (cambios.Count > 0)
        {
            await _auditoria.RegistrarAsync(empleadoId, "Modificacion", "Residente", residente.Id, string.Join("; ", cambios));
        }
    }
}
