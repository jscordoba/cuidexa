using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class EmpleadoService : IEmpleadoService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;

    public EmpleadoService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
    }

    public async Task<List<Empleado>> ObtenerTodosAsync()
    {
        return await _db.Empleados.Include(e => e.Especialidad).Include(e => e.Centro).OrderBy(e => e.Nombre).ToListAsync();
    }

    public async Task<Empleado?> ObtenerPorIdAsync(int id) => await _db.Empleados.FindAsync(id);

    // Los 5 roles operativos necesitan un turno por defecto para que el
    // calendario/Comensales tengan algo que asumir cada día; Admin/Dirección
    // no cubren turnos de centro, así que no lo llevan.
    private static bool EsRolOperativo(RolEmpleado rol) =>
        rol is not (RolEmpleado.Admin or RolEmpleado.Direccion or RolEmpleado.DirectorOrganizacion);

    public async Task<Empleado> CrearAsync(EmpleadoCreateDto dto, int? empleadoIdActor, int? centroId = null)
    {
        // Solo DirectorOrganizacion pasa centroId (eligiendo entre los
        // centros de su propia organización) — se valida que de verdad sea
        // uno de los suyos, no un id cualquiera. Admin nunca lo pasa: usa
        // siempre el centro de su propia sesión.
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

        // (CentroId, Email) es la unicidad real (Fase 9) — comprobar solo en
        // el centro destino, no en todos los de la organización, ya que dos
        // centros distintos pueden reutilizar el mismo email.
        if (await _db.Empleados.IgnoreQueryFilters().AnyAsync(e => e.CentroId == centroIdEfectivo && e.Email == dto.Email))
        {
            throw new InvalidOperationException("Ya existe un usuario con ese email en ese centro.");
        }

        if (EsRolOperativo(dto.Rol) && dto.PlantillaTurnoDefectoId is null)
        {
            throw new InvalidOperationException("Elige el turno por defecto de este usuario.");
        }

        PasswordHasher.Validar(dto.Password);

        var empleado = new Empleado
        {
            CentroId = centroIdEfectivo,
            Nombre = dto.Nombre,
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Rol = dto.Rol,
            EspecialidadId = dto.Rol == RolEmpleado.Profesional ? dto.EspecialidadId : null,
            PlantillaTurnoDefectoId = EsRolOperativo(dto.Rol) ? dto.PlantillaTurnoDefectoId : null,
            Activo = true
        };

        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, "Alta", "Empleado", empleado.Id,
            $"Alta de {empleado.Nombre} ({empleado.Email}) con rol {empleado.Rol}.");

        return empleado;
    }

    public async Task ActualizarAsync(int id, EmpleadoEditDto dto, int? empleadoIdActor)
    {
        var empleado = await _db.Empleados.FindAsync(id)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        // Acotado al propio centro del empleado (no al de quien edita, que
        // con DirectorOrganizacion puede ser otro centro de la misma
        // organización) — (CentroId, Email) es la unicidad real.
        if (await _db.Empleados.IgnoreQueryFilters().AnyAsync(e => e.CentroId == empleado.CentroId && e.Email == dto.Email && e.Id != id))
        {
            throw new InvalidOperationException("Ya existe otro usuario con ese email en ese centro.");
        }

        if (EsRolOperativo(dto.Rol) && dto.PlantillaTurnoDefectoId is null)
        {
            throw new InvalidOperationException("Elige el turno por defecto de este usuario.");
        }

        var nuevaEspecialidadId = dto.Rol == RolEmpleado.Profesional ? dto.EspecialidadId : null;
        var nuevaPlantillaDefectoId = EsRolOperativo(dto.Rol) ? dto.PlantillaTurnoDefectoId : null;

        var cambios = new List<string>();
        if (empleado.Nombre != dto.Nombre) cambios.Add($"Nombre: {empleado.Nombre} → {dto.Nombre}");
        if (empleado.Email != dto.Email) cambios.Add($"Email: {empleado.Email} → {dto.Email}");
        if (empleado.Rol != dto.Rol) cambios.Add($"Rol: {empleado.Rol} → {dto.Rol}");
        if (empleado.EspecialidadId != nuevaEspecialidadId) cambios.Add("Especialidad actualizada");
        if (empleado.PlantillaTurnoDefectoId != nuevaPlantillaDefectoId) cambios.Add("Turno por defecto actualizado");

        empleado.Nombre = dto.Nombre;
        empleado.Email = dto.Email;
        empleado.Rol = dto.Rol;
        empleado.EspecialidadId = nuevaEspecialidadId;
        empleado.PlantillaTurnoDefectoId = nuevaPlantillaDefectoId;

        if (!string.IsNullOrWhiteSpace(dto.NuevaPassword))
        {
            PasswordHasher.Validar(dto.NuevaPassword);
            empleado.PasswordHash = PasswordHasher.Hash(dto.NuevaPassword);
            // Vía de recuperación de cuenta sin email: un Admin/DirectorOrganizacion
            // resetea la contraseña de un compañero que la olvidó o que se
            // bloqueó por fuerza bruta, y queda desbloqueada al instante.
            var estabaBloqueado = BloqueoCuenta.EstaBloqueada(empleado);
            BloqueoCuenta.Desbloquear(empleado);
            cambios.Add(estabaBloqueado ? "Contraseña restablecida (cuenta desbloqueada)" : "Contraseña restablecida");
        }

        await _db.SaveChangesAsync();

        if (cambios.Count > 0)
        {
            await _auditoria.RegistrarAsync(empleadoIdActor, "Modificacion", "Empleado", empleado.Id, string.Join("; ", cambios));
        }
    }

    public async Task CambiarEstadoAsync(int id, bool activo, int? empleadoIdActor)
    {
        if (!activo && id == empleadoIdActor)
        {
            throw new InvalidOperationException("No puedes desactivar tu propia cuenta.");
        }

        var empleado = await _db.Empleados.FindAsync(id)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        empleado.Activo = activo;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, activo ? "Activacion" : "Desactivacion", "Empleado", empleado.Id,
            activo ? "Cuenta reactivada." : "Cuenta desactivada.");
    }

    public async Task<Empleado> CrearAdminInicialAsync(int centroId, string nombre, string email, string password)
    {
        // IgnoreQueryFilters + comparación explícita de CentroId: quien llama
        // (SuperAdmin) no tiene Centro de sesión propio, así que el filtro
        // global evaluaría CentroId == 0 y nunca encontraría el choque real.
        var yaExiste = await _db.Empleados.IgnoreQueryFilters()
            .AnyAsync(e => e.CentroId == centroId && e.Email == email);
        if (yaExiste)
        {
            throw new InvalidOperationException("Ya existe un usuario con ese email en este centro.");
        }

        PasswordHasher.Validar(password);

        var empleado = new Empleado
        {
            CentroId = centroId,
            Nombre = nombre,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Rol = RolEmpleado.Admin,
            Activo = true
        };

        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(null, "Alta", "Empleado", empleado.Id,
            $"Alta del primer Admin ({empleado.Email}) al provisionar este centro, vía SuperAdmin.",
            centroIdExplicito: centroId);

        return empleado;
    }
}
