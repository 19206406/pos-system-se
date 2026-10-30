# Auditoría Fase 0 — Identity (autenticación y sesiones)

Fecha: 2026-10-30. Rama: `DEV/0.0.1/pos-system` (último commit `18b0e`, con cambios sin confirmar en el árbol de trabajo).
Alcance: lectura completa de `Shared`, host `PosSystem.Api` y los cinco proyectos de Identity, más la referencia en `C:\Users\urreg\Downloads\identity-session-actions` (no existe `_reference/` en el repositorio; no la copié para no modificar nada en esta fase).

Única modificación en el repositorio: este archivo. Las compilaciones de diagnóstico adicionales se hicieron sobre una **copia** en el directorio temporal de la sesión, nunca sobre el repositorio.

---

## 0. Resumen ejecutivo

- **La solución no compila** (1 error). Corrigiendo ese error en una copia, compila, pero **el host no arranca** por dos causas encadenadas (DI y opciones JWT).
- **No hay autenticación ni autorización en el pipeline**: no existe configuración de JWT Bearer, ni políticas, ni `UseAuthentication`/`UseAuthorization`. Todos los endpoints son públicos, incluido `POST /users`.
- **El schema de la base aplicada es `"identity  "` (con dos espacios)**, y el último commit cambió el código a `"identity"`. Con la base actual, las consultas y las migraciones apuntan a un schema que no existe.
- **SetPassword permite tomar cualquier cuenta**: no comprueba que el token pertenezca al usuario del email enviado.
- El estado real ya no coincide con la sección 2 del HANDOFF: buena parte de la referencia **ya está integrada** a medias (10 de las 11 acciones nuevas existen en `Features`), los renombrados están hechos a medias y no hay proyectos heredados.
- No hay proyectos de pruebas.

---

## a) Mapa real de proyectos y referencias

Solución `POS-system.slnx` (7 proyectos .NET y `docker-compose.dcproj`). Todos `net10.0`, `Nullable` y `ImplicitUsings` activos.

| Proyecto | SDK | Referencias de proyecto | Paquetes / framework |
|---|---|---|---|
| `Shared` | `Microsoft.NET.Sdk` | — | FluentValidation 12.1.1, FluentValidation.DependencyInjectionExtensions 12.1.1, **FrameworkReference `Microsoft.AspNetCore.App`** |
| `Identity.Domain` | `Microsoft.NET.Sdk` | — | — |
| `Identity.Application` | `Microsoft.NET.Sdk` | Shared, Domain | — |
| `Identity.Infrastructure` | `Microsoft.NET.Sdk` | Application | Konscious Argon2 1.3.1, Microsoft.IdentityModel.JsonWebTokens 8.23.0 |
| `Identity.Persistence` | `Microsoft.NET.Sdk` | Application, Domain | Npgsql.EFCore.PostgreSQL 10.0.3, EFCore.Tools 10.0.12 |
| `Identity.Presentation` | `Microsoft.NET.Sdk` | Application, **Infrastructure, Persistence** | FrameworkReference `Microsoft.AspNetCore.App` |
| `PosSystem.Api` | `Microsoft.NET.Sdk.Web` | **solo Presentation** | FluentValidation, OpenApi, EFCore.Tools 10.0.12, Scalar, Containers.Tools |

### Violaciones de las reglas de dependencia (CLAUDE.md, "Reglas de dependencia")

1. **Presentation referencia a Infrastructure y Persistence** (`Identity.Presentation.csproj`, `ItemGroup` de `ProjectReference`, líneas 15-19). La regla dice que esas tres capas "no se referencian entre sí" y que "solo el host referencia todas las capas". Hoy el host referencia solo Presentation, y `IdentityModuleRegistration.AddIdentityModule` (`Identity.Presentation/IdentityModuleRegistration.cs:11-21`) compone las cuatro capas. Es un patrón defendible ("el módulo se compone a sí mismo"), pero contradice lo documentado. Ver decisión D2.
2. **`Shared` arrastra `Microsoft.AspNetCore.App`** (`Shared.csproj`, `FrameworkReference`). Como Application referencia Shared, Application puede ver `HttpContext` de forma transitiva, contra la regla "Application nunca ve `HttpContext`". Ningún archivo de `Shared` usa tipos de ASP.NET Core (solo `Microsoft.Extensions.DependencyInjection`, que llega por FluentValidation.DependencyInjectionExtensions), así que la referencia sobra.
3. **Constantes del módulo Identity dentro de Shared**: `PermissionCodes` vive en `Shared/Constants/Claims/AuthClaimTypes.cs:10-19`. No viola una referencia, pero mezcla un contrato de Identity con código transversal.

### Proyectos heredados

- **No hay** `Identity.Api`, `Identity.Data`, `Identity.DTOs` ni `Identity.UseCases`, ni en la solución ni en disco.
- Quedan restos de nombres heredados: el namespace `Identity.Data.Configurations` en `Identity.Persistence/Configurations/Entities/PasswordTokenConfiguration.cs:5`, y dos namespaces de DTOs que conviven (`Identity.Application.DTOs.*` y `Identity.Application.Dtos.*`) dentro de la misma carpeta `Dtos`.
- Hay carpetas `bin/` y `obj/` en la raíz del repositorio (no versionadas), sin proyecto que las genere.

---

## b) Convención real de acciones

**Interfaces** (`Shared/CQRS`, `Shared/Mediator`):
- `ICommand` (equivale a `ICommand<Unit>`), `ICommand<TResponse>`, `IQuery<TResponse>`; todas derivan de `IRequest<TResponse>`.
- `ICommandHandler<TCommand>` (devuelve `Unit`), `ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery, TResponse>`; todas derivan de `IRequestHandler<TRequest, TResponse>` con `Task<TResponse> Handle(TRequest, CancellationToken)`. Los handlers exigen `TResponse : notnull`.
- `ISender.Send<TResponse>(IRequest<TResponse>, CancellationToken = default)`; `Unit` es un `readonly record struct` con `Unit.Value`.
- `IPipelineBehavior<TRequest, TResponse>` con un único comportamiento, `ValidationBehavior`.

**Descubrimiento** (`MediatorServiceCollectionExtensions.cs:21-48`): por reflexión sobre el ensamblado; registra como `Scoped` toda clase concreta que implemente `IRequestHandler<,>`, **incluidas las `internal`**. Lanza si hay dos handlers para el mismo request. Es "magia" por convención, pero acotada y con error explícito.

**Validación**: en el pipeline del mediator (`ValidationBehavior.cs:13-31`), registrada con `AddMediatorValidation` (`MediatorValidationExtensions.cs:11-23`), que llama a `AddValidatorsFromAssembly` con los valores por defecto. **Ese método ignora los validators `internal`** (parámetro `includeInternalTypes`, "The default is false", según la documentación XML del paquete 12.1.1). Tres validators son `internal` y **nunca se ejecutan**: `ChangePasswordValidation`, `ForgotPasswordValidation` y `ValidatePasswordTokenValidation`.

**Errores**: solo excepciones, no hay `Result<T>` en la solución. Todas derivan de `BaseException` (`Shared/Exceptions/Common/BaseException.cs`) con un `ErrorType`, y `GlobalExceptionHandler` (`PosSystem.Api/ExceptionHandling/GlobalExceptionHandler.cs:76-91`) las traduce a `ProblemDetails`:

| Tipo | HTTP | Título |
|---|---|---|
| `ValidationException` (FluentValidation) | 400 | "One or more validation errors occurred." |
| `BadRequestException` | 400 | "Bad request." |
| `UnauthorizedException` | 401 | "Unauthorized." |
| `InvalidTokenException` | 401 | "Invalid token" |
| `ForbiddenException` | 403 | "Forbidden." |
| `NotFoundException` | 404 | "Resource not found." |
| `ConflictException` | 409 | "Conflict." |
| `BusinessException` | 422 | "Business rule violation." |

Todas son `sealed`. El `Detail` de la respuesta es `exception.Message`, y en los casos 4xx el mensaje también se escribe en el log (`GlobalExceptionHandler.cs:45-47`).

**Forma típica de un handler existente**: clase (a veces `internal`, a veces `public`) con dependencias por constructor (contratos de `Contracts.Persistence`, `Contracts.Security`, `Contracts.Services`), consulta con `IUserRepository` (con tracking), excepción de `Shared.Exceptions` ante cada fallo y `IUnitOfWork.SaveChangesAsync` al final. Los commands son `record`. La respuesta vive en `Dtos/Responses`, no en la carpeta de la acción. Los requests HTTP viven en `Identity.Application/Dtos/Request`, no en Presentation.

Desviaciones frecuentes de la convención documentada:
- `CancellationToken` sin propagar: `SaveChangesAsync()` sin token en `CreateUserCommandHandler.cs:53`, `ResendInviteCommandHandler.cs:56`, `ForgotPasswordCommandHandler.cs:52`, `SetUserPasswordCommandHandler.cs:52`, `AuthSessionService.cs:101` y `:133`; consultas sin token en `UserRepository.cs:24, 30, 44`, `UserAccessReader.cs:31`, `SessionRepository.cs:66, 70`.
- `DateTime.UtcNow`/`DateTimeOffset.UtcNow` en lugar de `TimeProvider` en `ForgotPasswordCommandHandler.cs:35, 46, 47`, `SetUserPasswordCommandHandler.cs:42, 49`, `ValidatePasswordTokenQueryHandler.cs:32`, `CreateUserCommandHandler.cs:35, 36, 45, 46`, `ResendInviteCommandHandler.cs:40, 50, 51`. Las acciones nuevas sí usan `TimeProvider`.
- Visibilidad mezclada: commands `internal` (`ChangePasswordCommand`, `ForgotPasswordCommand`, `LogoutSessionCommand`, `ValidatePasswordTokenQuery`) que Presentation no podrá construir cuando los exponga.

---

## c) Compilación y pruebas

Entorno: .NET SDK 10.0.403. Docker no está en ejecución (`docker ps` falla al conectar con el daemon), así que **no pude verificar el estado de la base**.

**`dotnet build POS-system.slnx` sobre el repositorio: falla.**

- `Identity.Application/Features/Users/Queries/GetUsers/GetUsersQuery.cs(6,18): error CS0246: El nombre del tipo o del espacio de nombres 'GetUsersResponse' no se encontró`.
- Resultado: 0 advertencias, 1 error. Compilan solo `Identity.Domain` y `Shared`; Infrastructure, Persistence, Presentation y el host **no llegan a compilarse**, así que sus errores quedan ocultos.

**Copia con el `using` faltante añadido (solo en el directorio temporal): compila.**

- 0 errores, 3 advertencias `MSB3277` (66 líneas), todas en `Identity.Presentation.csproj`: conflicto entre `Microsoft.EntityFrameworkCore`, `.Abstractions` y `.Relational` 10.0.4 (que trae Npgsql 10.0.3) y 10.0.12 (que trae `EFCore.Tools` 10.0.12). Las versiones de EF Core no están alineadas.
- No aparecen advertencias de nulabilidad.

**Arranque de la copia compilada (Development, sin base de datos): falla.**

1. `System.AggregateException: Some services are not able to be constructed ... Unable to resolve service for type 'Identity.Infrastructure.Security.Sha256TokenHasher' while attempting to activate 'Identity.Infrastructure.Security.RefreshTokenGenerator'`. Causa: `RefreshTokenGenerator` pide la clase concreta (`RefreshTokenGenerator.cs:14, 17`) y solo se registra la interfaz (`IdentityInfrastructureServicesRegistration.cs:16`).
2. Con ese registro añadido en la copia: `OptionsValidationException: ... 'SigningKey' ... is required; 'AccessTokeLifetimeMinutes' must be between 1 and 60; 'SessionAbsoluteLifetimeDays' must be between 1 and 90`. Causas:
   - `SigningKey` vacío (`appsettings.Development.json:14`) y no hay user-secrets para el `UserSecretsId` del host. Es el comportamiento que pide la especificación (no arrancar sin clave).
   - Nombres desalineados: la propiedad es `AccessTokeLifetimeMinutes` (`JwtOptions.cs:16`, falta una "n") y la configuración dice `AccessTokenLifetimeMinutes`; la configuración dice `SessionAbsuluteLifetimeDays` (`appsettings.Development.json:16`) y la propiedad es `SessionAbsoluteLifetimeDays`. Ambas quedan en 0. **Aunque se configure la clave, la app seguiría sin arrancar.**
   - `appsettings.json` no tiene sección `Jwt`: fuera de Development tampoco hay emisor ni audiencia.

No continué más allá: el siguiente paso requiere una clave de firma y una base de datos.

**Pruebas**: no existe ningún proyecto de pruebas en la solución ni en disco. No ejecuté `dotnet test` porque no hay nada que ejecutar y la solución no compila.

---

## d) Acciones existentes frente a la especificación

### Situación de exposición HTTP

Hoy solo hay cuatro endpoints:

| Ruta real | Acción |
|---|---|
| `POST api/identity/auth/login` (`AuthController.cs:24-35`) | LoginUser |
| `PUT api/identity/auth/password` (`AuthController.cs:37-47`) | SetUserPassword |
| `GET api/identity/users/{id}` (`UsersController.cs:23-31`) | GetUserById |
| `POST api/identity/users` (`UsersController.cs:33-42`) | CreateUser |

El resto de handlers existe pero no tiene endpoint. La ruta base real es `api/identity/...`; la especificación usa `/auth/...` y `/users/...` (ver decisión D3). Ningún controlador tiene `[Authorize]` ni `[AllowAnonymous]`, y como no hay autenticación configurada, todo es anónimo.

### 6.1 Autenticación y contraseña

**LoginUser — no cumple.**
- Usa el servicio de sesiones para emitir tokens (`LoginUserCommandHandler.cs:49`): cumple.
- Errores distintos por causa: email inexistente da 404 con el email en el mensaje (`:34-35`), `hash_password` NULL da 400 (`:37-38`), contraseña incorrecta da 401 (`:41-42`). La especificación exige un 401 genérico e idéntico. Permite enumerar usuarios, y el email queda en el log (`GlobalExceptionHandler.cs:45-47`).
- **No comprueba `IsActive`**: un usuario desactivado puede iniciar sesión.
- No hay verificación contra un hash ficticio cuando el email no existe (sin igualación de tiempos).
- Contexto del cliente fijo en `"not delivered"` (`:48`); el command no lo recibe.
- Hay un límite de sesiones que no está en la especificación (`:44-46`): con más de 3 sesiones "activas" responde 403. `CountAllActiveByUserAsync` cuenta las no revocadas **aunque estén vencidas** (`SessionRepository.cs:64-66`) y no hay purga, así que un usuario que inicie sesión 4 veces sin hacer logout **queda bloqueado para siempre**. Ver decisión D5.
- Sin `Cache-Control: no-store` en la respuesta (`AuthController.cs:29-35`). Sin rate limiting.
- Validación de contraseña mínima de 4 caracteres (`LoginUserValidator.cs:16`); en login no debería imponerse la regla de contraseña nueva, pero el máximo de 128 sí sirve contra entradas enormes.

**LogoutUser (en el repositorio, `LogoutSession`) — no cumple.**
- Exige el id de usuario y responde 404 si no existe (`LogoutSessionCommandHandler.cs:22-25`); la especificación pide anónimo, identificado solo por el refresh token, y 204 siempre.
- La revocación en sí es idempotente (`AuthSessionService.cs:125-134`), aunque la condición `Now() >= session.RevokedAt` es poco legible y el `SaveChangesAsync` no recibe token.
- No tiene endpoint. El command es `internal`.

**ForgotPassword — no cumple.**
- Email inexistente lanza 401 y existente devuelve `true` (`ForgotPasswordCommandHandler.cs:32-33, 54`): respuesta distinta, enumeración de usuarios.
- No comprueba `IsActive`.
- Invalida **todos** los tokens pendientes del usuario, incluidos los `invite` (`PasswordTokenRepository.cs:38-44`); la especificación habla de invalidar los `reset` anteriores. Ver decisión D9.
- Reset con vigencia de 72 horas (`:46`); la especificación recomienda 15 a 30 minutos, configurable.
- No publica evento ni hay envío de correo. No revoca sesiones: cumple.
- Su validator es `internal` y no se ejecuta. No tiene endpoint.

**ValidatePasswordToken — no cumple.**
- Ya está en `Queries` con el nombre correcto: cumple el renombrado.
- **Lógica de vencimiento invertida**: lanza error si `ExpiresAt > now` (`ValidatePasswordTokenQueryHandler.cs:32-33`); rechaza los tokens vigentes y acepta los vencidos.
- Resultados distintos por causa: 404 si no existe, 409 si está usado o vencido (`:26-33`). La especificación pide 200 con indicador de validez e igual para todas las causas.
- **Registra el token en claro**: el mensaje del 404 incluye `query.Token` (`:27`) y el manejador global lo escribe en el log. Viola "nunca loguear tokens".
- Devuelve el id interno del token (`:35-37`); no es necesario.
- Validator `internal`, no se ejecuta. No tiene endpoint.

**SetPassword (en el repositorio, `SetUserPassword`) — no cumple. Contiene una vulnerabilidad crítica.**
- **No comprueba que el token pertenezca al usuario** del email (`SetUserPasswordCommandHandler.cs:30-50`): busca el usuario por email, busca el token por hash, y nunca compara `passwordToken.UserId` con `user.Id`. Cualquier persona con un token `invite` válido propio puede fijar la contraseña de **cualquier** cuenta, incluida la del administrador. Tampoco exige que el usuario esté sin contraseña.
- Solo acepta tokens `invite` (`:42`): el flujo de reset es imposible.
- El controlador pasa `VerificationPassword` como token (`AuthController.cs:44`); el request no tiene campo de token (`SetUserPasswordRequestDto.cs:3`). El endpoint, tal como está, no puede funcionar.
- Devuelve el token en claro en la respuesta (`:54`, `SetPasswordResponseDto.cs:4`).
- Consumo no atómico (lee y luego escribe, `:37-52`): dos peticiones simultáneas pueden usar el mismo token.
- No invalida los demás tokens pendientes, no revoca sesiones, no aplica la regla de contraseña (mínimo 4 en `SetUserPasswordValidation.cs:16`, sin máximo) y no publica evento.
- La especificación lo define sin email (el token identifica al usuario); el command actual lo exige.
- Respuesta 200 con datos; la especificación pide 204.

### 6.2 Administración de usuarios

**CreateUser — cumple parcialmente.**
- Sin contraseña, `hash_password` NULL, crea token `invite` hasheado (`CreateUserCommandHandler.cs:28-49`): cumple.
- **Bug**: `PhoneNumber = command.FullName` (`:42`). La columna `phone_number` tiene un máximo de 20 (`UserConfiguration.cs:24-26`) y el nombre admite 100, así que cualquier nombre de más de 20 caracteres provoca un 500 al guardar, y el teléfono nunca se guarda.
- Email duplicado: sin comprobación previa; el índice único (`UserConfiguration.cs:53-55`) produce un `DbUpdateException` y un **500**, no un error de negocio.
- Email sensible a mayúsculas (sin normalizar): `Ana@x.com` y `ana@x.com` son usuarios distintos, y el login falla si cambia la capitalización (`UserRepository.cs:38`).
- `RegisterUserRequestDTO` tiene `JobTitle` y `Position` (`RegisterUserRequestDTO.cs:3-8`); el controlador usa `Position` e ignora `JobTitle`.
- No publica evento; el token en claro se pierde (no hay correo), así que hoy ningún usuario creado puede fijar su contraseña.
- Sin permiso `users.create`: endpoint anónimo.
- Los DTOs conservan el nombre `RegisterUser*`.

**ResendInvite — no cumple.**
- No comprueba que el usuario no tenga contraseña (`ResendInviteCommandHandler.cs:35-54`); la especificación pide 422 en ese caso.
- Invalida todos los tokens pendientes, no solo los invites (mismo método que ForgotPassword).
- 404 si no existe: cumple. Sin endpoint ni permiso. Sin validator.

**GetUserById — cumple parcialmente.**
- 404 si no existe, la respuesta no incluye el hash (`GetUserByIdQueryHandler.cs:19-26`): cumple.
- Lee con tracking a través del repositorio de escritura (`UserRepository.cs:44`, sin token); la convención pide `AsNoTracking` con proyección.
- El namespace sigue siendo `...Users.Querys.GetUserById` (`GetUserByIdQuery.cs:4`, `GetUserByIdQueryHandler.cs:6`, `UsersController.cs:5`). Sin validator. Sin permiso `users.read`.

### 6.3 Acciones nuevas ya presentes en el repositorio (integración parcial de la referencia)

| Acción | Estado | Evidencia |
|---|---|---|
| RefreshToken | Parcial | El servicio implementa rotación, vencimiento absoluto y detección de reuso (`AuthSessionService.cs:60-116`). La carrera perdida lanza `new Exception()` y da **500** en lugar de 401 (`:108`). El validator valida `RefreshTokenResponse` en lugar del command (`RefreshTokenCommandValidator.cs:6`), así que no valida nada. Sin endpoint. |
| LogoutAllSessions | Cumple en el handler | `LogoutAllSessionsCommandHandler.cs:16-20`. Sin validator ni endpoint. |
| ChangePassword | No cumple | Contraseña actual incorrecta da **401** (`ChangePasswordCommandHandler.cs:42-43`); la especificación exige 422 porque un 401 hace que el cliente cierre la sesión. La regla de contraseña es "máximo 25", sin mínimo (`ChangePasswordValidation.cs:15, 19`), y ese validator es `internal`, así que no se ejecuta. **`RevokeAllActiveByUserExceptAsync` ignora la sesión excluida** (`SessionRepository.cs:50-52`): revoca también la actual. |
| RevokeMySession | Cumple en el handler | Filtro por `UserId` en la consulta (`SessionRepository.cs:41-48`), mismo 404 para todos los casos. Sin endpoint. |
| GetCurrentUser | No cumple | No devuelve roles ni permisos (`GetCurrentUserQueryHandler.cs:23-26`, `GetCurrentUserResponse.cs:3-8`). |
| GetMySessions | Cumple en el handler | Marca la actual y no expone el hash (`GetMySessionsQueryHandler.cs:23-35`), pero lee del repositorio de escritura en lugar de `ISessionReadRepository`. |
| GetUsers | No compila | Query sin `using` y handler vacío (`GetUsersQueryHandler.cs:3-5`). Falta el repositorio de lectura. |
| DeactivateUser | Parcial | Autodesactivación da **401** en lugar de 422 (`DeactivateUserCommandHandler.cs:30-31`). Lo demás cumple (idempotente, revoca siempre). |
| ReactivateUser | Cumple en el handler | Nombre con errata `ReactivateUserCommnadHandler` y declarado como `record class` (`ReactivateUserCommnadHandler.cs:8`); el archivo del command se llama `ReactiveUserCommand.cs`. |
| RevokeUserSessions | Cumple en el handler | `RevokeUserSessionsCommandHandler.cs:20-27`. |
| GetUserSessions | Falta | No existe en el repositorio. |

Ninguna tiene endpoint. Falta `PasswordRules` (la regla única de contraseña de la sección 7 de la especificación).

---

## e) Hallazgos

### Bloqueantes

| # | Hallazgo | Evidencia | Recomendación |
|---|---|---|---|
| B1 | La solución no compila | `GetUsersQuery.cs(6,18) CS0246`; handler vacío `GetUsersQueryHandler.cs:3-5` | Fase 1: completar GetUsers con la referencia. |
| B2 | El host no arranca: dependencia concreta no registrada | `RefreshTokenGenerator.cs:14, 17`; `IdentityInfrastructureServicesRegistration.cs:16`; salida de arranque en la sección c | Hacer que `RefreshTokenGenerator` dependa de `ITokenHasher` (corrección de DI, autónoma). |
| B3 | El host no arranca: nombres de opciones JWT desalineados | `JwtOptions.cs:16`; `appsettings.Development.json:15-16` | Corregir `AccessTokeLifetimeMinutes` a `AccessTokenLifetimeMinutes` y `SessionAbsuluteLifetimeDays` a `SessionAbsoluteLifetimeDays`. Añadir la sección `Jwt` (sin clave) a `appsettings.json`. Documentar el `dotnet user-secrets set` de la clave. |
| B4 | Schema `"identity  "` (dos espacios) en la migración aplicada; el código usa `"identity"` desde el commit `18b0e` | `20261003150934_Initial.cs:14-15` y todas las tablas; `IdentityDbContextModelSnapshot.cs:20`; `Schemas.cs:5`; historial de git de `Schemas.cs` (`"Identity"`, luego `"identity  "`, luego `"identity"`) | Con la base actual, EF busca tablas e historial de migraciones en un schema inexistente; un `database update` intentaría reaplicar `Initial` y fallaría. No pude verificar la base (Docker apagado). Ver decisión D1. |
| B5 | No hay autenticación ni autorización | `Program.cs:5-32` sin `AddAuthentication`/`UseAuthentication`/`UseAuthorization`; ningún `[Authorize]`; `PermissionCodes` existe pero ninguna política lo usa; no hay paquete `Microsoft.AspNetCore.Authentication.JwtBearer` en la solución | Los tokens se emiten pero nadie los valida, y `POST /users` es anónimo. Fase 3, con aprobación del paquete (decisión D7). |
| B6 | SetPassword permite tomar cualquier cuenta | `SetUserPasswordCommandHandler.cs:30-50` (no compara `passwordToken.UserId` con `user.Id`) | Reescribir según 6.1: el token identifica al usuario, consumo atómico, sin email en el request. |
| B7 | Un usuario queda bloqueado para siempre tras 4 logins | `LoginUserCommandHandler.cs:44-46`; `SessionRepository.cs:64-66` (cuenta sesiones vencidas) | Ver decisión D5. |
| B8 | El token de password se escribe en claro en el log | `ValidatePasswordTokenQueryHandler.cs:27` combinado con `GlobalExceptionHandler.cs:45-47` | Reescribir ValidatePasswordToken (sin excepciones por causa) y no meter datos sensibles en mensajes de excepción. |

Confirmación del hallazgo conocido 2 del HANDOFF ("ningún token se emite"): **parcialmente desactualizado**. LoginUser sí llama a `CreateSessionAsync` (`LoginUserCommandHandler.cs:49`), pero el resultado práctico es el mismo: el host no arranca (B2, B3), no hay refresh expuesto y nadie valida los tokens (B5).

### Importantes

| # | Hallazgo | Evidencia | Recomendación |
|---|---|---|---|
| I1 | Enumeración de usuarios en login, forgot-password y set-password | `LoginUserCommandHandler.cs:34-42`; `ForgotPasswordCommandHandler.cs:32-33` | Mensaje y código únicos; hash ficticio en login; respuesta constante en forgot. |
| I2 | Usuario inactivo puede iniciar sesión | `LoginUserCommandHandler.cs:30-49` sin comprobar `IsActive` | Incluir en el error genérico. |
| I3 | Validators `internal` que nunca se ejecutan | `ChangePasswordValidation.cs:5`, `ForgotPasswordValidation.cs:5`, `ValidatePasswordTokenValidation.cs:5`; `MediatorValidationExtensions.cs:14` | Hacerlos `public` (mecánico) o pasar `includeInternalTypes: true`. Recomiendo `public`, más explícito. |
| I4 | Validator de RefreshToken aplicado al tipo de respuesta | `RefreshTokenCommandValidator.cs:6` | Cambiar a `RefreshTokenCommand`. |
| I5 | ChangePassword revoca también la sesión actual | `SessionRepository.cs:50-52` | Añadir `s.Id != exceptSessionId` (versión de la referencia). |
| I6 | ChangePassword y DeactivateUser responden 401 en reglas de negocio | `ChangePasswordCommandHandler.cs:42-43`; `DeactivateUserCommandHandler.cs:30-31` | 422 con `BusinessException`. |
| I7 | Carrera perdida en refresh da 500 | `AuthSessionService.cs:108` | 401 genérico. |
| I8 | Dos tipos 401 con títulos distintos | `GlobalExceptionHandler.cs:81, 86` ("Unauthorized." frente a "Invalid token") | La especificación pide respuestas indistinguibles; usar un único tipo y mensaje para fallos de autenticación. |
| I9 | ValidatePasswordToken con vencimiento invertido | `ValidatePasswordTokenQueryHandler.cs:32` | Reescribir según 6.1. |
| I10 | CreateUser guarda el nombre en el teléfono y falla con nombres de más de 20 caracteres | `CreateUserCommandHandler.cs:42`; `UserConfiguration.cs:24-26` | Corrección directa. |
| I11 | Email duplicado da 500; email sensible a mayúsculas | `UserConfiguration.cs:53-55`; `UserRepository.cs:38` | Comprobación previa con error de negocio; normalizar email (minúsculas y sin espacios) en Application. La unicidad insensible a mayúsculas en la base requiere migración (decisión D1). |
| I12 | `device_info` sin truncar frente a una columna de 100 | `Session.cs:8`; `SessionConfiguration.cs:29-31`; `AuthSessionService.cs:44, 94` | Confirma y agrava el hallazgo conocido 10: la entidad **no** trunca. En cuanto llegue un User-Agent real (más de 100 caracteres es habitual), login y refresh darán 500. Truncar con una constante compartida entre entidad y configuración. |
| I13 | Sin primer administrador ni seed de permisos | Sin `HasData` ni seeder en `src` | Confirma el hallazgo conocido 3. Fase 3. |
| I14 | Sin rate limiting | `Program.cs` | Confirma el hallazgo conocido 4. Fase 3 (nativo de ASP.NET Core, sin paquete). |
| I15 | Sin purga de sesiones ni tokens | — | Confirma el hallazgo conocido 5. Agrava B7 mientras exista el límite de sesiones. |
| I16 | `DateTime` frente a `DateTimeOffset` dentro del propio módulo | `PasswordToken.cs:9-11` (`DateTimeOffset`); `Session.cs:10-12` y `User.cs:12-13` (`DateTime`); `IPasswordTokenRepository.cs:10` | Confirma el hallazgo conocido 6, aunque la inconsistencia es interna: no encontré ningún documento de arquitectura en el repositorio que fije `DateTimeOffset`. Ver decisión D6. |
| I17 | Violaciones de dependencia | Sección a | Decisión D2. |
| I18 | Secretos de base de datos versionados | `appsettings.Development.json:3` y `docker-compose.yml` (base, `environment`) contienen la contraseña de Postgres; ambos están en git | Moverlos a user-secrets y al override (el patrón de CLAUDE.md pone secretos y puertos solo en el override, y hoy está al revés: el override solo tiene la red). La contraseña ya está en el historial: rotarla. Riesgo bajo por ser local, pero contradice las reglas. |
| I19 | Versiones de EF Core desalineadas | MSB3277 en la sección c | Alinear Npgsql y Tools en la misma versión de parche, o fijar EF Core de forma explícita. Requiere tocar versiones de NuGet (decisión D7). |
| I20 | Contraseña de reset o invite sin regla única | `SetUserPasswordValidation.cs:14-16`; `ChangePasswordValidation.cs:17-19` | Introducir `PasswordRules` (10 a 128) en las dos acciones. |
| I21 | Requests HTTP en Application | `Identity.Application/Dtos/Request/*` | Moverlos a `Identity.Presentation/Requests` (mecánico, según la convención de CLAUDE.md). |
| I22 | Índice `idx_sessions_user_id` sobre `id` | `20261003150934_Initial.cs:212-216`; snapshot `:211-212`; la configuración ya está corregida en `SessionConfiguration.cs:54-55` | Confirma el hallazgo conocido 1, pero **lo bajo de bloqueante a importante**: la misma migración creó `IX_sessions_user_id` sobre `user_id` por la FK (`Initial.cs:224-228`), así que las búsquedas por usuario ya tienen índice; el índice mal nombrado solo duplica la PK. Hay un cambio de modelo pendiente que irá en la próxima migración junto con D1. |

### Menores

| # | Hallazgo | Evidencia |
|---|---|---|
| M1 | Renombrados pendientes: namespace `Querys` (`GetUserByIdQuery.cs:4`, handler `:6`, `UsersController.cs:5`); tipos `SetUserPassword*` en la carpeta `SetPassword`; DTOs `RegisterUser*`; `ReactivateUserCommnadHandler`; archivo `ReactiveUserCommand.cs`; `GetMySessionQueryValidator`; `SessionResposeDto` (sin usar); `AuthClaimTypes.SessionsId`. Conviven los namespaces `DTOs` y `Dtos`. `Identity.Data.Configurations` en `PasswordTokenConfiguration.cs:5`. Del hallazgo conocido 9: `ResentInvite` a `ResendInvite` y `ValidateToken` a `ValidatePasswordToken` ya están hechos; los demás, a medias. |
| M2 | Hallazgo conocido 7 (`Sessiones`): **descartado**, ya es `Sessions` (`User.cs:16`, `SessionConfiguration.cs:58`). |
| M3 | Hallazgo conocido 8 (proyectos heredados): **descartado**; solo quedan los restos de nombres de M1 y las carpetas `bin/` y `obj/` de la raíz. |
| M4 | Índices con nombre por defecto de EF (`IX_sessions_replaced_by_id`, `IX_roles_permissions_permission_id`, `IX_users_roles_role_id`), contra la convención `idx_` (`Initial.cs:206-250`). |
| M5 | CLAUDE.md dice que se usa `EFCore.NamingConventions` y que el schema es `Identity`; en realidad los nombres son `HasColumnName` explícitos y el paquete no está. Actualizar el documento. |
| M6 | Comentarios XML intercambiados en `ISessionRepository.cs:8-11`; métodos sin uso (`CountAllActiveByUserAsync` si se elimina el límite, `GetAllByUserAsync`, `UserRepository.DisableAsync`/`EnableAsync` fuera de la interfaz, `GetWithPasswordTokensAsync`). |
| M7 | `Argon2PasswordHasher.Verify` lanza ante un hash mal formado (`Argon2PasswordHasher.cs:26-29`): 500 en lugar de fallo de autenticación. |
| M8 | La clave de errores de validación es `"Errors"` en mayúscula (`GlobalExceptionHandler.cs:72`); el estándar de `ValidationProblemDetails` es `errors`. |
| M9 | OpenAPI y Scalar expuestos en todos los entornos (`Program.cs:24-28`); sin `UseHttpsRedirection`; sin health checks (la especificación pide `[AllowAnonymous]` en ellos). |
| M10 | `AuthResponseDto.Success` siempre `true` (`LoginUserCommandHandler.cs:58`), redundante con el código HTTP. |
| M11 | `docker-compose.yml` fuerza `password_encryption=md5` en Postgres 16 (el valor por defecto, `scram-sha-256`, es más seguro). |

---

## f) Suposiciones de la sección 5 del HANDOFF

1. **Interfaces de `Shared/CQRS` y `Shared/Mediator`.** Confirmada. Los nombres, los genéricos, `Handle(TRequest, CancellationToken)`, `ISender` y `Unit` coinciden con lo que usa la referencia (detalle en la sección b). Restricción adicional: `TResponse : notnull` en los handlers.
2. **Contratos de Application.** Parcial.
   - `IUserRepository.GetByIdAsync` existe y lee con tracking, pero ignora el token (`UserRepository.cs:44`). El método de existencia se llama `ExistAsync`, no `ExistsAsync`.
   - `IPasswordHasher.Hash(password)` y `Verify(password, storedHash)`: coinciden.
   - `IUnitOfWork.SaveChangesAsync(ct = default)` y además `ExecuteInTransactionAsync`.
   - **Namespaces distintos**: la referencia usa `Identity.Application.Contracts`; el repositorio usa `Contracts.Persistence`, `Contracts.Security`, `Contracts.Services` y `Contracts.Authentication`.
   - **La referencia llama a `_refreshTokenGenerator.Hash(...)`**, que no existe; el repositorio hashea con `ITokenHasher`.
   - **La referencia usa `Session.Create`, `IsRevoked`, `IsExpired` y `Revoke`**; la entidad real es anémica, sin métodos (`Session.cs`).
   - El servicio real se llama `RevokeAllUserSessionAsync` (singular); la referencia, `RevokeAllUserSessionsAsync`.
3. **Excepciones.** Parcial.
   - Existen `NotFoundException` (404, constructores `(message)` y `(entity, key)`) y `BusinessException` (422, `(message)`), pero **son `sealed`**: las cuatro excepciones específicas de la referencia, que heredan de ellas, no compilan.
   - Para el 401 hay `UnauthorizedException` e `InvalidTokenException`; **no existe** `InvalidRefreshTokenException`, que la referencia usa en todo el servicio.
   - El mapeo está en `GlobalExceptionHandler.cs:78-88`.
4. **FluentValidation.** Confirmada, con matiz: pipeline del mediator, pero los validators `internal` se omiten (I3).
5. **Requests HTTP.** No coincide. Viven en `Identity.Application/Dtos/Request` (namespaces `Identity.Application.DTOs.Request` y `Identity.Application.Dtos.Request`); no existe `Identity.Presentation/Requests` ni `Identity.Presentation/Extensions`.
6. **Entidades y `DbSet`.** Confirmada.
   - `Role.RoleName`; el código del permiso es `Permission.Identifier` (también existe `PermissionName`).
   - Navegaciones `User.UserRoles`, `Role.RolePermissions`, `UserRole.Role`, `RolePermission.Permission`.
   - `DbSet`: `Users`, `Sessions`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions` y `PasswordTokens`.
   - `UserAccessReader` ya existe y los usa bien (`UserAccessReader.cs:22-31`). La referencia usa `Set<T>()`, que también funciona.
7. **Controladores.** No coincide. Existen `AuthController` (`api/identity/auth`) y `UsersController` (`api/identity/users`). La referencia trae tres controladores nuevos (`AccountController` y `SessionsController` en `auth`, `UserAdministrationController` en `users`), que crearían rutas en otra base (`/auth` frente a `/api/identity/auth`). Hay que fusionar. La referencia también usa `AuthClaimTypes.SessionId` (en el repositorio, `SessionsId`) y `Shared.Constants.PermissionCodes` (en el repositorio, `Shared.Constants.Claims.PermissionCodes`).

Otra suposición que no se cumple: la sección 2 del HANDOFF dice que "la configuración de JWT Bearer y las políticas por permiso" ya se aplicaron. **No existen** (B5). Solo existen los generadores, `JwtOptions` con validación al arrancar y `PermissionCodes`.

---

## Decisiones que necesito de ti

**D1. Schema `"identity  "` y la migración `Initial`.** No pude ver la base. Opciones:
- (A) Base local desechable: borrar la base local, eliminar y regenerar `Initial` con el schema correcto. Es lo más limpio, pero rompe la regla "no editar `Initial`" y exige tu permiso para borrar datos.
- (B) Conservar `Initial`: una migración nueva que mueve las tablas al schema `identity`. El problema es la tabla de historial: hoy está dentro de `"identity  "` y EF la busca en `identity`, así que antes de aplicar hay que moverla a mano (un script SQL que ejecutarías tú) o desacoplar el schema del historial del schema del modelo. Funciona, pero es frágil.
- (C) Volver a `"identity  "` en `Schemas.cs`. Lo descarto: perpetúa el error.

**Recomiendo A**: el proyecto no está en producción, no hay seed y ningún usuario puede tener contraseña por un flujo válido (SetPassword está roto). Así el índice de sesiones, los nombres `idx_` y la unicidad de email entrarían en una `Initial` limpia. También hay que decidir el nombre: recomiendo `identity` en minúsculas (convención de PostgreSQL) y corregir CLAUDE.md, que dice `Identity`.

**D2. Composición del módulo.**
- (A) El host referencia las cuatro capas y llama a cada `AddIdentity<Capa>Services`, y Presentation queda solo con Application, como dice CLAUDE.md.
- (B) Aceptar que Presentation sea la raíz de composición del módulo y actualizar CLAUDE.md.

**Recomiendo A**: es la regla escrita y hace que el compilador impida que un controlador use un repositorio. Implica cambiar referencias de proyecto. Aprovecharía para quitar `Microsoft.AspNetCore.App` de `Shared`.

**D3. Rutas.**
- (A) Mantener la base real `api/identity/...` y actualizar la especificación.
- (B) Pasar a `/auth` y `/users`.

**Recomiendo A**: el prefijo por módulo evita colisiones cuando lleguen Ventas, Menú y los demás.

**D4. Controladores.**
- (A) Fusionar las acciones de la referencia en `AuthController` y `UsersController`.
- (B) Usar los tres controladores de la referencia sobre la base real.

**Recomiendo A**: dos controladores alineados con las secciones 6.1/6.3 y 6.2, sin rutas repartidas.

**D5. Límite de 3 sesiones en login** (no está en la especificación).
- (A) Eliminarlo.
- (B) Mantenerlo y documentarlo: contar solo sesiones no vencidas, y al superar el límite revocar la más antigua en lugar de responder 403.

**Recomiendo A** por ahora. Si lo quieres para un POS (por ejemplo, limitar dispositivos), B, añadiéndolo a la especificación.

**D6. Tipo de fecha.**
- (A) `DateTime` en UTC en todas las entidades.
- (B) `DateTimeOffset` en todas.

**Recomiendo A**: es lo mayoritario, lo que usan los generadores y `TimeProvider.GetUtcNow().UtcDateTime`, y con Npgsql ambos se guardan como `timestamptz`, así que no hace falta cambiar el esquema. Afecta a `PasswordToken`, `IPasswordTokenRepository` y `GetPasswordTokenResponseDto`.

**D7. Paquetes NuGet.**
- Necesarios: `Microsoft.AspNetCore.Authentication.JwtBearer` (Presentation, para B5) y la alineación de versiones de EF Core (I19).
- Para la Fase 4: un proyecto de pruebas con xUnit, `Microsoft.AspNetCore.Mvc.Testing` y Testcontainers para PostgreSQL (la rotación concurrente necesita una base real).

Pido aprobación explícita para cada uno.

**D8. Excepciones específicas.**
- (A) Usar `NotFoundException`, `BusinessException` y `UnauthorizedException` directamente, con mensajes constantes.
- (B) Quitar `sealed` en Shared para permitir las subclases de la referencia.

**Recomiendo A**: menos tipos, y es el estilo de los handlers actuales. Además unificaría el 401 en un solo tipo (I8).

**D9. Alcance de la invalidación de tokens.** ForgotPassword y ResendInvite invalidan hoy todos los tokens pendientes.
- (A) Invalidar solo los del mismo tipo.
- (B) Invalidar todos.

**Recomiendo B**: un usuario solo debería tener un enlace válido a la vez, y simplifica. Pero difiere de la letra de la especificación ("invalida resets anteriores"), así que hay que confirmarlo y actualizar el documento.

---

## g) Plan propuesto (Fases 1 a 5)

**Fase 1 — Compilar, arrancar e integrar la referencia**
1. Corregir B1 a B3 (GetUsers, DI de `RefreshTokenGenerator`, nombres de `JwtOptions`, sección `Jwt` en `appsettings.json`).
2. Aplicar D2 si lo apruebas (referencias de proyecto).
3. Integrar lo que falta de la referencia, adaptado al repositorio:
   - `PasswordRules`.
   - `IUserReadRepository` e `ISessionReadRepository` con sus implementaciones y registro en DI.
   - GetUsers y GetUserSessions.
   - `PermissionCodes` ampliado (`users.update`, `sessions.read`, `sessions.revoke`).
   - Extensiones de claims y de `ClientContext` en Presentation.
   - Requests en `Identity.Presentation/Requests`.
4. Fusionar los cinco archivos que se solapan:
   - `IAuthSessionService` y `AuthSessionService`: el del repositorio es casi el de la referencia; conservar `ITokenHasher` y la entidad anémica.
   - `ISessionRepository` y `SessionRepository`: tomar la corrección de `RevokeAllActiveByUserExceptAsync`.
   - `PermissionCodes`.
5. Renombrados mecánicos (M1), validators a `public` (I3 e I4), `CancellationToken` y `TimeProvider`.
6. Compilar hasta cero errores y cero advertencias nuevas, y arrancar con una clave de prueba en user-secrets que configuras tú.

**Fase 2 — Corregir las acciones existentes**
- Prioridad de seguridad: SetPassword (B6), ValidatePasswordToken (B8, I9), LoginUser (I1, I2, hash ficticio, `ClientContext`, `no-store`, D5), ForgotPassword (I1, vigencia configurable), LogoutUser (anónimo, 204).
- Después: ChangePassword e I5 a I7, CreateUser (I10, I11), ResendInvite, DeactivateUser.
- Opciones de vigencia de tokens de password (invite y reset) en configuración.
- Truncado de `device_info` (I12).

**Fase 3 — Autenticación, autorización y transversales**
- JWT Bearer según la sección 3 de la especificación: HS256 fijado, 30 segundos de tolerancia, sin mapeo de claims entrantes.
- Una política por código, política de respaldo autenticada y `[AllowAnonymous]` explícito.
- `ProblemDetails` para 401 y 403.
- Orden del pipeline: excepciones, cabeceras reenviadas, HTTPS, autenticación, autorización, controladores.
- Rate limiting nativo; health checks anónimos.
- Seeder del primer administrador (permisos, rol Admin, usuario con invite).
- Migración según D1: generada, **sin aplicar**, con la orden exacta para aplicarla.
- Evento de contraseña cambiada (por ahora solo el contrato y su publicación; el envío de correo no existe).
- Purga en segundo plano.
- Secretos fuera del repositorio (I18).

**Fase 4 — Pruebas**
- Proyecto de pruebas de integración contra PostgreSQL en contenedor (D7), con los casos de la sección 10 de la especificación.
- Requiere Docker en marcha. Informaré de lo que no pueda ejecutarse.

**Fase 5 — Cierre**
- Compilación limpia, pruebas en verde y resumen por fase.
- Actualizar CLAUDE.md (schema, ausencia de `EFCore.NamingConventions`, composición) y la especificación (rutas, D5, D9).
- Lista de pendientes y de lo no verificado.

## No verificado

- El estado real de la base (Docker apagado): qué schema existe, dónde está la tabla de historial y si hay datos.
- El comportamiento en tiempo de ejecución más allá del arranque: no hay clave de firma ni base disponibles.
- Que el código de la referencia compile: lo contrasté por lectura contra las firmas reales (sección f).
