# MiPOS — contexto permanente del proyecto

Este archivo es la fuente de verdad sobre cómo está construido el proyecto y cómo se trabaja en él. Léelo completo antes de tocar código. Donde diga **verificar**, no asumas: lee el código real y adáptate a él.

## Qué es el proyecto

- Sistema POS (punto de venta) para restaurantes, construido como **monolito modular** en .NET (`net10.0`).
- Proyecto universitario y, a la vez, pieza de portafolio orientada a roles de arquitectura y sistemas distribuidos. Se prioriza código mantenible y decisiones defendibles sobre velocidad.
- Módulos de negocio previstos: Identity (usuarios, autenticación, autorización, roles, permisos), Ventas (mesas, comandas), Menú/Catálogo, Facturación, Inventario, Compras.
- Hoy el módulo activo y "piloto" es **Identity**. Se completa un patrón entero en un módulo antes de replicarlo en otros.

## Cómo trabajar con Sebastián

- Comunícate en **español**. Identificadores de código en **inglés**; términos de dominio en español están permitidos (Ventas, Comandas, Mesas).
- Sé directo y honesto. Si algo está mal diseñado, dilo con argumentos concretos; no valides por cortesía.
- Prefiere código **explícito y trazable** sobre "magia" (reflexión, convenciones implícitas, resolución automática difícil de seguir).
- Prefiere **nombres concretos y específicos**.
- En documentación no escribas ejemplos de código salvo que los pida: él toma las decisiones de implementación. En código sí entrega implementaciones completas.
- Cuando haya una decisión de diseño con alternativas reales, explica las opciones, recomienda una y pregunta; no impongas en silencio.
- Diagnostica con evidencia exacta: cita archivo y línea, pega el error, no hables en abstracto.

## Estructura de la solución

- Solución `pos-system`. El host es **`PosSystem.Api`** (único proyecto de tipo Web; es la raíz de composición, no un gateway).
- Carpeta `Shared` con el proyecto `Shared` (Constants, CQRS, Exceptions, Mediator) y código transversal.
- Cada módulo se compone de proyectos de biblioteca de clases (`Microsoft.NET.Sdk`, nunca Web SDK). Identity tiene:
  - `Identity.Domain` — entidades (`User`, `Session`, `Role`, `Permission`, `UserRole`, `PasswordToken`, ...). Sin dependencias.
  - `Identity.Application` — contratos (`Contracts`), DTOs, casos de uso en `Features`, validaciones, excepciones, servicios de aplicación.
  - `Identity.Infrastructure` — seguridad (hashing de contraseñas, generación de tokens JWT y refresh).
  - `Identity.Persistence` — `IdentityDbContext`, configuraciones EF, repositorios.
  - `Identity.Presentation` — controladores MVC, requests HTTP, configuración de autenticación JWT Bearer y políticas.
  - `Identity.Module` — raíz de composición del módulo. Sin lógica: solo `AddIdentityModule`, que llama a los registros de cada capa.
- Cada capa expone su propio registro de DI con el patrón `AddIdentity<Capa>Services`. El proyecto `<Módulo>.Module` los compone, y `Program.cs` solo llama a un `Add<Módulo>Module` por módulo.
- **Verificar:** pueden quedar proyectos heredados de una estructura anterior (`Identity.Api`, `Identity.Data`, `Identity.DTOs`, `Identity.UseCases`). Si existen, repórtalos; no los borres sin preguntar.

### Reglas de dependencia (se hacen cumplir con referencias de proyecto)

- Domain no referencia a nadie. Application referencia Domain y Shared.
- Infrastructure, Persistence y Presentation referencian Application (y Domain si lo necesitan). **No se referencian entre sí.**
- Solo `<Módulo>.Module` referencia todas las capas de su módulo. El host referencia únicamente los proyectos `.Module`.
- `Shared` no referencia ASP.NET Core, para que Application no vea `HttpContext`. Las capas que necesitan opciones, configuración o HTTP (Infrastructure, Presentation) declaran su propia `FrameworkReference`.
- Application nunca ve `HttpContext`, EF Core ni `IdentityDbContext`: define contratos y Persistence/Infrastructure los implementan.
- Entre módulos solo hay interfaces públicas de contrato o eventos de integración. Sin llaves foráneas entre schemas, sin acceso directo a tablas de otro módulo, sin transacciones entre módulos.

## Stack y convenciones técnicas

- ASP.NET Core con **controladores MVC** en Identity (otros módulos podrán usar otro estilo; la arquitectura varía a propósito según la complejidad del módulo).
- EF Core **Code First** con Fluent API (`IEntityTypeConfiguration<T>`), Npgsql, nombres en snake_case (`EFCore.NamingConventions`), un `DbContext` y un schema por módulo en una única base PostgreSQL.
- `IdentityDbContext` implementa `IUnitOfWork`. Los repositorios de escritura usan tracking; las lecturas usan `AsNoTracking()` con proyección (`Select`) a DTOs.
- Restricciones con nombre explícito: `pk_`, `fk_`, `uq_`, `idx_`, `chk_`. Preferir `VARCHAR + CHECK` sobre enums nativos de Postgres.
- Fechas **siempre en UTC** y con `DateTime` (decisión D6 de la auditoría): todas las entidades usan `DateTime`, obtenido con `TimeProvider.GetUtcNow().UtcDateTime`; las columnas son `timestamptz`. No se usa `DateTimeOffset`.
- El tiempo se obtiene con `TimeProvider` inyectado, nunca con `DateTime.UtcNow` directo en lógica de negocio.
- Todo método asíncrono propaga `CancellationToken`.
- Validación con **FluentValidation**, ejecutada antes del handler (pipeline), de modo que aplique sin importar quién llame.
- Excepciones de dominio/aplicación derivan de `BaseException` y las traduce un `IExceptionHandler` global a `ProblemDetails`. Convención: validación 400, no autenticado 401, sin permiso 403, no encontrado 404, regla de negocio 422, límite de tasa 429. **Verificar** si existe además un tipo `Result<T>` para fallos esperados y cuál de los dos mecanismos usan las acciones ya implementadas; sigue el que ya esté en uso y pregunta ante conflicto.
- Mediator propio en `Shared` (no MediatR, por el cambio de licencia). **Verificar** las interfaces reales de `Shared/CQRS` y `Shared/Mediator` antes de escribir handlers.

## Convención de cada acción (CQRS)

Cada acción vive en su carpeta, `Features/<Área>/<Commands|Queries>/<NombreAcción>/`, con:

- el Command o Query (un `record`),
- su Handler (un handler por acción),
- su Validator de FluentValidation,
- su Response, solo si devuelve datos.

El **Request HTTP** vive en Presentation. El controlador es delgado: arma el command a partir del request, los claims del token y el contexto del cliente, lo envía por el mediator y traduce el resultado a HTTP. Los identificadores del usuario autenticado salen **siempre** de los claims, nunca del body ni de la ruta en acciones sobre "mi cuenta".

Nombres: `Querys` es un error ortográfico heredado; el nombre correcto es `Queries`. Ver los renombrados pendientes en `HANDOFF.md`.

## Seguridad: reglas no negociables

- Contraseñas con **Argon2id**. Tokens de alta entropía (refresh, invite, reset) con **SHA-256** y solo se guarda el hash. Nunca se guarda ni se registra un token o contraseña en claro.
- Nunca loguear contraseñas, hashes, tokens ni la clave de firma.
- Secretos fuera del repositorio: user-secrets en desarrollo, variables de entorno o secretos del orquestador en producción. Si falta la clave de firma JWT, la aplicación no debe arrancar.
- Mensajes de error de autenticación **genéricos e idénticos** para todas las causas (usuario inexistente, contraseña incorrecta, cuenta inactiva, token inválido o expirado).
- Toda acción sobre recursos del usuario filtra por su identidad en la consulta (prevención de IDOR). Un recurso ajeno es indistinguible de uno inexistente.
- Las operaciones de un solo uso (rotar refresh token, consumir token de password) son **atómicas** (actualización condicional), no "leer y luego escribir".
- Endpoints públicos sensibles (login, refresh, forgot-password, set-password, validación de token) requieren rate limiting.
- Las respuestas que contienen tokens llevan `Cache-Control: no-store`.

El diseño completo de autenticación y autorización está en `docs/identity-auth-spec.md`. Ese documento manda sobre cualquier suposición.

## Base de datos y entorno local

- PostgreSQL 16 en Docker Compose: servicio `postgres-posdb`, contenedor `posdb`, puerto del host 5433, base `pos_db`. Patrón `docker-compose.yml` base más `docker-compose.override.yml` con secretos y puertos solo en el override.
- Connection string `PosSystemDb`, definida en `appsettings.Development.json` del host.
- Las migraciones de Identity usan tabla de historial propia en el schema `Identity`. La migración `Initial` ya fue aplicada: **no se edita**; los cambios van en migraciones nuevas.
- Migraciones en producción: previstas como Job de Kubernetes, no al arrancar el pod. No bloquear el pipeline HTTP esperando la base de datos.

## Comandos habituales

- Compilar la solución: `dotnet build`
- Pruebas: `dotnet test`
- Crear migración de Identity: `dotnet ef migrations add <Nombre> --context IdentityDbContext --project Identity.Persistence --startup-project PosSystem.Api --output-dir Migration`
- Aplicar migraciones: `dotnet ef database update ...` con los mismos parámetros. **Pide confirmación antes de ejecutarlo.**

## Qué no hacer sin preguntar

- Aplicar migraciones a la base de datos, borrar datos o recrear contenedores.
- Editar una migración ya aplicada.
- Cambiar referencias entre proyectos, mover proyectos o borrar proyectos.
- Cambiar una decisión arquitectónica registrada en este archivo o en `docs/identity-auth-spec.md`.
- Hacer `git push`, cambiar de rama o reescribir historial.
- Agregar paquetes NuGet que no estén ya en la solución (explica por qué y espera aprobación).

## Forma de entregar cambios

- Cambios pequeños y verificables: compila después de cada bloque coherente.
- Un commit por unidad lógica, con mensaje descriptivo en español.
- Al terminar cada tarea: qué cambiaste, qué verificaste, qué quedó pendiente y por qué.
