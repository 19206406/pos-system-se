# Especificación de autenticación y autorización — módulo Identity

Documento de referencia permanente. Define el diseño, el comportamiento exacto de cada acción y los casos de prueba. Está escrito sin código a propósito: las decisiones de implementación las toma Sebastián, apoyado en este documento.

## 1. Modelo general

- **Access token**: JWT firmado, corta duración (15 minutos), sin estado en servidor. Se valida en cada request solo con la firma y sus claims; no se consulta la base de datos.
- **Refresh token**: valor opaco aleatorio (no es un JWT). Solo su hash vive en la tabla `sessions`. Sirve únicamente para obtener un par nuevo de tokens.
- **Sesión**: una fila de `sessions` por refresh token activo. Un usuario puede tener varias sesiones simultáneas (varios dispositivos).
- **Rotación**: cada refresh invalida el token usado y crea una sesión nueva. La sesión vieja queda con `revoked_at` y `replaced_by_id` apuntando a la nueva.
- **Vencimiento absoluto**: la sesión nueva hereda el `expires_at` de la anterior. Una cadena de sesiones muere a los N días del login sin importar cuántas veces rote. (Alternativa deslizante descartada por seguridad; revisar solo si Sebastián lo pide.)
- **Detección de reuso**: si llega un refresh token ya rotado (revocado con `replaced_by_id`), se asume robo y se revocan todas las sesiones activas del usuario. Si llega uno revocado por logout (sin `replaced_by_id`), solo se rechaza.
- **Rotación atómica**: marcar la sesión vieja como reemplazada es una actualización condicional ("solo si sigue sin revocar"). Si otra petición ganó la carrera, se descarta la sesión recién creada y se rechaza.

## 2. Piezas y quién las usa

| Pieza | Capa | La usa | No debe usarla |
|---|---|---|---|
| `IAuthSessionService` | Application | Handlers de login, refresh, logout, logout-all, revocar sesión, cambiar contraseña, set-password, desactivar usuario | Controladores |
| Generadores de access y refresh token | Infrastructure | Solo `AuthSessionService` | Handlers, controladores |
| `IUserAccessReader` (roles y permisos) | Persistence | `AuthSessionService` y la query de usuario actual | Otros handlers |
| `ISessionRepository` (escritura) | Persistence | Solo `AuthSessionService` | Handlers |
| `ISessionReadRepository`, `IUserReadRepository` (lectura) | Persistence | Queries | Comandos de escritura |
| Opciones JWT | Application (clase), Infrastructure (registro) | Generadores y configuración de JWT Bearer | Handlers |
| Configuración de JWT Bearer y políticas | Presentation | El middleware | Handlers |
| `IPasswordTokenIssuer` (tokens invite y reset) | Application (contrato), Infrastructure | Solo los handlers del outbox que entregan invitaciones y resets | Acciones HTTP |
| `IOutbox` (escritura de mensajes) | Application (contrato), Persistence | Acciones que provocan un correo: CreateUser, ResendInvite, ForgotPassword, SetPassword, ChangePassword, y el arranque del primer admin | Handlers del outbox |
| `IOutboxStore` y `OutboxMessageProcessor` | Persistence y Application | Solo el procesador en segundo plano | Acciones HTTP |

Los tokens de password (`invite`, `reset`) son un mecanismo distinto, con su propia tabla. No reutilizan los generadores del refresh token, y no los generan las acciones HTTP sino la entrega del outbox (sección 11).

## 3. Claims del access token

`sub` (id de usuario), `jti`, `email`, `name`, `sid` (id de la sesión que lo originó), `role` (uno por rol), `permission` (uno por código de permiso). Emisor y audiencia vienen de configuración.

Validación en el host: emisor, audiencia, firma, expiración y algoritmo fijado (HS256); tolerancia de reloj de 30 segundos; sin mapeo automático de claims entrantes (los claims conservan su nombre original). Un token con firma inválida, otro algoritmo, otra audiencia o vencido produce 401.

HS256 es válido mientras la misma aplicación emite y valida. Si algún día un servicio separado solo valida tokens, migrar a firma asimétrica.

## 4. Configuración (`appsettings.json`)

Sección `Jwt`:

Valores: emisor, audiencia, clave de firma, duración del access token en minutos (15), duración absoluta de la sesión en días (7), tolerancia de reloj en segundos (30).

- La clave de firma queda **vacía en el repositorio**. Desarrollo: user-secrets. Docker y producción: variable de entorno `Jwt__SigningKey` en el override de compose o secreto del orquestador. Mínimo 32 caracteres.
- Las opciones se validan al arrancar. Con la clave vacía o corta, la aplicación no inicia.

Otras secciones del módulo, todas validadas al arrancar:

- `PasswordTokens`: duración del invite en horas (72) y del reset en minutos (30).
- `IdentityLinks`: URL base del frontend y ruta de la página que llama a set-password; con ellas se arman los enlaces de los correos. Obligatoria.
- `Smtp`: servidor, puerto, TLS, remitente y, si hacen falta, credenciales (en secretos, nunca en el repositorio). Obligatoria. En desarrollo apunta a Mailpit.
- `Outbox`: intervalo de sondeo, tamaño de lote, intentos máximos y duración del bloqueo de un mensaje.
- `Retention`: días que se conservan sesiones, tokens y mensajes vencidos o usados antes de purgarlos (30), e intervalo de la purga.
- `Bootstrap`: email y nombre del primer administrador; vacío desactiva el arranque.
- `IdentityRateLimits`: permisos por ventana de cada política (sección 9).
- `ForwardedHeaders:KnownProxies`: direcciones de los proxies de confianza.
- La connection string `PosSystemDb` es un secreto: user-secrets en desarrollo, variable de entorno o secreto del orquestador en producción.

## 5. Autorización

- Permisos como códigos de texto (por ejemplo `users.read`, `users.create`, `users.update`, `sessions.read`, `sessions.revoke`) definidos en `PermissionCodes`. Cada código debe existir en tres sitios: la tabla `permissions` (seed), el rol correspondiente y la lista `PermissionCodes.All`. Si falta en la lista, la política no existe y usarla produce un 500.
- Una política por código, que exige el claim `permission` con ese valor.
- **Política de respaldo**: todo endpoint sin atributo exige usuario autenticado. Los públicos llevan `[AllowAnonymous]` explícito, incluido el endpoint de health checks.
- Los permisos viajan en el token: un cambio de rol se refleja en el siguiente refresh (máximo 15 minutos). Si se retira un privilegio sensible y debe aplicar de inmediato, revocar las sesiones del usuario afectado.
- Desactivar un usuario revoca sus sesiones, pero su access token vigente funciona hasta que expire (máximo 15 minutos). Decisión aceptada para un POS; si deja de serlo, reducir la duración del access token.

## 6. Matriz de acciones

Todas las rutas llevan el prefijo del módulo, `/api/identity` (decisión D3 de la auditoría): por ejemplo, `POST /auth/login` se expone como `POST /api/identity/auth/login`. El prefijo por módulo evita colisiones cuando existan Ventas, Menú y los demás.

### 6.1 Autenticación y contraseña

| Acción | Ruta | Acceso | Comportamiento exacto | Resultados |
|---|---|---|---|---|
| LoginUser | `POST /auth/login` | Anónimo | Busca por email; verifica Argon2id; si el email no existe ejecuta igual una verificación contra un hash ficticio para igualar tiempos; mismo error para: usuario inexistente, contraseña incorrecta, `hash_password` NULL, usuario inactivo; en éxito crea sesión con el contexto del cliente (User-Agent e IP) | 200 con tokens y `no-store`; 401 genérico; 400 validación; 429 |
| LogoutUser | `POST /auth/logout` | Anónimo (se identifica con el refresh token) | Revoca esa sesión; idempotente; no requiere access token vigente | 204 siempre |
| ForgotPassword | `POST /auth/forgot-password` | Anónimo | Respuesta idéntica exista o no el correo; si existe y está activo, solo registra en el outbox una solicitud de reset (se permite si `hash_password` es NULL). La entrega invalida **todos** los tokens pendientes del usuario, de cualquier tipo (decisión D9), crea el token `reset` y envía el correo; la respuesta nunca espera el envío; **no** revoca sesiones. Diferencia de tiempo residual: cuando el correo existe hay una escritura más, que el rate limiting mitiga | 202 genérico; 429 |
| ValidatePasswordToken | `GET /auth/password-token/validate` | Anónimo | Es una query: no consume el token. Devuelve si es válido y su tipo (`invite` o `reset`); mismo resultado para inexistente, expirado o usado; sin datos del usuario | 200 con indicador de validez; 429 |
| SetPassword | `POST /auth/set-password` | Anónimo | Una sola acción para invite y reset; el token identifica al usuario (no se envía email). Valida token (existe, no usado, no expirado) y usuario activo; lo marca usado de forma atómica; aplica la regla de contraseña nueva; guarda hash Argon2id; invalida los demás tokens pendientes del usuario; **revoca todas las sesiones siempre** (sin ramificar por tipo de token); no hace login automático; registra en el outbox el aviso de contraseña cambiada. Todo en una transacción | 204; 400 token inválido o expirado (mensaje genérico); 429 |

Duración recomendada de tokens de password: invite 48 a 72 horas, reset 15 a 30 minutos, configurables.

### 6.2 Administración de usuarios

| Acción | Ruta | Permiso | Comportamiento exacto |
|---|---|---|---|
| CreateUser | `POST /users` | `users.create` | Sin campo de contraseña; `hash_password` NULL; email normalizado (minúsculas, sin espacios); registra la invitación en el outbox en la misma transacción que el usuario (la entrega crea el token `invite`); email duplicado es error de negocio (422) |
| ResendInvite | `POST /users/{id}/resend-invite` | `users.create` | Solo si el usuario aún no tiene contraseña (si ya la tiene, error de negocio 422); registra una nueva invitación en el outbox, cuya entrega invalida todos los tokens pendientes (D9) y crea uno nuevo; 404 si no existe |
| GetUserById | `GET /users/{id}` | `users.read` | 404 si no existe; la respuesta nunca incluye el hash |

### 6.3 Sesiones y administración ampliada

| Acción | Tipo | Ruta | Acceso | Comportamiento exacto |
|---|---|---|---|---|
| RefreshToken | Command | `POST /auth/refresh` | Anónimo | Canjea refresh por un par nuevo; rota; reuso revoca todo; `no-store`; 401 genérico ante cualquier fallo, incluido un token vacío o ausente (sin validator que dé 400) |
| LogoutAllSessions | Command | `POST /auth/logout-all` | Autenticado | Revoca todas las sesiones del usuario del token, incluida la actual |
| ChangePassword | Command | `POST /auth/change-password` | Autenticado | Verifica contraseña actual (422 si es incorrecta, nunca 401); la nueva debe cumplir la regla y diferir de la actual; guarda primero, revoca después las demás sesiones (conserva la actual por `sid`); sin contraseña previa o cuenta inactiva es error de negocio |
| RevokeMySession | Command | `DELETE /auth/sessions/{id}` | Autenticado | Revoca una sesión solo si es del usuario del token; ajena, inexistente o ya revocada dan el mismo 404 |
| GetCurrentUser | Query | `GET /auth/me` | Autenticado | Perfil más roles y permisos leídos de la base de datos (no del token) |
| GetMySessions | Query | `GET /auth/sessions` | Autenticado | Sesiones activas (no revocadas, no expiradas), marca la actual por `sid`; expone fecha del último refresh y vencimiento; **nunca** el hash del token |
| GetUsers | Query | `GET /users` | `users.read` | Paginada (página mínima 1, tamaño 1 a 100), filtro por texto en nombre o email y por estado, orden estable; el texto se escapa para que `%` y `_` no actúen como comodines |
| DeactivateUser | Command | `POST /users/{id}/deactivate` | `users.update` | Un admin no puede desactivarse a sí mismo (422); marca inactivo; revoca todas las sesiones siempre (idempotente) |
| ReactivateUser | Command | `POST /users/{id}/reactivate` | `users.update` | Marca activo; no restaura sesiones |
| GetUserSessions | Query | `GET /users/{id}/sessions` | `sessions.read` | 404 si el usuario no existe; mismas reglas de proyección que GetMySessions |
| RevokeUserSessions | Command | `DELETE /users/{id}/sessions` | `sessions.revoke` | 404 si el usuario no existe; revoca todas sin desactivarlo |

## 7. Regla de contraseña nueva

Longitud mínima 10 y máxima 128 (el máximo evita entradas enormes contra Argon2id). Una única regla compartida debe aplicarse en SetPassword y ChangePassword.

## 8. Códigos HTTP y manejo de errores

400 validación (FluentValidation, formato `ProblemDetails`), 401 no autenticado o refresh inválido, 403 sin permiso, 404 no encontrado, 422 regla de negocio, 429 límite de tasa, 500 solo ante errores reales. Los 401 y 403 del middleware de autenticación también deben salir como `ProblemDetails` consistente.

## 9. Piezas transversales

1. Bootstrap del primer administrador. Los permisos de `PermissionCodes.All`, el rol Admin y su relación se crean con el seed de la migración. Mientras nadie tenga el rol Admin, al arrancar se asigna ese rol al email de `Bootstrap:AdminEmail` (se crea el usuario si no existe) y se le envía una invitación.
2. Rate limiting nativo, por IP:
   - Login, set-password y cambio de contraseña: 5 por minuto.
   - Forgot-password y validación de token: 5 cada 15 minutos.
   - Refresh: 30 por minuto.

   Todos son configurables. El rechazo es 429 con `Retry-After` y `ProblemDetails`.
3. Purga en segundo plano, cada hora: sesiones vencidas o revocadas, tokens de password vencidos o usados y mensajes del outbox entregados, todos con más de 30 días. Los mensajes no entregados se conservan para inspección.
4. Aviso de "contraseña cambiada" por correo, a través del outbox, tras SetPassword y ChangePassword.
5. Reenvío de cabeceras (`UseForwardedHeaders`) con lista de proxies de confianza; sin ella, la IP registrada y la del rate limiting es la del proxy.
6. Pruebas automatizadas (sección 10).

## 10. Casos de prueba que deben existir

**Login y sesiones**
- Login correcto devuelve tokens; en base de datos existe una sesión cuyo `token_hash` no es el token entregado.
- Contraseña incorrecta, email inexistente, usuario sin contraseña y usuario inactivo devuelven respuestas indistinguibles.

**Refresh**
- Refresh correcto: token nuevo, sesión vieja revocada con `replaced_by_id`, nueva sesión con el mismo vencimiento.
- Reusar un refresh ya rotado: 401 y todas las sesiones activas del usuario quedan revocadas.
- Usar un refresh revocado por logout: 401 y las demás sesiones siguen activas.
- Refresh expirado, inexistente, vacío o de usuario inactivo: 401 con el mismo mensaje.
- Dos refresh simultáneos con el mismo token: exactamente uno tiene éxito.

**Logout y revocaciones**
- Logout es idempotente. Logout-all deja cero sesiones activas.
- Revocar la sesión de otro usuario devuelve 404 y no la revoca.
- GetMySessions marca la sesión actual y no expone hashes.

**Contraseñas**
- SetPassword con reset revoca todas las sesiones; el token no puede usarse dos veces, ni en paralelo.
- ForgotPassword no revoca sesiones y responde igual para correos existentes e inexistentes.
- ChangePassword con actual incorrecta da 422; con éxito revoca las demás sesiones y conserva la actual.

**Autorización**
- Sin token: 401. Con token sin el permiso: 403. Con el permiso: éxito.
- Token vencido, con audiencia distinta o con algoritmo distinto: 401.
- Un endpoint nuevo sin atributo exige autenticación (política de respaldo).
- Desactivar un usuario revoca sus sesiones; un admin no puede desactivarse a sí mismo.

**Arranque**
- Con la clave de firma vacía o menor a 32 caracteres, la aplicación no inicia.

Estos casos están implementados en `tests/Identity.IntegrationTests`, contra PostgreSQL real (Testcontainers), junto con los del flujo de invitación, los reintentos del outbox, el arranque del primer administrador, la purga y el rate limiting.

## 11. Correo y efectos secundarios (outbox)

- Las acciones no envían correos ni generan tokens de password. Registran un mensaje en `identity.outbox_messages` en la misma transacción que el cambio que lo provoca: si el cambio se guarda, el mensaje también; si no, ninguno de los dos.
- Mensajes actuales: invitación solicitada, reset solicitado y contraseña cambiada. Cada uno lleva solo el id del usuario. **Ningún payload contiene tokens ni contraseñas.**
- Un procesador en segundo plano reclama los mensajes pendientes con una actualización condicional (bloqueo con vencimiento), así que funciona con varias instancias. Entrega cada mensaje en su propio alcance de inyección de dependencias.
- Para invitación y reset, la entrega invalida los tokens pendientes del usuario, genera el token, guarda solo su hash y después envía el correo con el enlace. Si el envío falla, el mensaje se reintenta. Cada reintento emite un token nuevo e invalida el anterior, de modo que solo funciona el último enlace enviado.
- La entrega es "al menos una vez": tras una caída entre el envío y la marca de procesado, el usuario puede recibir un segundo correo, y el primer enlace deja de funcionar.
- Tras el número máximo de intentos el mensaje queda sin entregar, con el último error, para inspección. Los entregados se purgan a los 30 días.
