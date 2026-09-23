# Roadmap — Cuidexa

Este documento traza el camino desde el MVP actual hasta la visión completa de
producto: una plataforma SaaS multi-centro para la gestión integral de
residencias y centros de atención a personas con discapacidad.

## Origen y contexto

Cuidexa nació de un documento de visión mucho más amplio (gestión de
usuarios/residentes, cocina, auxiliares, enfermería, terapias, limpieza,
turnos, notificaciones inteligentes, app móvil, multi-tenant SaaS...). Un
primer intento trató de construir esa visión completa de golpe (Docker,
SQLite, PWA, 7 roles) y resultó sobredimensionado para validar el concepto.

Este MVP 1.0 es la corrección deliberada a ese error: un monolito .NET en
capas simples, PostgreSQL, un único flujo (alta de residente → distribución
automática de avisos), sin Docker ni multi-tenant todavía. Las fases
siguientes reconstruyen progresivamente la visión completa **sobre esta base
técnica**, sin volver a saltar directamente a la complejidad completa.

## Principio transversal: mobile-first para roles operativos

Aplica desde la Fase 0 y en cada fase siguiente, no es una fase aparte:

- Cocina, Auxiliares, Enfermería, Fisio/Logopedia, Limpieza y los dashboards
  de dispositivos compartidos se diseñan y prueban primero en viewport
  móvil/tablet — nunca como ajuste posterior de una vista pensada para
  escritorio.
- Admin y Dirección pueden usar tablas y layouts más densos (uso típico de
  escritorio), pero deben seguir siendo usables al menos en tablet.
- Toda vista nueva de un rol operativo se valida en el navegador a 375px de
  ancho antes de darse por terminada — es criterio de aceptación, no una
  sugerencia.
- Pendiente de revisar: el formulario "Dar de alta" (Admin) es largo y no se
  ha probado en móvil; no es prioritario salvo que también se quiera dar de
  alta desde tablet.

## Restricción de arquitectura (no renegociable sin decisión explícita)

Monolito .NET en capas (`Controller → Service → DbContext → PostgreSQL`), sin
microservicios. Docker vuelve a evaluarse en la Fase 9, pero como tema de
**despliegue**, no de re-arquitectura.

## Fase 0 — MVP núcleo ✅ completada

Admin, Cocina, Auxiliares, Dirección (lectura). Alta de residente →
distribución automática de avisos a Cocina y Auxiliares. Un solo centro, sin
multi-tenant. Validado end-to-end en navegador (2026-09-10).

## Fase 1 — Endurecer para datos reales (single-tenant) ✅ completada

No añade funcionalidad nueva, cierra lo que el propio README marca como "no
producción":

- Sustituir el hash SHA256 por BCrypt con salting automático.
- Auditoría básica (quién cambió qué y cuándo) — obligatorio antes de tocar
  datos de salud reales (RGPD/AEPD).
- Cifrado en reposo (AES-256-GCM) de documento de identidad, teléfono, email
  y contacto de emergencia del residente.
- Gestión de ciclo de vida del residente: baja, traslado, historial de
  habitación (derivado del log de auditoría).

Ampliada a petición del usuario con dos piezas de infraestructura que hacían
falta para poder seguir avanzando (sin ellas, la Fase 2 no podría dar de
alta al personal de Enfermería sin tocar código):

- Edición de los datos de un residente ya dado de alta (todo excepto
  habitación/estado, que mantienen sus flujos dedicados). Los cambios de
  dieta y alergias siguen notificando a Cocina; los de movilidad/asistencia
  a Auxiliares — igual que en el alta.
- Panel de administración de usuarios (Admin): alta, edición, cambio de rol,
  restablecer contraseña, activar/desactivar cuenta. Protegido para que un
  Admin no pueda desactivarse a sí mismo.

Completada y validada en navegador el 2026-09-10. Detalle técnico en la
memoria del proyecto.

## Fase 2 — Enfermería y medicación ✅ completada

- Rol Enfermería.
- Patologías (catálogo + asignación con observaciones por residente).
- Medicación con historial (activa mientras no tenga fecha de fin) y
  registro de cada administración (quién, cuándo).
- "Agenda de hoy": qué medicación toca y si ya se registró como administrada,
  a modo de alertas sin necesidad de un motor de alertas aparte.
- Separación clara entre información clínica (solo enfermería) e información
  operativa (lo que ya reciben auxiliares/cocina): el historial de auditoría
  clínico usa un `EntidadTipo` distinto al del historial que ve Admin, así
  que nunca se mezclan — verificado directamente en base de datos.
- Alta y baja de residente (por Admin) siguen notificando automáticamente a
  Enfermería, igual que ya hacían con Cocina/Auxiliares — Admin no gestiona
  datos clínicos, solo avisa de que hay un residente que revisar.
- Nombre, dosis, horario, instrucciones y observaciones clínicas van
  cifrados en reposo, con el mismo mecanismo de la Fase 1.

Completada y validada en navegador el 2026-09-10.

**Bug de arquitectura encontrado y corregido de paso (no específico de esta
fase):** la ruta por defecto de la app (`Program.cs`) tenía
`{controller=Account}/{action=Login}/{id?}` — el valor por defecto de
`{action}` era "Login" para *todos* los controladores, así que cualquier URL
de un solo segmento ("/Residentes", "/Empleados", "/Enfermeria"...)
devolvía 404 en vez de aterrizar en su Index. Esto llevaba roto desde la
Fase 1 (los enlaces de navegación "Residentes"/"Usuarios" del layout nunca
se habían probado con un clic real). Corregido separando la ruta raíz ("/"
→ login) de la ruta general (por defecto, Index).

## Fase 3 — Profesionales y terapias ✅ completada

Un único rol **Profesional** cubre fisioterapia, logopedia, terapia
ocupacional y psicología — en vez de un rol/controlador por disciplina, un
catálogo de **Especialidad** (asignada al empleado, igual que la contraseña
o el rol) distingue quién es quién. Evita duplicar cuatro roles casi
idénticos y deja la puerta abierta a añadir especialidades nuevas sin tocar
código.

- Sesiones de terapia por residente: programar, marcar realizada (con
  evolución/observaciones cifradas), cancelar. Cada profesional ve y
  gestiona solo sus propias sesiones, aunque otro compañero de la misma
  especialidad atienda al mismo residente.
- "Mi agenda": las sesiones programadas del profesional logueado, en su
  panel principal — mismo patrón que la "Agenda de hoy" de Enfermería.
- Alta y baja de residente (por Admin) notifican automáticamente a
  Profesionales, igual que a Cocina/Auxiliares/Enfermería.
- Auditoría de terapias con su propia etiqueta (`Terapia`, distinta de
  `Clinico` de Enfermería): así el historial de una disciplina no se mezcla
  con el de otra ni con el de enfermería.

Completada y validada en navegador el 2026-09-10.

**Dos bugs reales encontrados y corregidos por el camino:**
- Postgres exige que un `DateTime` guardado en una columna `timestamp with
  time zone` tenga `Kind=Utc`; el campo de formulario `datetime-local` llega
  con `Kind=Unspecified` y la petición fallaba con un 500. Corregido
  marcando la hora explícitamente como Utc antes de guardar — documentado en
  el modelo porque es una hora local "disfrazada" de UTC (nunca se convierte
  de verdad), así que las vistas no deben aplicarle `.ToLocalTime()`.
- La resincronización de secuencia de identidad de Postgres (el mismo
  patrón de la Fase 2) se repitió aquí para el nuevo empleado sembrado y
  para el catálogo de especialidades — ya es la segunda vez que hace falta,
  así que queda como paso obligatorio de cualquier migración que siembre
  filas con Id explícito.

## Fase 4 — Limpieza y espacios ✅ completada

- Rol Limpieza, con tareas ligadas a una habitación (reutiliza el catálogo
  `Habitacion` ya existente) o a una zona común (texto libre — comedor,
  pasillo... sin necesidad de un catálogo nuevo para esto).
- Distribución automática: dar de alta con habitación asignada, trasladar o
  dar de baja generan tanto la tarea de limpieza como el aviso a Limpieza,
  sin que nadie tenga que pedirlo — la habitación que se vacía siempre
  queda con una tarea pendiente.
- Limpieza también puede crear sus propias tareas para zonas comunes
  (limpieza rutinaria no ligada a un alta/baja/traslado).
- Sin auditoría propia: a diferencia de Enfermería/Profesionales, esto es
  información puramente operativa (como los avisos de Cocina/Auxiliares),
  así que el propio estado de la tarea (pendiente/completada, quién, cuándo)
  ya es suficiente rastro — no hacía falta añadir complejidad de más.
- **Mejora de consistencia aprovechada de paso**: el panel de Dirección
  solo mostraba avisos pendientes de Cocina y Auxiliares — un hueco que
  quedó abierto desde que se añadieron Enfermería y Profesionales en las
  Fases 2 y 3. Ahora calcula los pendientes de todos los roles operativos
  genéricamente a partir del enum `RolEmpleado`, así que un rol nuevo
  aparece solo con añadirlo al enum, sin tocar el controlador de Dirección.

Completada y validada en navegador el 2026-09-10.

## Fase 5 — Gestión de turnos ✅ completada

- Turnos por departamento (rol): Admin crea un turno indicando rol, fechas
  y quién lo cubre — un empleado de la plantilla o, si es asistencia
  externa, solo un nombre libre (sin necesidad de un catálogo de personal
  externo completo).
- Cada rol operativo tiene su propia pantalla "Mis turnos": próximos turnos
  visibles, anteriores/cancelados colapsados (mismo patrón que los avisos
  leídos). Desde ahí se puede solicitar un cambio de turno, proponiendo
  opcionalmente quién lo cubre.
- Admin revisa las solicitudes pendientes y aprueba (eligiendo el sustituto
  definitivo, que reasigna el turno automáticamente) o rechaza.
- Histórico: no hace falta una pantalla aparte — se deriva de ver
  Admin todos los turnos (filtrable) y cada empleado sus turnos anteriores
  colapsados.
- Como Cocina y Auxiliares pasaron a tener dos secciones (Avisos + Turnos),
  se convirtieron al mismo patrón dashboard+subpáginas que ya tenían
  Enfermería/Profesionales/Limpieza — consistencia en los 5 roles
  operativos, no una excepción para los que antes solo tenían una cosa.

Completada y validada en navegador el 2026-09-11: ciclo completo probado
(crear turno → empleado solicita cambio → Admin aprueba con sustituto →
turno reasignado → Admin cancela), incluida auditoría de cada paso y
comprobación en móvil.

## Fase 6 — Dashboards para dispositivos compartidos ✅ completada

- `CuentaDispositivo`: cuenta de "tablet compartida" por departamento
  (Cocina/Enfermería/Auxiliares), sin representar a un empleado concreto.
- Tercer esquema de cookie ("Dispositivo"), separado a propósito del de
  Empleado — nunca satisface un `[Authorize(Roles = "...")]` operativo, así
  que "solo consulta" queda garantizado a nivel de framework, no por checks
  sueltos en cada acción.
- Dashboard de solo lectura: personal en turno hoy, avisos pendientes,
  comensales/medicación según el rol de la tablet — pensado para consulta
  rápida durante el turno, no para gestión.

Completada el 2026-09-21.

## Fase 7 — Notificaciones inteligentes + app móvil ✅ completada

- Notificaciones segmentadas: además de por departamento (ya existente desde
  fases anteriores), grupos de notificación ad-hoc (`GrupoNotificacion`) que
  mezclan empleados de cualquier rol dentro de un mismo centro.
- Web Push real (VAPID): un empleado activa notificaciones con un gesto
  explícito (nunca al cargar la página) y recibe avisos aunque no tenga la
  app abierta.
- App móvil resuelta como **PWA responsive**, no nativa: `manifest.webmanifest`
  + service worker (`sw.js`) sobre la base mobile-first ya validada desde la
  Fase 0, sin el salto directo a complejidad completa del primer intento.

Completada el 2026-09-21.

## Fase 8 — Administración avanzada, informes y personalización ✅ completada

- Informes exportables a Excel: turnos/personal, residentes/cuidados,
  actividad/incidencias.
- Personalización de marca (logo, color de acento) — construida primero en
  single-tenant sobre `ConfiguracionCentro`, y reestructurada en la Fase 9 al
  separarse en `Organizacion` (marca compartida) y `Centro` (datos físicos).

Completada el 2026-09-21.

## Fase 9 — Multi-tenant real (SaaS) ✅ completada

La fase arquitectónicamente más grande: de "un único centro" a un modelo
real de dos niveles, **Organización** (marca) → uno o varios **Centro**s
físicos, sobre una única base de datos compartida con filtros globales de
aislamiento (`HasQueryFilter`) en las 19 tablas por centro y las 5 catálogos
compartidos por organización — se mantiene el monolito en capas, sin pasar a
microservicios ni bases de datos separadas por centro.

- Login con código de centro (Empleado y Tablet) + rol nuevo
  `DirectorOrganizacion`: acceso de lectura y escritura completo a todos los
  centros de su organización (Residentes, Usuarios, Turnos, Dispositivos,
  Grupos, Informes), nunca a los de otra organización.
- Esquema de cookie propio para **SuperAdmin**, por encima del modelo de
  tenant: provisión manual de organizaciones/centros nuevos y su primer
  Admin — sin autoservicio público, sin facturación/planes.
- Marca por organización editable tanto por `DirectorOrganizacion` como por
  `SuperAdmin` desde su propio panel (cierra el hueco de una organización
  recién provisionada sin nadie todavía ascendido a `DirectorOrganizacion`),
  válido igual para una organización de un único centro que para una con
  varios.
- Endurecimiento de seguridad de paso: protección CSRF global, rate limiting
  por IP + bloqueo de cuenta tras fallos repetidos (con recuperación vía
  reseteo de contraseña por Admin, sin necesitar email), secretos movidos de
  `appsettings.json` a `dotnet user-secrets`, cabeceras de seguridad HTTP,
  política mínima de contraseña, y bloqueo de subida de SVG en el logo.
- Docker se deja fuera de esta fase (tema de despliegue a reevaluar aparte,
  no de arquitectura), tal como marca la restricción del roadmap.

Completada y validada en navegador el 2026-09-22/23: aislamiento cruzado
probado entre tres centros (dos organizaciones, una de ellas con dos
centros) en Residentes/Usuarios/Turnos/Dispositivos/Grupos, incluyendo
casos negativos explícitos (por ejemplo, un grupo de notificación no puede
mezclar miembros de dos centros distintos).

## Fase 10 — Backlog evolutivo (abierto, no bloqueante)

Incidencias, firma digital, inventario, integración con otros sistemas, IA
para informes/predicción de personal. No es necesario para llamar
"completo" al producto SaaS v1 — es evolución continua.
