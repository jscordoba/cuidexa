# Cuidexa — MVP 1.0

[![GitHub](https://img.shields.io/badge/GitHub-jscordoba%2Fcuidexa-blue?logo=github)](https://github.com/jscordoba/cuidexa)

MVP que valida un único flujo: cuando Admin da de alta a un residente,
el sistema genera automáticamente los avisos que Cocina y Auxiliares
necesitan ver, sin que nadie tenga que comunicarlo manualmente.
Dirección tiene un panel de solo lectura con el estado general.

## Alcance de este MVP

Incluido: Admin, Cocina, Auxiliares, Dirección · alta de residente ·
distribución automática de información · historial de dietas.

Fuera de alcance (deliberadamente, para no repetir el error del primer
intento): enfermería/medicación, limpieza, fisioterapia, multi-tenant,
notificaciones push, apps móviles nativas, Docker, microservicios.

## Arquitectura

Monolito .NET en capas simples — nada de patrones que no aporten valor
a este tamaño de proyecto:

```
Controller → Service → DbContext (EF Core) → PostgreSQL
```

- `Models/` — entidades del dominio (coinciden con el diagrama ER acordado)
- `Data/CuidexaDbContext.cs` — mapeo EF Core + datos de prueba (seed)
- `Services/` — lógica de negocio (aquí vive el flujo de alta y distribución)
- `Controllers/` + `Views/` — un controlador y una vista por rol

## Requisitos para ejecutarlo

1. [.NET SDK 10](https://dotnet.microsoft.com/download) instalado
2. PostgreSQL corriendo en local (puedes usar la instalación nativa o
   un contenedor suelto de Docker si ya lo tienes — no hace falta
   Docker Compose para esto, es un único servicio)
3. Herramienta de migraciones de EF Core:
   ```
   dotnet tool install --global dotnet-ef
   ```

## Puesta en marcha

Desde la carpeta `Cuidexa.Web`:

```bash
# 1. Restaurar paquetes NuGet
dotnet restore

# 2. Ajustar la cadena de conexión si tu Postgres no usa
#    usuario/contraseña "postgres"/"postgres" (ver appsettings.json)

# 3. Crear la migración inicial (genera las tablas a partir del modelo)
dotnet ef migrations add InicialMvp

# 4. Aplicar la migración a la base de datos
dotnet ef database update

# 5. Ejecutar la aplicación
dotnet run
```

El primer arranque también aplica migraciones automáticamente
(`db.Database.Migrate()` en `Program.cs`), así que el paso 4 es
redundante pero recomendable la primera vez para ver que todo esté bien.

## Usuarios de prueba (datos semilla)

| Rol        | Email                  | Contraseña   |
|------------|-------------------------|--------------|
| Admin      | admin@demo.local         | Demo1234!    |
| Cocina     | cocina@demo.local        | Demo1234!    |
| Auxiliar   | auxiliar@demo.local      | Demo1234!    |
| Dirección  | direccion@demo.local     | Demo1234!    |

## Flujo a probar

1. Entra como `admin@demo.local`.
2. Ve a "Residentes" → "Dar de alta" → rellena el formulario (asigna
   una dieta y alguna alergia).
3. Cierra sesión y entra como `cocina@demo.local` → verás el aviso
   generado automáticamente con la dieta y alergias del nuevo residente.
4. Repite entrando como `auxiliar@demo.local` → verás el aviso con
   movilidad y necesidades de asistencia.
5. Entra como `direccion@demo.local` → verás el conteo de residentes
   y avisos pendientes por rol.

## Cosas señaladas a propósito como pendientes (no producción)

- **Hash de contraseñas**: usa SHA256 simple (`PasswordHasher.cs`) solo
  para probar el flujo. Antes de manejar datos reales, sustituir por
  ASP.NET Core Identity o BCrypt/Argon2 con salting.
- **Sin cifrado de datos sensibles** ni auditoría — necesario antes de
  tocar datos de salud reales (ver comentario sobre RGPD/AEPD).
- **Sin multi-tenant** — todo el modelo asume un único centro.

## Próximos pasos sugeridos

Una vez pruebes este flujo y confirmes que el concepto funciona como
esperabas, los siguientes pasos naturales (en este orden) serían:
añadir el rol de Enfermería, luego contenerizar con Docker, y más
adelante evaluar si tiene sentido separar servicios.
