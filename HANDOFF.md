# Encargo para Claude Code — cierre de autenticación y sesiones en Identity

Documento temporal de esta sesión. Cuando el trabajo termine, bórralo; lo permanente es `CLAUDE.md` y `docs/identity-auth-spec.md`. Lee primero esos dos archivos completos.

## 1. Objetivo

1. **Verificar** que las acciones de Identity que ya existen están bien construidas y son consistentes con la especificación.
2. **Terminar** las acciones pendientes de autenticación y sesiones, integrando la implementación de referencia.
3. **Auditar** el proyecto completo y **corregir** lo que esté mal, con el criterio de autonomía de la sección 3.

## 2. Estado de partida (honesto)

- La implementación base de JWT ya fue aplicada al código: servicio de sesiones, generadores de access y refresh token, opciones y validación de JWT Bearer, políticas por permiso. **Ninguna acción la usa todavía.**
- Existen en `Features`: Auth/Commands con `ForgotPassword`, `LoginUser`, `LogoutUser`, `SetUserPassword`, `ValidateToken` (esta última es en realidad una query); Users/Commands con `RegisterUser` y `ResentInvite`; Users/Querys con `GetUserById`. Auth/Querys está vacía. El contenido interno de esas acciones **no ha sido revisado**.
- La implementación de referencia de las 11 acciones nuevas está en `_reference/identity-session-actions` (ver sección 4). **Nunca se compiló**: se escribió sin SDK de .NET y sin ver el código real de `Shared` ni de los handlers existentes. Trátala como borrador de alta calidad, no como verdad.
- La migración `Initial` del módulo ya está aplicada en la base local. Crea en el schema `Identity` las tablas users, roles, permissions, users_roles, roles_permissions, sessions y password_tokens.

## 3. Reglas de esta sesión

**Orden obligatorio:** primero solo lectura y un informe; después cambios. No modifiques nada en la Fase 0.

**Autonomía.** Corrige directamente, sin preguntar: errores de compilación, advertencias de nulabilidad, ortografía de nombres y namespaces (`Querys` por `Queries`, `ResentInvite` por `ResendInvite`, `Sessiones` por `Sessions`), `using` faltantes, `CancellationToken` no propagado, registros de DI faltantes, y desviaciones mecánicas de la convención de acciones. **Pregunta antes** de: cambios de esquema o migraciones, cambios en referencias entre proyectos, borrar proyectos o archivos con contenido, cambiar una decisión registrada en `CLAUDE.md` o en la especificación, agregar paquetes NuGet, y cualquier decisión de diseño con alternativas reales (en ese caso expón opciones, recomienda una y espera).

**Prohibido sin permiso explícito:** aplicar migraciones a la base de datos, editar la migración `Initial`, `git push`, cambiar de rama, reescribir historial.

**Adapta la referencia al repositorio, nunca el repositorio a la referencia.** Si la referencia choca con una convención real del proyecto, gana el proyecto, salvo que la convención sea un error de seguridad; en ese caso repórtalo.

**Evidencia.** Cada hallazgo cita archivo y línea. Cada afirmación de "funciona" se respalda con compilación o pruebas ejecutadas, no con lectura.

## 4. Material de referencia

Descomprime `identity-session-actions.zip` en la raíz del repositorio, en una carpeta `_reference/identity-session-actions`, **fuera de los proyectos** para que no se compile. Contiene, mirando la estructura de proyectos: contratos, DTOs, excepciones, validación de contraseña, versión ampliada del servicio de sesiones, las 11 acciones (Command o Query, Handler, Validator, Response), repositorios de lectura y escritura, tres controladores, requests, extensiones de claims y de contexto de cliente, y `PermissionCodes` ampliado.

Cinco archivos reemplazan versiones que ya están en el repositorio y deben **fusionarse**, no copiarse a ciegas: `IAuthSessionService`, `AuthSessionService`, `ISessionRepository`, `SessionRepository` y `PermissionCodes`. Compara contra la versión del repositorio y conserva cualquier cambio local legítimo.

## 5. Suposiciones de la referencia que debes verificar contra el código real

1. Interfaces de `Shared/CQRS` y `Shared/Mediator`: nombres, genéricos, firma del método del handler, y si existen un emisor (`ISender`) y un tipo `Unit`.
2. Contratos existentes en Application: repositorio de usuarios con lectura por id con tracking, el servicio de hashing de contraseñas (nombres de los métodos de hash y verificación) y la unidad de trabajo.
3. Excepciones base: existencia de tipos para 404 y 422, sus constructores y el mapeo a HTTP en el manejador global. Existencia de la excepción 401 para refresh inválido.
4. Cómo se ejecuta FluentValidation: pipeline del mediator o llamada manual.
5. Dónde viven realmente los Requests HTTP del proyecto y los nombres de carpetas.
6. En el lector de roles y permisos: nombres reales de las entidades de roles y permisos y de sus propiedades (nombre del rol, código del permiso, navegaciones); y nombres de los `DbSet` del contexto.
7. Si los controladores ya existen con otro nombre o ruta base, para fusionar acciones en lugar de duplicar rutas.

## 6. Fases

### Fase 0 — Orientación y auditoría (solo lectura)

Entrega un informe en `docs/handoff/AUDITORIA.md` con:

- Mapa real de proyectos y referencias entre ellos; señala referencias que violen las reglas de dependencia y proyectos heredados.
- La convención real de acciones, tal como la implementan los handlers existentes (qué interfaces, qué estilo de errores, dónde validan).
- Resultado de compilar la solución y de ejecutar las pruebas, si las hay.
- Cada acción existente comparada con su fila de la especificación (secciones 6.1 y 6.2): cumple, cumple parcialmente o no cumple, con evidencia.
- Lista de hallazgos del proyecto clasificados en **bloqueante**, **importante** y **menor**, con recomendación.
- Respuestas a las suposiciones de la sección 5.

Detente y espera la confirmación de Sebastián sobre el informe antes de seguir.

### Fase 1 — Integrar la referencia

- Fusionar los cinco archivos que reemplazan versiones existentes; copiar el resto adaptando a las convenciones reales.
- Registrar en DI los repositorios de lectura. Confirmar que los handlers y validators se descubren con el mecanismo actual.
- Compilar hasta cero errores y cero advertencias nuevas.
- Aplicar los renombrados mecánicos de la sección 7.

### Fase 2 — Verificar y corregir las acciones existentes

Para cada acción existente, cerrar las brechas contra la especificación. Puntos que casi seguro faltan, a confirmar en la auditoría:

- **LoginUser**: usa el servicio de sesiones para emitir tokens; mensaje de error único; igualación de tiempos; recibe el contexto del cliente dentro del command; respuesta con tokens y sin caché.
- **LogoutUser**: usa la revocación por refresh token; idempotente; acepta anónimo.
- **SetPassword**: revoca todas las sesiones siempre; consume el token de forma atómica; invalida los demás tokens pendientes; no hace login automático; aplica la regla de contraseña compartida.
- **ForgotPassword**: respuesta y tiempo constantes; no espera el envío del correo; no revoca sesiones; invalida resets anteriores.
- **ValidateToken**: pasa a Queries con el nombre `ValidatePasswordToken`, no consume el token, respuesta indistinguible entre causas.
- **RegisterUser**: nombre `CreateUser`, sin contraseña, `hash_password` nulo, token invite hasheado, evento publicado.
- **ResendInvite**: solo si no tiene contraseña; invalida invites previos.
- Todas las rutas protegidas con la política de permiso que indica la especificación y todas las públicas con `AllowAnonymous` explícito.

### Fase 3 — Completar lo que falta

- Las 11 acciones nuevas ya integradas en la Fase 1: verificar cada una contra su fila de la especificación (6.3).
- Pendientes transversales de la sección 9 de la especificación, en este orden: bootstrap del primer administrador y seed de permisos (incluidos `users.update`, `sessions.read` y `sessions.revoke`), rate limiting, `ProblemDetails` para 401 y 403, evento de contraseña cambiada, reenvío de cabeceras, tarea de purga de sesiones y tokens expirados.
- Fusionar el orden correcto del pipeline en el host: manejo de excepciones, reenvío de cabeceras, HTTPS, autenticación, autorización, controladores.

### Fase 4 — Pruebas

Implementar los casos de la sección 10 de la especificación. Las pruebas de rotación concurrente y de reuso deben correr contra PostgreSQL real (contenedor), no contra proveedor en memoria, porque dependen de actualizaciones condicionales. Reporta cuáles no se pudieron ejecutar y por qué.

### Fase 5 — Cierre

Compilación limpia, pruebas en verde, resumen por fase de lo hecho, lista de decisiones pendientes para Sebastián y lista de lo que no se pudo verificar.

## 7. Hallazgos ya conocidos (confirmar y corregir)

**Bloqueante**
1. En la configuración de la tabla `sessions`, el índice llamado `idx_sessions_user_id` está construido sobre el id de la sesión (ya es clave primaria) en lugar del id de usuario. Corregir con una **migración nueva**; no editar `Initial`. Preguntar antes de generar la migración.
2. Hasta que las acciones usen el servicio de sesiones, ningún token se emite ni se renueva. Resolver en la Fase 2.

**Importante**
3. No existe el primer administrador: las altas requieren un admin con permiso y los usuarios nacen sin contraseña.
4. Sin límite de tasa en los endpoints públicos sensibles.
5. Sin purga de sesiones ni tokens de password vencidos.
6. Inconsistencia posible entre `DateTime` (entidades) y `DateTimeOffset` (decisión documentada en arquitectura). Reportar y preguntar; no cambiar en silencio.
7. Propiedad de navegación `Sessiones` en `User`; el nombre correcto es `Sessions` (cambio sin migración, pero toca la configuración de `Session`).
8. Si `Identity.Api` u otros proyectos heredados siguen en la solución como proyectos Web o duplicados, reportar.

**Menor**
9. Renombrados: carpetas `Querys` a `Queries`, `ResentInvite` a `ResendInvite`, `ValidateToken` a `ValidatePasswordToken` (y a Queries), `RegisterUser` a `CreateUser`, `SetUserPassword` a `SetPassword`. Actualizar rutas y namespaces; no cambia comportamiento.
10. Longitud de `device_info` (100) frente a User-Agents reales más largos: la entidad ya trunca; confirmar que la configuración y la entidad usan la misma constante.

## 8. Entregables

1. `docs/handoff/AUDITORIA.md` (Fase 0).
2. Código integrado, compilando y con pruebas.
3. Una migración nueva para el índice de sesiones, **generada pero sin aplicar**, con la orden exacta para aplicarla.
4. Actualización de `CLAUDE.md` y `docs/identity-auth-spec.md` si alguna decisión cambió durante el trabajo.
5. Resumen final con decisiones pendientes.

## 9. Primer mensaje sugerido para Claude Code

Lee `CLAUDE.md`, `docs/identity-auth-spec.md` y `HANDOFF.md` completos. Ejecuta únicamente la Fase 0: no modifiques ningún archivo salvo crear `docs/handoff/AUDITORIA.md`. Cuando termines, resume los hallazgos bloqueantes e importantes y las respuestas a las suposiciones de la sección 5, y espera mi confirmación antes de continuar.
