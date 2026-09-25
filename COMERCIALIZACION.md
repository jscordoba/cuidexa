# Plan de comercialización

Lista de trabajo para llevar Cuidexa a su primera empresa de pruebas (piloto
real con datos de residentes reales). Complementa a [ROADMAP.md](ROADMAP.md)
— el roadmap cubre funcionalidad de producto, este documento cubre lo que
hace falta para operarlo de forma segura con un cliente real.

Se marca aquí el progreso de cada bloque a medida que se completa, para que
cualquier sesión futura (yo mismo u otra persona) pueda ver de un vistazo
qué queda.

## Orden de prioridad y por qué

1. **Monitorización/logging** — sin esto, cualquier fallo durante el piloto
   se descubre porque el cliente se queja, no porque lo veamos nosotros
   primero. Base para todo lo que sigue.
2. **Backups automatizados** — antes de meter datos reales de residentes
   (salud, contacto de emergencia) tiene que existir una copia de
   seguridad automática y probada. No negociable.
3. **Canal de soporte para clientes** — en cuanto haya un piloto, alguien
   tiene que poder decir "esto no funciona" y que llegue a alguien.
   Esfuerzo bajo, impacto alto nada más arrancar.
4. **App instalable / pulir experiencia offline** — los roles operativos
   (Cocina, Auxiliares, Enfermería...) son mobile-first por principio del
   roadmap; el piloto se va a juzgar en gran parte por cómo se siente en
   un móvil/tablet real, con cobertura de red irregular en un centro.
5. **Portal de familiares** — el diferenciador comercial más fuerte de
   este bloque, pero es funcionalidad nueva grande (nuevo tipo de acceso,
   nuevas vistas). Se aborda con la base ya estabilizada (monitorizada,
   con backups, soportada).
6. **Multi-idioma (ES/EN)** — mejor hacerlo con las vistas ya estables,
   para no traducir dos veces por cambios en curso. No bloquea al primer
   piloto si es en España.
7. **Sistema de facturación** — el alta de la primera empresa de pruebas
   la puede seguir haciendo el SuperAdmin manualmente (ya existe ese
   flujo desde la Fase 9). Construir cobro automático antes de tener un
   cliente real dispuesto a pagar es esfuerzo prematuro — se aborda al
   final de este bloque, cuando el piloto ya valida el producto.

## Checklist

### 1. Monitorización/logging ✅ (base local completada 2026-09-23)
- [x] Logging estructurado (Serilog): consola + fichero con rotación diaria
      (`logs/cuidexa-.log`, 30 días de retención), una línea por request
      (método, ruta, código, duración) vía `UseSerilogRequestLogging`,
      enriquecida con `CentroId`/`Usuario` — imprescindible en un SaaS
      multi-tenant para filtrar logs de un cliente concreto.
- [x] Endpoint `/health` (`Microsoft.Extensions.Diagnostics.HealthChecks`)
      que comprueba conectividad real con PostgreSQL — listo para
      monitores externos de disponibilidad (uptime) y para el propio
      despliegue.
- [ ] Captura de excepciones con alerta externa (Sentry/Application
      Insights o equivalente) — pendiente de decisión de negocio (cuenta/
      coste del servicio); el código ya está preparado, añadir un sink de
      Serilog es la única pieza que falta.
- [ ] Alertas activas de caída del servicio / tasa de error elevada —
      depende de tener un monitor externo llamando a `/health` (ej.
      UptimeRobot/Better Stack) o del servicio de alertas anterior.

### 2. Backups automatizados ✅ (base completada 2026-09-23)
- [x] Backup automático diario de PostgreSQL (`Services/BackupService.cs`,
      `BackgroundService` sin dependencias externas — coherente con el
      monolito autocontenido), cifrado en reposo con AES-256-GCM
      (`Services/BackupCifrado.cs`, clave independiente en
      `Backup:ClaveCifrado`). Desactivado por defecto
      (`Backup:Habilitado=false`) — hay que activarlo explícitamente por
      entorno.
- [x] Retención configurable por días (`Backup:RetencionDias`, 30 por
      defecto) — purga automática de backups más antiguos que el límite.
- [x] Prueba real de restauración documentada y ejecutada: backup real →
      `Scripts/Restaurar-Backup.ps1` (descifra + `pg_restore`) → base de
      datos de pruebas nueva → verificado con `SELECT count(*)` sobre
      Residentes/Empleados que los datos coinciden. Repetible por cualquiera
      con el script.
- [ ] Backups fuera del host (hoy se escriben en disco local del propio
      servidor — si se pierde la máquina, se pierden los backups con ella).
      Sincronizar `Backup:Directorio` a almacenamiento externo (S3/Azure
      Blob/Backblaze) es la pieza que falta, y requiere credenciales de un
      proveedor externo (decisión de negocio, no solo código).
- [ ] Plan de recuperación ante desastres (RPO/RTO) escrito formalmente —
      con backups diarios el RPO real hoy es de hasta 24h; falta
      documentarlo como política explícita.

### 3. Canal de soporte para clientes ✅ (2026-09-23)
- [x] Punto de contacto visible dentro de la app: enlace "Soporte" en la
      navegación de cualquier rol de Empleado (`Controllers/SoporteController.cs`,
      `Views/Soporte/Index.cshtml`) — formulario in-app, sin depender de
      email/SMTP (que la app no tiene configurado).
- [x] Flujo de recepción: panel `/SuperAdmin/Soporte` — ve los tickets de
      todos los centros/organizaciones (con su centro/organización
      indicados) y responde ahí mismo; la respuesta aparece de vuelta en
      `/Soporte` para el empleado que lo reportó. Probado de extremo a
      extremo (crear ticket → responder como SuperAdmin → verificar que
      llega al empleado correcto).
- [x] Documentación mínima de usuario: texto de ayuda integrado en el
      propio formulario ("qué esperabas, qué ha pasado, en qué pantalla").
- [ ] Notificación activa (push/email) al SuperAdmin cuando entra un
      ticket nuevo — hoy hay que entrar a mirar el panel; sin esto no hay
      SLA real de respuesta. Requiere el mismo canal de alertas externo
      pendiente en el bloque 1.

### 4. App instalable / pulir experiencia offline ✅ (base completada 2026-09-23)
- [x] Revisado `manifest.webmanifest` y `sw.js` existentes (Fase 7): ya
      cumplían instalabilidad — sin cambios necesarios ahí.
- [x] Comportamiento offline explícito: `wwwroot/offline.html` (pantalla
      propia, con botón "Reintentar") servida por el service worker cuando
      una navegación de página completa falla por falta de red, en vez del
      error genérico del navegador. Deliberadamente SIN caché de datos
      (avisos/turnos/residentes) — decisión ya tomada en Fase 7 y respetada:
      cachearlos mostraría información obsoleta.
- [x] Banner "Sin conexión" (`wwwroot/js/estado-conexion.js`, visible en
      toda la app autenticada vía `_Layout.cshtml`) que aparece/desaparece
      con los eventos `online`/`offline` del navegador — avisa antes de que
      un formulario falle en silencio. Probado en desktop y en viewport
      móvil (375px).
- [ ] Validado en dispositivo real (Android/iOS) — solo verificado en el
      navegador de este entorno; falta la prueba en un móvil físico antes
      de darlo por definitivo.

### 5. Portal de familiares ✅ (2026-09-23)
- [x] Alcance definido y cerrado: documentos firmados e incidencias ya
      resueltas del residente — nada de avisos internos ni datos clínicos
      (decisión explícita, para no exponer información pensada para
      personal).
- [x] Cuarto esquema de cookie ("Familiar", `Controllers/FamiliarController.cs`)
      — sin CentroId/OrganizacionId, alcance real un único Residente
      (`Models/Familiar.cs`, sin `ITieneCentro`), aislado explícitamente por
      `ResidenteId` en vez de por Centro.
- [x] Alta por invitación: Admin crea el acceso desde
      `/Residentes/Familiares/{id}` (nombre, relación, email, contraseña
      inicial) — mismo patrón manual que Empleados/SuperAdmin, sin depender
      de email transaccional.
- [x] Vistas de solo lectura (`/Familiar/Login`, `/Familiar/Index`)
      probadas de extremo a extremo: Admin invita → familiar entra con sus
      credenciales → ve solo los datos de su residente.
- [x] Revocación de acceso (2026-09-25, cierre de hueco detectado en revisión
      de seguridad post-implementación): desde `/Residentes/Familiares/{id}`,
      Admin puede desactivar/reactivar el acceso y restablecer la
      contraseña — mismo patrón ya usado en Empleados. `FamiliarService`
      valida `familiar.ResidenteId == residenteId` en ambas operaciones
      (si no, un Admin podría tocar el acceso de un familiar de OTRO
      centro adivinando su Id, ya que `Familiar` no lleva filtro de
      tenant). De paso se corrigió un bug real encontrado al probarlo: el
      toggle activar/desactivar usaba `value="@(boolExpr)"` en un input
      oculto, lo que activa el "conditional attribute rendering" de Razor
      y nunca emitía "True"/"False" de verdad — con `.ToString()` se
      evita. Probado de extremo a extremo: desactivar → login falla →
      restablecer contraseña → reactivar → login funciona.
- [ ] Notificar al familiar cuando hay un documento/incidencia nuevo — hoy
      tiene que entrar a mirar; requiere el mismo canal de alertas externo
      pendiente en los bloques 1 y 3.

### 6. Multi-idioma (ES/EN) — infraestructura y selector completados (2026-09-23)
- [x] Infraestructura de recursos de idioma: `IStringLocalizer<SharedResource>`
      + `Resources/SharedResource.en.resx` (español = claves sin traducir,
      cero fichero necesario para que España siga igual). Aplicada a
      `_Layout.cshtml` (navegación completa de los 7 roles + "Salir" +
      tagline) y `Views/Account/Login.cshtml` (campos, botón, título).
- [x] Selector de idioma (`CulturaController.Cambiar`, cookie estándar de
      ASP.NET Core `CookieRequestCultureProvider`) — persiste por
      navegador/dispositivo, visible en el login y en toda la app
      autenticada (`Views/Shared/_SelectorIdioma.cshtml`). Probado de
      extremo a extremo: ES→EN→ES, con el idioma sobreviviendo al login.
- [ ] Traducción completa de los flujos operativos (contenido de cada
      pantalla más allá del login/nav — Avisos, Turnos, Residentes...) —
      deliberadamente fuera de este alcance inicial, tal como ya marcaba
      este mismo checklist ("no bloquea al primer piloto si es en España").
      Con la infraestructura ya en pie, es trabajo incremental: añadir
      `Localizer["..."]` vista a vista y su traducción en el .resx.
- [ ] Etiquetas de rol (`ViewHelpers/RolPresentacion.cs`) siguen en español
      — es una clase estática sin acceso a `IStringLocalizer` hoy; requiere
      convertirla en un servicio inyectable si se quiere traducir.

### 7. Sistema de facturación — deliberadamente aplazado (2026-09-23)
Decisión explícita: nada todavía. La primera empresa de pruebas se da de
alta a mano por SuperAdmin (flujo ya existente desde la Fase 9), sin cobro
automático. Construir facturación real antes de tener un cliente de pago
confirmado es esfuerzo prematuro — se retoma cuando llegue ese momento.
- [ ] Definir planes (por centro, por residente, plano...).
- [ ] Integración con pasarela de pago (Stripe o Redsys).
- [ ] Facturación recurrente + gestión de impagos/cancelación.

## Fuera de este documento (gestión, no código)

Política de privacidad, términos de servicio, contrato de encargado de
tratamiento (DPA), EIPD, seguro de responsabilidad civil, hosting
contratado en la UE. Ver conversación de referencia — son decisiones y
contratos que gestiona el usuario directamente; el asistente puede ayudar
a redactar textos base si se pide explícitamente.
