# AGENTS.md — Backend (SumiClas)

Aplica a `apps/backend/**`. Ver también el `AGENTS.md` de la raíz (convenciones comunes de idioma, commits, ramas y qué no tocar sin permiso).

## Estado actual (importante)

`apps/backend/api/Program.cs` sigue siendo solo versionado por header, Swagger/OpenAPI y un health check en `/healthy` — la capa `api` aún no tiene endpoints reales. En `apps/backend/src/SumiClas` ya existe el kernel compartido (`shared/domain/`) y el núcleo estructural en capa **Domain únicamente** (`Schools`, `SchoolYears`, `Classes`, `Enrollments`, `Students`, `Users`, cada uno con su propio agregado raíz). `shared/application/` y `shared/infrastructure/persistence/` siguen vacías, y ningún módulo tiene todavía `Application/` ni `Infrastructure/` (ni Endpoints propios) — eso es la arquitectura **objetivo** que sigue el resto de este documento, no algo ya construido. Si al leer el código encuentras que algo no coincide con este documento, coméntalo antes de asumir cuál de los dos manda.

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

Cada módulo es **autocontenido**: trae su propio `Domain/`, `Application/` e `Infrastructure/`.

**Un agregado tiene exactamente una raíz** (`: AggregateRoot`); todo lo demás que forme parte de ese mismo agregado es una **entidad hija** (`: Entity`, nunca `: AggregateRoot`) o un value object, accesible/mutable solo a través de la raíz — nunca con su propio repositorio ni su propia colección de domain events. Ejemplo real: `Announcement` (raíz) contiene una colección privada de `AnnouncementReadReceipt` (entidad hija — quién ha leído el aviso); se crean y protegen sus invariantes (ej. "como mucho un recibo por usuario") desde un método de la propia raíz (`Announcement.MarkAsReadBy(...)`), nunca instanciando la hija por fuera. Si dudas si algo es una raíz nueva o una entidad hija, pregúntate: ¿tiene sentido consultarlo/guardarlo independientemente de su supuesto "padre"? Si no, es hija.

La regla para decidir si dos **agregados** (cada uno con su propia raíz) van en el mismo módulo o en módulos separados es de **acoplamiento/bounded context**, no "un módulo = un agregado" a rajatabla: los del núcleo estructural (`School`, `SchoolYear`, `Class`, `Enrollment`, `Student`, `User`) están débilmente acoplados entre sí → cada uno en su propio módulo. `Subject` (catálogo de asignaturas) también tiene ciclo de vida propio e independiente de `Assignment` → módulo separado, aunque ambos pertenezcan a la feature "Deberes".

**Nombre de la carpeta de módulo = plural de la raíz del agregado** (`: AggregateRoot`) que contiene, siempre — no el nombre de la pantalla/feature del frontend si difiere: `School`→`Schools`, `SchoolYear`→`SchoolYears`, `CalendarEvent`→`CalendarEvents`, `Assignment`→`Assignments`. Cuando un módulo tiene una única raíz (todos los casos hasta ahora), el nombre de la carpeta es literal y sin ambigüedad; no lo sustituyas por un nombre de feature más "bonito" (`Calendar`, `Homework`) aunque coincida con el nombre de la pantalla del frontend.

Estado real hoy (solo `Domain/`, el resto pendiente):
```
SumiClas/
  shared/domain/       → Entity, AggregateRoot, DomainEventBase, ValueObject, Error, Result/Result<T>, IClock
  shared/infrastructure/ → SystemClock
  Schools/Domain/
  SchoolYears/Domain/
  Classes/Domain/
  Enrollments/Domain/
  Students/Domain/
  Users/Domain/
  Announcements/Domain/  → Announcement (raíz) + AnnouncementReadReceipt (entidad hija)
  CalendarEvents/Domain/ → CalendarEvent (raíz) + EventDate (value object)
    · Los eventos puntuales (Trip/Meeting/Activity/Other) referencian ClassId (una Class concreta, coherente con que no son permanentes).
    · Birthday referencia StudentId, nunca ClassId: Class se recrea cada SchoolYear, y un cumpleaños se repite para siempre — si se enlazara a una Class fija, dejaría de aparecer al cambiar el alumno de clase el curso siguiente. Qué clase mostrar se resuelve en consulta (Student → Enrollment activo → Class), no se guarda en el evento.
  Subjects/Domain/       → Subject (raíz) — catálogo de asignaturas
    · Scoped por ClassId (por clase, no por colegio): se recrea cada curso junto con su Class, aunque en la práctica coincidan nombres entre clases distintas.
  Assignments/Domain/    → Assignment (raíz) + AssignmentComment (entidad hija)
  ClassExpenses/Domain/  → ClassExpense (raíz) + ExpenseParticipant (value object)
    · ClassId (no StudentId): a diferencia de Birthday, un gasto es puntual, no recurrente — no hay problema de "seguir al alumno" entre cursos.
    · Participantes y su estado de pago van juntos en ExpenseParticipant (GuardianId + HasPaid) en vez de dos listas separadas (participantIds/paidBy del mock), para que no puedan desincronizarse.
  LostItems/Domain/      → LostItem (raíz)
    · ClassId (no StudentId): igual que ClassExpense, no es recurrente — se publica una vez y se resuelve, no hay que "seguir" al alumno entre cursos.
    · PhotoPrivacyConfirmed protege en Domain la regla de privacidad de la sección 7.3 (opt-in por foto): si hay PhotoUrl, tiene que venir confirmado, si no Create falla.
```

Nota: las pantallas del frontend se llaman "Calendario"/`Calendar` y "Deberes"/`Homework`, pero los módulos del backend son `CalendarEvents` y `Assignments` (nombre de su agregado raíz, no de la pantalla) — no son el mismo nombre y es intencional.
`shared/` es la excepción, no la regla: solo debe contener lo que de verdad usan varios módulos (p. ej. la clase base `Entity`, el `DbContext` compartido cuando exista). Si algo es específico de un módulo, va dentro de su carpeta, no en `shared/`.

**Manejo de errores en Domain**: los agregados y value objects no lanzan excepciones de dominio — todo factory method o método de comportamiento que pueda fallar devuelve `Result`/`Result<T>` (`SumiClas.Shared.Domain`), con un `Error` (`Code` + `Message`) describiendo el fallo. Quien llama decide qué hacer con `IsFailure`/`Error`; no introduzcas una excepción de dominio nueva sin acordarlo antes.

**Idioma de `Error.Code` vs. `Error.Message`**: `Code` es un identificador simbólico de código (en inglés, ej. `"SchoolYear.Archived"`) para que la futura Application lo mapee a un `ProblemDetails` sin parsear texto — no cambia. `Message` es el texto que puede acabar mostrándose al usuario final de la app (directo o como base de un mensaje en la respuesta HTTP), así que va **en español**, igual que el resto de texto de cara al usuario (ver `AGENTS.md` de la raíz). Cada `*Errors.cs` nuevo debe seguir este mismo reparto: `Code` en inglés, `Message` en español.

**Fechas/horas en Domain — `IClock`, nunca `DateTimeOffset.UtcNow` directo**: ningún agregado, value object o evento de dominio llama a `DateTimeOffset.UtcNow` (ni `DateTime.Now`) directamente — eso acopla el dominio a un reloj real y hace imposible testear comportamiento dependiente del tiempo. En su lugar:
- `IClock` (`SumiClas.Shared.Domain`, con `DateTimeOffset UtcNow { get; }`) es la abstracción que se pasa como parámetro a cualquier factory method o método de comportamiento que necesite "ahora" (ej. `Enrollment.Create(studentId, classId, IClock clock)`, `SchoolYear.Archive(IClock clock)`).
- `SystemClock` (`SumiClas.Shared.Infrastructure`, en `shared/infrastructure/`) es la única implementación real (`DateTimeOffset.UtcNow`), y la única clase de todo el dominio donde ese literal puede aparecer. Cuando exista Application/Infrastructure, `SystemClock` se registra como singleton en el contenedor de DI; en tests se sustituye por un `IClock` de prueba con una fecha fija.
- No añadas una segunda forma de obtener la hora actual (otro helper, otra clase) sin acordarlo antes — `IClock` es la única vía.

## Reglas de dependencia entre capas y módulos

- **Dentro de un módulo**: `Domain` no depende de nada externo (ni de `Application`, ni de `Infrastructure`, ni de paquetes de infraestructura como EF Core); `Application` depende solo de su propio `Domain`; `Infrastructure` implementa los contratos (interfaces) definidos en `Application`/`Domain` de ese mismo módulo.
- **Entre módulos distintos**: nunca acceso directo al `Domain` o `Infrastructure` de otro módulo. La comunicación entre las capas `Application` de módulos distintos se hace enviando una `Query`/`Command` a través de un **QueryBus implementado con MediatR** (`IMediator`/`ISender.Send(...)`): el `Handler` correspondiente, registrado en la `Application` del módulo dueño, la resuelve. Los módulos no se acoplan por referencia directa a la implementación del otro (solo al tipo del mensaje Query/Command) — nunca se inyecta directamente una interfaz/servicio del módulo dueño en la `Application` del módulo consumidor, y nunca se importan sus clases internas directamente.
- La capa `api` puede depender de `Application` de cualquier módulo (para invocar casos de uso vía MediatR) pero nunca debe contener lógica de negocio ni acceder directamente a `Domain`/`Infrastructure`.

## CQRS con MediatR

Los casos de uso de cada módulo se implementan como **Commands/Queries + Handlers de MediatR**, viviendo en la carpeta `Application` de ese módulo (p. ej. `Announcements/Application/CreateAnnouncement/CreateAnnouncementCommand.cs` + `CreateAnnouncementCommandHandler.cs`). Los endpoints en `api/Endpoints/` no contienen lógica: reciben la request HTTP, la traducen a un Command/Query y llaman a `IMediator.Send(...)`.

## Nomenclatura

- **Idioma**: a partir de ahora, todo el código en inglés — nombres de clases de dominio, value objects, Commands/Queries, carpetas de módulo (`Announcements`, `CalendarEvents`, `Subjects`, `Assignments`, `ClassExpenses`, `LostItems`...). Esto sustituye la convención anterior de nombrar en español; ver el `AGENTS.md` de la raíz para el acuerdo completo (el código va en inglés, pero commits, `README`, y los propios `AGENTS.md` siguen en español).
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
