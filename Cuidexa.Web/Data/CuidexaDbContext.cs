using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cuidexa.Web.Data;

public class CuidexaDbContext : DbContext
{
    private readonly byte[] _encryptionKey;
    private readonly ITenantContext _tenant;

    public CuidexaDbContext(DbContextOptions<CuidexaDbContext> options, IConfiguration configuration, ITenantContext tenant) : base(options)
    {
        _encryptionKey = Convert.FromBase64String(configuration["Encryption:Key"]!);
        _tenant = tenant;
    }

    public DbSet<Organizacion> Organizaciones => Set<Organizacion>();
    public DbSet<Centro> Centros => Set<Centro>();
    public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<Residente> Residentes => Set<Residente>();
    public DbSet<Dieta> Dietas => Set<Dieta>();
    public DbSet<ResidenteDieta> ResidenteDietas => Set<ResidenteDieta>();
    public DbSet<Alergia> Alergias => Set<Alergia>();
    public DbSet<ResidenteAlergia> ResidenteAlergias => Set<ResidenteAlergia>();
    public DbSet<EventoDistribucion> EventosDistribucion => Set<EventoDistribucion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Patologia> Patologias => Set<Patologia>();
    public DbSet<ResidentePatologia> ResidentePatologias => Set<ResidentePatologia>();
    public DbSet<Medicacion> Medicaciones => Set<Medicacion>();
    public DbSet<RegistroAdministracion> RegistrosAdministracion => Set<RegistroAdministracion>();
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<SesionTerapia> SesionesTerapia => Set<SesionTerapia>();
    public DbSet<TareaLimpieza> TareasLimpieza => Set<TareaLimpieza>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<CambioTurno> CambiosTurno => Set<CambioTurno>();
    public DbSet<PlantillaTurno> PlantillasTurno => Set<PlantillaTurno>();
    public DbSet<Vacacion> Vacaciones => Set<Vacacion>();
    public DbSet<Feriado> Feriados => Set<Feriado>();
    public DbSet<CuentaDispositivo> CuentasDispositivo => Set<CuentaDispositivo>();
    public DbSet<GrupoNotificacion> GruposNotificacion => Set<GrupoNotificacion>();
    public DbSet<GrupoNotificacionEmpleado> GruposNotificacionEmpleados => Set<GrupoNotificacionEmpleado>();
    public DbSet<SuscripcionPush> SuscripcionesPush => Set<SuscripcionPush>();
    public DbSet<Incidencia> Incidencias => Set<Incidencia>();
    public DbSet<DocumentoFirmado> DocumentosFirmados => Set<DocumentoFirmado>();
    public DbSet<TicketSoporte> TicketsSoporte => Set<TicketSoporte>();
    public DbSet<Familiar> Familiares => Set<Familiar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cifrado transparente en reposo para los campos identificativos/de contacto
        // del residente. Nombre/Apellidos/FechaNacimiento se dejan en claro a propósito:
        // se usan en listados, orden y búsqueda por todas partes y cifrarlos también
        // rompería eso — queda pendiente para una pasada de arquitectura posterior
        // si hace falta cifrado total de PII.
        var camposCifrados = new ValueConverter<string?, string?>(
            v => v == null ? null : EncryptionService.Encrypt(v, _encryptionKey),
            v => v == null ? null : EncryptionService.Decrypt(v, _encryptionKey));

        // Misma idea que camposCifrados pero para propiedades no-nulas (Medicacion.Nombre,
        // Dosis, Horario) — evita el warning de nulabilidad de aplicar un converter
        // string?→string? a una propiedad string no anulable.
        var camposCifradosRequeridos = new ValueConverter<string, string?>(
            v => EncryptionService.Encrypt(v, _encryptionKey),
            v => v == null ? string.Empty : EncryptionService.Decrypt(v, _encryptionKey));

        modelBuilder.Entity<Residente>().Property(r => r.DocumentoIdentidad).HasConversion(camposCifrados);
        modelBuilder.Entity<Residente>().Property(r => r.Telefono).HasConversion(camposCifrados);
        modelBuilder.Entity<Residente>().Property(r => r.Email).HasConversion(camposCifrados);
        modelBuilder.Entity<Residente>().Property(r => r.ContactoEmergenciaTelefono).HasConversion(camposCifrados);
        modelBuilder.Entity<Residente>().Property(r => r.ContactoEmergenciaEmail).HasConversion(camposCifrados);

        // Información clínica (Enfermería): datos específicos del residente, no
        // catálogo, así que se cifran igual que los campos de contacto de arriba.
        modelBuilder.Entity<ResidentePatologia>().Property(rp => rp.Observaciones).HasConversion(camposCifrados);
        modelBuilder.Entity<Medicacion>().Property(m => m.Nombre).HasConversion(camposCifradosRequeridos);
        modelBuilder.Entity<Medicacion>().Property(m => m.Dosis).HasConversion(camposCifradosRequeridos);
        modelBuilder.Entity<Medicacion>().Property(m => m.Horario).HasConversion(camposCifradosRequeridos);
        modelBuilder.Entity<Medicacion>().Property(m => m.Instrucciones).HasConversion(camposCifrados);
        modelBuilder.Entity<RegistroAdministracion>().Property(r => r.Observaciones).HasConversion(camposCifrados);
        modelBuilder.Entity<SesionTerapia>().Property(s => s.Observaciones).HasConversion(camposCifrados);

        // --- Organizacion / Centro (Fase 9) ---
        // Nunca llevan filtro global: SuperAdmin y las pantallas de "mis
        // centros" de un Director de Organización necesitan verlas sin
        // restricción de tenant.
        modelBuilder.Entity<Centro>()
            .HasOne(c => c.Organizacion)
            .WithMany(o => o.Centros)
            .HasForeignKey(c => c.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Centro>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        // --- FK + filtro global de aislamiento por Centro (19 tablas) ---
        // El filtro deja pasar la fila si es del Centro del usuario, o si el
        // usuario es DirectorOrganizacion y la fila pertenece a cualquier
        // Centro de su misma Organizacion. Escritas de forma explícita (sin
        // generics) porque el filtro es una traducción a SQL: pasar por una
        // interfaz genérica puede hacer que EF Core no reconozca el miembro
        // de la clase concreta y falle en tiempo de ejecución — inaceptable
        // en un filtro de seguridad que separa los datos clínicos de cada
        // centro. La configuración es repetitiva a propósito, no por descuido.
        modelBuilder.Entity<Empleado>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Empleado>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<Residente>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Residente>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<Habitacion>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Habitacion>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<ResidenteDieta>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ResidenteDieta>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<ResidenteAlergia>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ResidenteAlergia>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<ResidentePatologia>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ResidentePatologia>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<Medicacion>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Medicacion>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<RegistroAdministracion>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<RegistroAdministracion>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<SesionTerapia>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SesionTerapia>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<TareaLimpieza>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TareaLimpieza>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<Turno>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Turno>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<CambioTurno>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CambioTurno>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<Vacacion>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vacacion>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<Feriado>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Feriado>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<CuentaDispositivo>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CuentaDispositivo>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<EventoDistribucion>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EventoDistribucion>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<GrupoNotificacion>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GrupoNotificacion>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<GrupoNotificacionEmpleado>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GrupoNotificacionEmpleado>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<SuscripcionPush>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SuscripcionPush>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<AuditLog>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<Incidencia>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Incidencia>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<DocumentoFirmado>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DocumentoFirmado>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        modelBuilder.Entity<TicketSoporte>().HasOne(e => e.Centro).WithMany().HasForeignKey(e => e.CentroId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TicketSoporte>().HasOne(e => e.Empleado).WithMany().HasForeignKey(e => e.EmpleadoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TicketSoporte>().HasOne(e => e.RespondidoPorSuperAdmin).WithMany().HasForeignKey(e => e.RespondidoPorSuperAdminId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TicketSoporte>().HasQueryFilter(e =>
            e.CentroId == _tenant.CentroId || (_tenant.AccesoOrganizacionCompleto && e.Centro!.OrganizacionId == _tenant.OrganizacionId));

        // Familiar: sin HasQueryFilter (no implementa ITieneCentro) — su
        // aislamiento real es por ResidenteId, aplicado explícitamente en
        // FamiliarService/FamiliarController, no por Centro.
        modelBuilder.Entity<Familiar>().HasOne(e => e.Residente).WithMany().HasForeignKey(e => e.ResidenteId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Familiar>().HasIndex(f => f.Email).IsUnique();

        // --- FK + filtro global de aislamiento por Organización (5 catálogos) ---
        // Compartidos por todos los Centros de una misma Organizacion — sin
        // rama AccesoOrganizacionCompleto: cualquier rol ya ve el catálogo de
        // su propia organización, sea o no DirectorOrganizacion.
        modelBuilder.Entity<Dieta>().HasOne(e => e.Organizacion).WithMany().HasForeignKey(e => e.OrganizacionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Dieta>().HasQueryFilter(e => e.OrganizacionId == _tenant.OrganizacionId);

        modelBuilder.Entity<Alergia>().HasOne(e => e.Organizacion).WithMany().HasForeignKey(e => e.OrganizacionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Alergia>().HasQueryFilter(e => e.OrganizacionId == _tenant.OrganizacionId);

        modelBuilder.Entity<Patologia>().HasOne(e => e.Organizacion).WithMany().HasForeignKey(e => e.OrganizacionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Patologia>().HasQueryFilter(e => e.OrganizacionId == _tenant.OrganizacionId);

        modelBuilder.Entity<Especialidad>().HasOne(e => e.Organizacion).WithMany().HasForeignKey(e => e.OrganizacionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Especialidad>().HasQueryFilter(e => e.OrganizacionId == _tenant.OrganizacionId);

        modelBuilder.Entity<PlantillaTurno>().HasOne(e => e.Organizacion).WithMany().HasForeignKey(e => e.OrganizacionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PlantillaTurno>().HasQueryFilter(e => e.OrganizacionId == _tenant.OrganizacionId);

        // Clave compuesta para la tabla intermedia N:M
        modelBuilder.Entity<ResidenteAlergia>()
            .HasKey(ra => new { ra.ResidenteId, ra.AlergiaId });

        modelBuilder.Entity<ResidenteAlergia>()
            .HasOne(ra => ra.Residente)
            .WithMany(r => r.Alergias)
            .HasForeignKey(ra => ra.ResidenteId);

        modelBuilder.Entity<ResidenteAlergia>()
            .HasOne(ra => ra.Alergia)
            .WithMany()
            .HasForeignKey(ra => ra.AlergiaId);

        modelBuilder.Entity<ResidenteDieta>()
            .HasOne(rd => rd.Residente)
            .WithMany(r => r.Dietas)
            .HasForeignKey(rd => rd.ResidenteId);

        modelBuilder.Entity<ResidentePatologia>()
            .HasKey(rp => new { rp.ResidenteId, rp.PatologiaId });

        modelBuilder.Entity<ResidentePatologia>()
            .HasOne(rp => rp.Residente)
            .WithMany(r => r.Patologias)
            .HasForeignKey(rp => rp.ResidenteId);

        modelBuilder.Entity<ResidentePatologia>()
            .HasOne(rp => rp.Patologia)
            .WithMany()
            .HasForeignKey(rp => rp.PatologiaId);

        modelBuilder.Entity<Medicacion>()
            .HasOne(m => m.Residente)
            .WithMany(r => r.Medicaciones)
            .HasForeignKey(m => m.ResidenteId);

        modelBuilder.Entity<RegistroAdministracion>()
            .HasOne(r => r.Medicacion)
            .WithMany(m => m.Registros)
            .HasForeignKey(r => r.MedicacionId);

        modelBuilder.Entity<EventoDistribucion>()
            .HasOne(e => e.Residente)
            .WithMany(r => r.Eventos)
            .HasForeignKey(e => e.ResidenteId);

        modelBuilder.Entity<EventoDistribucion>()
            .HasOne(e => e.EmpleadoOrigen)
            .WithMany()
            .HasForeignKey(e => e.EmpleadoOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Especialidad)
            .WithMany()
            .HasForeignKey(e => e.EspecialidadId);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.PlantillaTurnoDefecto)
            .WithMany()
            .HasForeignKey(e => e.PlantillaTurnoDefectoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SesionTerapia>()
            .HasOne(s => s.Residente)
            .WithMany(r => r.SesionesTerapia)
            .HasForeignKey(s => s.ResidenteId);

        modelBuilder.Entity<SesionTerapia>()
            .HasOne(s => s.Empleado)
            .WithMany()
            .HasForeignKey(s => s.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TareaLimpieza>()
            .HasOne(t => t.Habitacion)
            .WithMany(h => h.Tareas)
            .HasForeignKey(t => t.HabitacionId);

        modelBuilder.Entity<TareaLimpieza>()
            .HasOne(t => t.Empleado)
            .WithMany()
            .HasForeignKey(t => t.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Turno>()
            .HasOne(t => t.Empleado)
            .WithMany()
            .HasForeignKey(t => t.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Turno>()
            .HasOne(t => t.PlantillaTurno)
            .WithMany()
            .HasForeignKey(t => t.PlantillaTurnoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Turno>()
            .HasOne(t => t.EmpleadoOriginal)
            .WithMany()
            .HasForeignKey(t => t.EmpleadoOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vacacion>()
            .HasOne(v => v.Empleado)
            .WithMany()
            .HasForeignKey(v => v.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vacacion>()
            .HasOne(v => v.ResueltoPor)
            .WithMany()
            .HasForeignKey(v => v.ResueltoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Únicos ahora compuestos con CentroId (Fase 9) — dos centros distintos
        // pueden compartir la misma fecha de festivo o el mismo nombre de tablet.
        modelBuilder.Entity<Feriado>()
            .HasIndex(f => new { f.CentroId, f.Fecha })
            .IsUnique();

        modelBuilder.Entity<CuentaDispositivo>()
            .HasIndex(c => new { c.CentroId, c.Nombre })
            .IsUnique();

        // Nuevo (Fase 9): no existía ninguna restricción de unicidad de email
        // antes — dos centros distintos pueden reutilizar el mismo email, pero
        // no dos empleados del mismo centro.
        modelBuilder.Entity<Empleado>()
            .HasIndex(e => new { e.CentroId, e.Email })
            .IsUnique();

        modelBuilder.Entity<EventoDistribucion>()
            .HasOne(e => e.EmpleadoDestino)
            .WithMany()
            .HasForeignKey(e => e.EmpleadoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GrupoNotificacionEmpleado>()
            .HasKey(m => new { m.GrupoNotificacionId, m.EmpleadoId });

        modelBuilder.Entity<GrupoNotificacionEmpleado>()
            .HasOne(m => m.GrupoNotificacion)
            .WithMany(g => g.Miembros)
            .HasForeignKey(m => m.GrupoNotificacionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GrupoNotificacionEmpleado>()
            .HasOne(m => m.Empleado)
            .WithMany()
            .HasForeignKey(m => m.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SuscripcionPush>()
            .HasOne(s => s.Empleado)
            .WithMany()
            .HasForeignKey(s => s.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SuscripcionPush>()
            .HasIndex(s => s.Endpoint)
            .IsUnique();

        modelBuilder.Entity<CambioTurno>()
            .HasOne(c => c.EmpleadoSolicitante)
            .WithMany()
            .HasForeignKey(c => c.EmpleadoSolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CambioTurno>()
            .HasOne(c => c.EmpleadoSustitutoPropuesto)
            .WithMany()
            .HasForeignKey(c => c.EmpleadoSustitutoPropuestoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CambioTurno>()
            .HasOne(c => c.ResueltoPor)
            .WithMany()
            .HasForeignKey(c => c.ResueltoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incidencia>()
            .HasOne(i => i.Residente)
            .WithMany()
            .HasForeignKey(i => i.ResidenteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incidencia>()
            .HasOne(i => i.EmpleadoOrigen)
            .WithMany()
            .HasForeignKey(i => i.EmpleadoOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incidencia>()
            .HasOne(i => i.ResueltoPor)
            .WithMany()
            .HasForeignKey(i => i.ResueltoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoFirmado>()
            .HasOne(d => d.Residente)
            .WithMany()
            .HasForeignKey(d => d.ResidenteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoFirmado>()
            .HasOne(d => d.Incidencia)
            .WithMany(i => i.DocumentosFirmados)
            .HasForeignKey(d => d.IncidenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoFirmado>()
            .HasOne(d => d.EmpleadoRegistra)
            .WithMany()
            .HasForeignKey(d => d.EmpleadoRegistraId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Datos de prueba (demo local, no usar en producción) ---
        // Password para todos los empleados demo: "Demo1234!"
        // Hash BCrypt fijo (no se puede llamar a PasswordHasher.Hash aquí: genera
        // un salt distinto en cada ejecución, lo que EF vería como un cambio de
        // modelo constante). Recalculado una vez con Services.PasswordHasher.Hash.
        const string demoHash = "$2a$11$VQE3kVBg8hch1ktvqgajiuF46YhG/qf7QOIEotN6q3cMsiNfBO5Pm";

        // Organización/Centro demo (Fase 9) — todo el resto de datos sembrados
        // se cuelga de este Centro #1. Código de centro para el login: "DEMO".
        // FechaCreacion fijada explícitamente (no el DateTime.UtcNow por
        // defecto del modelo): un HasData con un valor que cambia en cada
        // rebuild hace que EF vea "cambios pendientes" en cada comando y
        // bloquee las migraciones — mismo motivo que el hash de BCrypt de
        // los empleados demo está fijo más abajo.
        var fechaSeedFase9 = new DateTime(2026, 9, 22, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Organizacion>().HasData(
            new Organizacion { Id = 1, Nombre = "Cuidexa", FechaCreacion = fechaSeedFase9 }
        );

        modelBuilder.Entity<SuperAdmin>().HasIndex(s => s.Email).IsUnique();

        // Cuenta SuperAdmin de arranque — mismo hash fijo que los empleados
        // demo de abajo (password "Demo1234!"), único punto de entrada para
        // provisionar la primera Organización/Centro reales.
        modelBuilder.Entity<SuperAdmin>().HasData(
            new SuperAdmin { Id = 1, Nombre = "SuperAdmin", Email = "superadmin@cuidexa.local", PasswordHash = demoHash }
        );

        modelBuilder.Entity<Centro>().HasData(
            new Centro { Id = 1, OrganizacionId = 1, Nombre = "Centro Demo", Codigo = "DEMO", FechaCreacion = fechaSeedFase9 }
        );

        modelBuilder.Entity<Empleado>().HasData(
            new Empleado { Id = 1, CentroId = 1, Nombre = "Admin Demo", Email = "admin@demo.local", PasswordHash = demoHash, Rol = RolEmpleado.Admin },
            new Empleado { Id = 2, CentroId = 1, Nombre = "Cocina Demo", Email = "cocina@demo.local", PasswordHash = demoHash, Rol = RolEmpleado.Cocina },
            new Empleado { Id = 3, CentroId = 1, Nombre = "Auxiliar Demo", Email = "auxiliar@demo.local", PasswordHash = demoHash, Rol = RolEmpleado.Auxiliar },
            new Empleado { Id = 4, CentroId = 1, Nombre = "Direccion Demo", Email = "direccion@demo.local", PasswordHash = demoHash, Rol = RolEmpleado.Direccion },
            // Id 5 no se usa: ya existe una fila real con ese Id en la base de
            // desarrollo (creada desde la app, no por seed) — se evita el choque.
            new Empleado { Id = 6, CentroId = 1, Nombre = "Enfermeria Demo", Email = "enfermeria@demo.local", PasswordHash = demoHash, Rol = RolEmpleado.Enfermeria },
            new Empleado { Id = 7, CentroId = 1, Nombre = "Fisioterapia Demo", Email = "profesional@demo.local", PasswordHash = demoHash, Rol = RolEmpleado.Profesional, EspecialidadId = 1 },
            new Empleado { Id = 8, CentroId = 1, Nombre = "Limpieza Demo", Email = "limpieza@demo.local", PasswordHash = demoHash, Rol = RolEmpleado.Limpieza }
        );

        modelBuilder.Entity<Habitacion>().HasData(
            new Habitacion { Id = 1, CentroId = 1, Numero = "101", Planta = "Planta 1" },
            new Habitacion { Id = 2, CentroId = 1, Numero = "102", Planta = "Planta 1" },
            new Habitacion { Id = 3, CentroId = 1, Numero = "201", Planta = "Planta 2" }
        );

        modelBuilder.Entity<Dieta>().HasData(
            new Dieta { Id = 1, OrganizacionId = 1, Nombre = "Normal", Descripcion = "Dieta estándar" },
            new Dieta { Id = 2, OrganizacionId = 1, Nombre = "Triturada", Descripcion = "Textura modificada" },
            new Dieta { Id = 3, OrganizacionId = 1, Nombre = "Diabética", Descripcion = "Control de azúcares" }
        );

        modelBuilder.Entity<Alergia>().HasData(
            new Alergia { Id = 1, OrganizacionId = 1, Nombre = "Frutos secos" },
            new Alergia { Id = 2, OrganizacionId = 1, Nombre = "Lactosa" },
            new Alergia { Id = 3, OrganizacionId = 1, Nombre = "Gluten" }
        );

        modelBuilder.Entity<Patologia>().HasData(
            new Patologia { Id = 1, OrganizacionId = 1, Nombre = "Hipertensión", Descripcion = "Presión arterial elevada" },
            new Patologia { Id = 2, OrganizacionId = 1, Nombre = "Diabetes tipo 2", Descripcion = "Control de glucosa en sangre" },
            new Patologia { Id = 3, OrganizacionId = 1, Nombre = "Alzheimer", Descripcion = "Deterioro cognitivo" },
            new Patologia { Id = 4, OrganizacionId = 1, Nombre = "Artrosis", Descripcion = "Desgaste articular" }
        );

        modelBuilder.Entity<Especialidad>().HasData(
            new Especialidad { Id = 1, OrganizacionId = 1, Nombre = "Fisioterapia" },
            new Especialidad { Id = 2, OrganizacionId = 1, Nombre = "Logopedia" },
            new Especialidad { Id = 3, OrganizacionId = 1, Nombre = "Terapia ocupacional" },
            new Especialidad { Id = 4, OrganizacionId = 1, Nombre = "Psicología" }
        );

        modelBuilder.Entity<PlantillaTurno>().HasData(
            new PlantillaTurno { Id = 1, OrganizacionId = 1, Nombre = "Mañana", HoraInicio = new TimeOnly(7, 0), HoraFin = new TimeOnly(15, 0), TipoDia = TipoDia.Laborable },
            new PlantillaTurno { Id = 2, OrganizacionId = 1, Nombre = "Tarde", HoraInicio = new TimeOnly(15, 0), HoraFin = new TimeOnly(23, 0), TipoDia = TipoDia.Laborable },
            new PlantillaTurno { Id = 3, OrganizacionId = 1, Nombre = "Noche", HoraInicio = new TimeOnly(23, 0), HoraFin = new TimeOnly(7, 0), TipoDia = TipoDia.Laborable }
        );
    }
}
