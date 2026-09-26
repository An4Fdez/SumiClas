# AGENTS.md — Backend (SumiClas)

Aplica a `apps/backend/**`. Ver también el `AGENTS.md` de la raíz (convenciones comunes de idioma, commits, ramas y qué no tocar sin permiso).

## Estado actual (importante)

Este backend es, a día de hoy, **un esqueleto**: `apps/backend/api/Program.cs` solo tiene versionado por header, Swagger/OpenAPI y un health check en `/healthy`. Las carpetas `shared/application`, `shared/domain`, `shared/infrastructure` en `apps/backend/src/SumiClas` están vacías. Todo lo que sigue es la arquitectura **objetivo** acordada con el usuario para cuando se implemente el primer módulo real, no una descripción de código ya existente. Si al leer el código encuentras que algo no coincide con este documento, coméntalo antes de asumir cuál de los dos manda.

## Estructura de proyectos

```
apps/backend/
  api/    → SumiClas.Api (ASP.NET Core Web API, net10.0) — capa HTTP, sin lógica de negocio
  src/
    SumiClas/  → SumiClas (net10.0) — dominio y lógica de aplicación, arquitectura vertical slice
```

### `api/` — capa HTTP (Minimal API)

Organizar en tres carpetas, no todo en `Program.cs`:

- **`Endpoints/`** — un archivo (o subcarpeta) por módulo con los endpoints de ese módulo mapeados como método de extensión, invocado desde `Program.cs`. Aquí también viven los DTOs de request/response de esa capa HTTP (no reutilices las entidades de dominio como DTOs).
  ```csharp
  // Endpoints/AnnouncementsEndpoints.cs
  public static class AnnouncementsEndpoints
  {
      public static void MapAnnouncementsEndpoints(this WebApplication app)
      {
          app.MapGet("/announcements", ...);
          app.MapPost("/announcements", ...);
      }
  }
  ```
- **`Middleware/`** — middleware transversal (manejo de errores, logging de requests, etc.).
- **`Bootstrap/`** — inyección de dependencias y arranque: métodos de extensión que registran servicios de cada módulo (`AddSumiClasApplication()`, `AddSumiClasInfrastructure()`, etc.), invocados desde `Program.cs` para que este no acumule configuración de DI directamente.

`Program.cs` debe quedar como orquestador fino: llama a los métodos de `Bootstrap/` para registrar servicios y a los `Map*Endpoints()` de `Endpoints/` para mapear rutas, sin lógica propia.

### `src/SumiClas/` — dominio y aplicación (vertical slice)

Cada módulo/agregado es **autocontenido**: trae su propio `Domain/`, `Application/` e `Infrastructure/`. Ejemplo (ilustrativo, aún no creado):
```
SumiClas/
  shared/           → código realmente transversal: clases base, value objects genéricos, utilidades comunes, configuración compartida de EF Core
  Announcements/
    Domain/
    Application/
    Infrastructure/
  Calendar/
    Domain/
    Application/
    Infrastructure/
  ...
```
`shared/` es la excepción, no la regla: solo debe contener lo que de verdad usan varios módulos (p. ej. una clase base `Entity`, el `DbContext` compartido). Si algo es específico de un módulo, va dentro de su carpeta, no en `shared/`.

## Reglas de dependencia entre capas y módulos

- **Dentro de un módulo**: `Domain` no depende de nada externo (ni de `Application`, ni de `Infrastructure`, ni de paquetes de infraestructura como EF Core); `Application` depende solo de su propio `Domain`; `Infrastructure` implementa los contratos (interfaces) definidos en `Application`/`Domain` de ese mismo módulo.
- **Entre módulos distintos**: nunca acceso directo al `Domain` o `Infrastructure` de otro módulo. Si un módulo necesita algo de otro, debe ser a través de un contrato expuesto en la capa `Application` del módulo dueño, o mediante eventos — nunca importando sus clases internas directamente.
- La capa `api` puede depender de `Application` de cualquier módulo (para invocar casos de uso vía MediatR) pero nunca debe contener lógica de negocio ni acceder directamente a `Domain`/`Infrastructure`.

## CQRS con MediatR

Los casos de uso de cada módulo se implementan como **Commands/Queries + Handlers de MediatR**, viviendo en la carpeta `Application` de ese módulo (p. ej. `Announcements/Application/CreateAnnouncement/CreateAnnouncementCommand.cs` + `CreateAnnouncementCommandHandler.cs`). Los endpoints en `api/Endpoints/` no contienen lógica: reciben la request HTTP, la traducen a un Command/Query y llaman a `IMediator.Send(...)`.

## Nomenclatura

- **Idioma**: a partir de ahora, todo el código en inglés — nombres de clases de dominio, value objects, Commands/Queries, carpetas de módulo (`Announcements`, `Calendar`, `Homework`, `Expenses`, `LostAndFound`...). Esto sustituye la convención anterior de nombrar en español; ver el `AGENTS.md` de la raíz para el acuerdo completo (el código va en inglés, pero commits, `README`, y los propios `AGENTS.md` siguen en español).
- Glosario de dominio: usa `docs/glosario-dominio.md` para traducir términos de negocio de forma consistente (y coherente con el frontend, que consume el mismo glosario). Si aparece un término nuevo, tradúcelo con el mismo criterio y añádelo allí, no lo dupliques aquí.
- Sigue el modelo de dominio ya documentado en `docs/arquitectura-app-coordinacion-escolar.md` (Colegio → CursoEscolar → Clase → Matrícula/Alumno, alcance `colegio`/`clase`, ahora `School`/`SchoolYear`/`Class`/`Enrollment`/`Student`, `scope` `school`/`class`) para nombrar entidades y relaciones nuevas — traduciendo con `docs/glosario-dominio.md`, no inventes un modelo paralelo sin consultarlo antes.

## Base de datos

- **PostgreSQL** vía **EF Core + Npgsql**.
- **Migraciones de EF Core desde el primer commit** que introduzca una entidad persistente — no acumules cambios de esquema sin migración correspondiente.
- El `DbContext` (u otros elementos de infraestructura realmente compartidos entre módulos, como configuración base de EF Core) vive en `SumiClas/shared/infrastructure/`.

## Manejo de errores

Usar el `ProblemDetails` estándar de ASP.NET Core (`builder.Services.AddProblemDetails()` + el middleware de excepciones correspondiente en `Middleware/`), sin inventar un formato de error propio.

## Versionado, OpenAPI y salud (ya implementado, no lo dupliques)

- Versionado por header `X-Api-Version` (paquete `Asp.Versioning`), versión por defecto `1.0`. Nuevos endpoints deben respetar este esquema de versionado, no introducir uno distinto (versionado por URL, por ejemplo) sin acordarlo antes.
- Swagger/OpenAPI ya configurado con un documento por versión — los nuevos endpoints aparecen automáticamente si siguen las convenciones de `Asp.Versioning`/`AddApiExplorer` ya presentes en `Program.cs`.
- Health check ya expuesto en `/healthy` vía `AddHealthChecks()`/`MapHealthChecks`.

## Estilo y verificación

- No hay `dotnet format` ni analizadores de estilo configurados todavía: sigue el estilo ya presente en el código (`Nullable`/`ImplicitUsings` habilitados).
- Antes de dar por terminada cualquier tarea que toque `apps/backend/**`: `dotnet build` sobre la solución (o al menos sobre el/los proyecto/s tocados) debe compilar sin errores.
- Tests: viven en **`apps/test/backend`** (no co-localizados junto al código en `apps/backend`), vacío todavía. Cuando exista contenido, el framework por defecto asumido es **xUnit** (el más habitual en .NET) — si prefieres otro, dímelo y lo documentamos aquí y en el futuro `apps/test/AGENTS.md`.

## Pendiente de decidir (no asumas nada aquí sin preguntar)

- Autenticación/JWT: el doc de arquitectura indica JWT (email+contraseña o magic link), pero no hay nada implementado todavía.
- Despliegue/hosting (Docker, Render/Fly.io/Railway según el doc) — no es responsabilidad de este AGENTS.md de código, pero no asumas configuración de infraestructura sin confirmarla.
