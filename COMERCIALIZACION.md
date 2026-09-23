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

### 1. Monitorización/logging
- [ ] Logging estructurado en producción (Serilog o similar) con niveles y
      contexto (CentroId, EmpleadoId, ruta).
- [ ] Captura de excepciones no controladas con alerta (Sentry/Application
      Insights o equivalente autoalojado).
- [ ] Panel o comando de salud (`/health`) para verificar BD y estado del
      servicio.
- [ ] Alertas básicas (caída del servicio, tasa de error elevada).

### 2. Backups automatizados
- [ ] Backup automático diario de PostgreSQL, cifrado en reposo.
- [ ] Retención definida (ej. 30 días diarios + 12 meses mensuales).
- [ ] Prueba real de restauración documentada (un backup que nunca se ha
      restaurado no es un backup fiable).
- [ ] Plan de recuperación ante desastres (RPO/RTO) escrito.

### 3. Canal de soporte para clientes
- [ ] Punto de contacto visible dentro de la app (formulario o enlace).
- [ ] Flujo de recepción (email dedicado o bandeja compartida).
- [ ] Documentación mínima de usuario (cómo reportar un problema).

### 4. App instalable / pulir experiencia offline
- [ ] Revisar manifest.webmanifest y service worker existentes.
- [ ] Comportamiento offline explícito (qué funciona sin red, qué avisa al
      usuario en vez de fallar en silencio).
- [ ] Validado en dispositivo real (Android/iOS), no solo en navegador de
      escritorio.

### 5. Portal de familiares
- [ ] Definir alcance (qué ve un familiar: documentos firmados,
      incidencias resueltas relevantes, fotos, contacto).
- [ ] Nuevo tipo de acceso/autenticación (no es un Empleado ni un
      Dispositivo — esquema de cookies propio, como SuperAdmin/Dispositivo).
- [ ] Vistas de solo lectura, aisladas por residente (un familiar nunca ve
      a otro residente).

### 6. Multi-idioma (ES/EN)
- [ ] Infraestructura de recursos de idioma (resx o similar) en vistas
      compartidas primero (login, layout, nav).
- [ ] Selector de idioma por usuario/dispositivo.
- [ ] Traducción completa de los flujos operativos (no solo Admin).

### 7. Sistema de facturación
- [ ] Definir planes (por centro, por residente, plano...).
- [ ] Integración con pasarela de pago (Stripe o Redsys).
- [ ] Facturación recurrente + gestión de impagos/cancelación.

## Fuera de este documento (gestión, no código)

Política de privacidad, términos de servicio, contrato de encargado de
tratamiento (DPA), EIPD, seguro de responsabilidad civil, hosting
contratado en la UE. Ver conversación de referencia — son decisiones y
contratos que gestiona el usuario directamente; el asistente puede ayudar
a redactar textos base si se pide explícitamente.
