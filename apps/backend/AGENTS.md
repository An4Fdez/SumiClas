# AGENTS.md — Backend (SumiClas)

Aplica a `apps/backend/**`. Ver también el `AGENTS.md` de la raíz (convenciones comunes de idioma, commits, ramas y qué no tocar sin permiso).

## Cómo trabajar en el backend

Backend .NET 10 con PostgreSQL (EF Core), arquitectura vertical slice por módulos y casos de uso con CQRS (MediatR). **El detalle de cada tema vive en skills** (`.claude/skills/<nombre>/SKILL.md`), para no cargar de golpe todo lo que no hace falta: antes de escribir o cambiar código, mira la tabla de abajo y **lee las skills que apliquen a la tarea** (casi siempre son varias: un endpoint de lectura nuevo pide `backend-endpoints`, `backend-read-queries`, `backend-use-cases`, `backend-authorization` y `backend-testing`). Si algo del código no coincide con lo que dice una skill, coméntalo antes de asumir cuál de los dos manda.

## Estructura de proyectos

```
apps/backend/
  api/    → SumiClas.Api (ASP.NET Core Web API, net10.0) — capa HTTP, sin lógica de negocio
  src/
    SumiClas/  → SumiClas (net10.0) — dominio y lógica de aplicación, arquitectura vertical slice
```

## Skills del backend

| Si vas a... | Lee la skill |
| --- | --- |
| crear un módulo o un agregado, mover código entre capas, poner nombre a una carpeta o clase, o dudar de dónde va algo | `backend-architecture` |
| crear o cambiar un agregado, value object, factory method o excepción de dominio | `backend-domain` |
| crear o modificar un caso de uso (Command o Query, Handler, repositorio, transacción) | `backend-use-cases` |
| leer datos o reglas de otro módulo, reaccionar a un evento de dominio o saber quién es el usuario actual | `backend-cross-module` |
| crear o cambiar un endpoint, su DTO de request, los metadatos OpenAPI, `Program.cs` o `Configuration/` | `backend-endpoints` |
| crear una excepción nueva, decidir su estado HTTP, tocar el manejo de errores o los logs | `backend-errors` |
| crear o cambiar un endpoint de lectura (`GET`), un DTO de respuesta o una lista paginada | `backend-read-queries` |
| añadir un campo a una entidad, buscar por criterios o crear o ampliar un repositorio | `backend-specifications` |
| añadir o cambiar una entidad persistente, un mapeo, un índice o una migración | `backend-persistence` |
| tocar el login, el registro, el refresh, el logout, los tokens, la cookie o la seguridad de la API | `backend-auth` |
| comprobar permisos por pertenencia o rol en un caso de uso | `backend-authorization` |
| tocar colegios, provincias, poblaciones, códigos postales o el importador | `backend-territorial-catalog` |
| tocar el resumen semanal con IA, `AiOptions` o la clave de la IA | `backend-weekly-summary` |
| escribir, cambiar o revisar tests, o decidir si un test sobra | `backend-testing` |
| consultar qué hace un caso de uso que ya existe y qué huecos tiene (referencia, no reglas) | `backend-status-notes` |

## Reglas que más importan (el detalle está en las skills)

- Los fallos esperados son **excepciones** que heredan de `DomainException`, no valores de retorno; el estado HTTP sale del sufijo del nombre de la excepción (`backend-errors`).
- Un caso de uso solo usa el repositorio de **su propio módulo**; lo de otro módulo se pide por `IQueryBus` o se reacciona con eventos (`backend-cross-module`).
- La hora sale de `IClock`, nunca de `DateTimeOffset.UtcNow`; el id lo aporta el cliente, con las excepciones que se explican allí (`backend-domain`).
- La autorización va en el caso de uso, no en el endpoint (`backend-authorization`).
- Todo endpoint declara `WithName`, `Produces` y su etiqueta, y tras cambiarlos se regenera el cliente del frontend (`backend-endpoints`).
- Los tests usan Object Mothers, Moq y AwesomeAssertions, y no se añade uno que otro ya cubra (`backend-testing`).

**Tipos explícitos, nunca `var` (regla obligatoria)**: en todo el código C# del backend (`src`, `api` y los tests) los locales, los `foreach`, los `out` y las deconstrucciones llevan su tipo escrito: `Guid current = ...`, `foreach (ClassResponse schoolClass in classes)`, `out Email? email`, `(IReadOnlyList<Announcement> announcements, string? nextCursor) = ...`. No se escribe `var` en código nuevo ni al modificar código que ya existe. Se respeta la nulabilidad real: `?` solo si el valor puede ser nulo (`GetByIdAsync`, `FirstOrDefault`, `TryGetValue`), nunca en la variable de un `foreach` ni en un tipo genérico sin restricción como `TResponse`. Quedan fuera las migraciones de EF generadas (`shared/infrastructure/persistence/migrations/`) y el importador de catálogo de `tools/`, que está pendiente de borrar y no se convierte. Para cumplirla sin escribir los tipos a mano: crear un `.editorconfig` temporal en la raíz con `csharp_style_var_for_built_in_types = false:warning`, `csharp_style_var_when_type_is_apparent = false:warning`, `csharp_style_var_elsewhere = false:warning` y `dotnet_diagnostic.IDE0008.severity = warning`; ejecutar `dotnet format style <proyecto>.csproj --diagnostics IDE0008 --severity info` (excluyendo las migraciones) hasta que una pasada no cambie nada; revisar los `?` de más, sobre todo en los `foreach`; y borrar el `.editorconfig`, que no se commitea. Antes de dar por terminada una tarea, `grep -rE "\bvar\b"` sobre `apps/backend/src`, `apps/backend/api` y `apps/test` (con `--include=*.cs` y descartando `bin/`, `obj/` y las migraciones) tiene que salir vacío.

## Estilo y verificación

- No hay `dotnet format` ni analizadores de estilo configurados todavía: sigue el estilo ya presente en el código (`Nullable`/`ImplicitUsings` habilitados).
- Antes de dar por terminada cualquier tarea que toque `apps/backend/**`: `dotnet build` sobre la solución (o al menos sobre el/los proyecto/s tocados) debe compilar sin errores.
- Tests: `dotnet test apps/test/backend/SumiClas.Tests` (NUnit; detalle y convenciones en `backend-testing`). Los tests que necesiten PostgreSQL arrancan un contenedor con Docker.

## Pendiente de decidir (no asumas nada aquí sin preguntar)

- Autenticación/JWT: **hecha en la API** (ver "Autenticación de la API" (skill `backend-auth`)); el frontend ya la usa (Bearer en memoria + cookie de refresh) y la verificación de extremo a extremo (J4) pasó en una API real y en un Chrome real. La **autorización por rol y pertenencia** está hecha (ver "Autorización" (skill `backend-authorization`)), con huecos concretos anotados allí.
- **Notificaciones push**: fuera del MVP (decisión del usuario, 2026-10-23); para una versión posterior. No hay nada construido, y el mecanismo está por decidir: la Web Push API del documento de arquitectura exige service worker y PWA, descartados en el stack del frontend (ver el `AGENTS.md` de la raíz). Ver la nota de alcance del §4 del documento de arquitectura.
- Despliegue/hosting: **decidido Azure** (decisión del usuario, 2026-10-24; el documento de arquitectura aún cita Render/Fly.io/Railway, que quedan descartados). Todavía no hay nada montado: servicio (App Service o Container App), PostgreSQL gestionado, alojamiento del frontend y migraciones en producción están por diseñar, y no asumas configuración de infraestructura sin confirmarla. Los secretos (`Jwt__SigningKey`, `Ai__ApiKey`, la cadena de conexión) entran como ajustes de la aplicación o como referencias a Key Vault con identidad administrada (permite rotarlos sin tocar la app); el código solo lee `IConfiguration`.
- Estrategia de migraciones de EF Core en producción (auto-aplicar al arrancar, como ya hace `ApplySumiClasMigrations()` en todos los entornos, vs. paso de despliegue separado) — ver sección "Base de datos" (skill `backend-persistence`).
- Cadena de conexión de un entorno real (no local): `ConnectionStrings:SumiClasDb` solo existe hoy en `appsettings.Development.json` con las credenciales de `docker-compose.yml`.
- **CORS**: decidido que **no se configura** (web y API en el mismo origen); ver "Autenticación de la API" (skill `backend-auth`). Si la web se sirviera algún día desde otro origen, habría que añadirlo.
- **Frontend y JWT**: resuelto (J3a/J3b/J4). El frontend envía `Authorization: Bearer` con el access token en memoria y refresca por la cookie `sumiclas_refresh`; no envía `X-User-Id` (ver `apps/frontend/AGENTS.md`). Para arrancar la API en local hace falta la clave de firma en User Secrets (ver "Autenticación de la API" (skill `backend-auth`) y el `AGENTS.md` raíz).
