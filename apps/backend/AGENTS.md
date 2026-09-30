# AGENTS.md — Backend (SumiClas)

Aplica a `apps/backend/**`. Ver también el `AGENTS.md` de la raíz (convenciones comunes de idioma, commits, ramas y qué no tocar sin permiso).

## Estado actual (importante)

`apps/backend/api/Program.cs` sigue siendo solo versionado por header, Swagger/OpenAPI, health check en `/healthy` y arranque de infraestructura — la capa `api` aún no tiene endpoints reales ni `Application` en ningún módulo. En `apps/backend/src/SumiClas` ya existe el kernel compartido (`shared/domain/`) y el núcleo estructural en capa **Domain** (`Schools`, `SchoolYears`, `Classes`, `Enrollments`, `Students`, `Users`, cada uno con su propio agregado raíz) más los módulos funcionales (`Announcements`, `CalendarEvents`, `Subjects`, `Assignments`, `ClassExpenses`, `LostItems`). Además ya existen los **mapeos de EF Core/PostgreSQL** de los 12 agregados en `shared/infrastructure/persistence/mappings/`, el `SumiClasDbContext`, la `SumiClasDbContextFactory` de diseño, y la **migración inicial ya generada y aplicada** en `shared/infrastructure/persistence/migrations/` (las 17 tablas ya existen en el Postgres de `docker-compose.yml`) — ver sección "Base de datos". `apps/backend/api/Bootstrap/` ya tiene contenido real (`AddSumiClasInfrastructure`, `ApplySumiClasMigrations`, ver esa sección) — es el primer trozo de la carpeta `Bootstrap/` descrita más abajo, el resto (`AddSumiClasApplication()`, etc.) sigue sin existir. `shared/application/` solo contiene `IUnitOfWork`, `ICommand` y `UnitOfWorkBehavior`, y únicamente `Schools` tiene `Application/` (caso de uso `RegisterSchool`, ver "Estructura de un caso de uso") e `Infrastructure/` (`SchoolRepository`); ningún módulo tiene todavía Endpoints propios — eso es la arquitectura **objetivo** que sigue el resto de este documento, no algo ya construido. Si al leer el código encuentras que algo no coincide con este documento, coméntalo antes de asumir cuál de los dos manda.

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

Estado real hoy (`Domain/` completo en los 12 módulos + mapeos/DbContext de EF Core, el resto pendiente):
```
SumiClas/
  shared/domain/       → Entity, AggregateRoot, DomainEventBase, ValueObject, Error, Result/Result<T>, IClock
  shared/infrastructure/ → SystemClock
  shared/infrastructure/persistence/mappings/ → un IEntityTypeConfiguration<T> por agregado (los 12), ver sección "Base de datos"
  shared/infrastructure/persistence/ → SumiClasDbContext, SumiClasDbContextFactory (solo diseño/dotnet ef)
  shared/infrastructure/persistence/migrations/ → migración inicial `InitialCreate` ya generada y aplicada (ver sección "Base de datos")
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

### Estructura de un caso de uso (patrón acordado)

Cada caso de uso vive en `<Módulo>/Application/<NombreCasoUso>.cs` (directamente en `Application/`, sin subcarpeta por caso de uso, para que la estructura no crezca) como **una clase estática** con el nombre del caso de uso, que anida tres tipos (excepción deliberada a "un archivo por clase", decisión explícita del usuario):

- **`Command`** (o `Query`): `record` que implementa `IRequest<Result>` / `IRequest<Result<T>>` de MediatR.
- **`UseCase`**: contiene la lógica (llama al Domain y al repositorio; nunca toca el `IUnitOfWork`). Expone `ExecuteAsync(Command, CancellationToken)`. No depende de MediatR, así que se puede testear sin él.
- **`Handler`**: adaptador fino de MediatR; solo delega en `UseCase.ExecuteAsync`.

El namespace es `SumiClas.<Módulo>.Application` (no hay subcarpeta ni namespace por caso de uso).

Ejemplo real: `Schools/Application/RegisterSchool.cs` (`RegisterSchool.Command`, `RegisterSchool.UseCase`, `RegisterSchool.Handler`).

**Persistencia:** los repositorios heredan de `BaseRepository<TEntity>` (`shared/infrastructure/persistence/`, solo para raíces de agregado; recibe el `SumiClasDbContext`), que ya trae `GetByIdAsync`, `AddAsync` y `Remove` (síncrono, como en EF Core). Un repositorio concreto (`Schools/Infrastructure/SchoolRepository.cs`) solo declara la herencia y añade sus consultas propias; la interfaz (`ISchoolRepository`) sigue en el Domain del módulo y solo expone lo que sus casos de uso necesitan. Los repositorios solo añaden/leen; **el guardado es transparente al caso de uso**: `UnitOfWorkBehavior<TRequest, TResponse>` (`shared/application/`, pipeline behavior de MediatR) gestiona una **transacción de BBDD por comando raíz**: el primer `Command` la abre y hace `Commit` solo si su `Result` es éxito (fallo o excepción → `Rollback`); los `Command` anidados (vía `Send` desde otro handler) se unen a la transacción activa y solo hacen `SaveChanges` (flush) para que las lecturas posteriores de la misma petición vean sus cambios. Solo el `Result` del raíz decide el commit: un handler que ignore el fallo de un comando anidado lo confirma igualmente, así que debe propagarlo. El `DbContext` es scoped (uno por petición) y el endpoint envía un único `Command`, así que comando raíz = petición HTTP; si un endpoint enviara varios, cada uno tendría su propia transacción Solo envuelve requests que implementen el marcador `ICommand` (los `Command` lo declaran junto a `IRequest<Result>`; las `Query` no). `IUnitOfWork` lo implementa `SumiClasDbContext`.

**El id se genera fuera del Domain** y llega en el `Command` (`Guid Id`). Los factory methods reciben `id` como primer parámetro y devuelven `Error` `<Agregado>.IdRequired` si es `Guid.Empty`. El command devuelve `Result` (no `Result<Guid>`), porque el llamador ya conoce el id.

> **Deuda pendiente:** solo `School.Register` sigue este patrón. Los otros 11 agregados aún generan su id con `Guid.NewGuid()` dentro del Domain; hay que migrarlos al mismo esquema (id como parámetro + `IdRequired`) cuando se construya su `Application`.
>
> **Pendiente de cablear:** `RegisterSchool` aún no está registrado en DI (`AddSumiClasApplication()`: MediatR, `UnitOfWorkBehavior` como `IPipelineBehavior<,>` abierto, `UseCase`, `ISchoolRepository`→`SchoolRepository`, `IUnitOfWork`→`SumiClasDbContext`) ni tiene endpoint ni tests. MediatR 12.5.0 (última versión con licencia Apache; las 13+ son comerciales).

## Nomenclatura

- **Idioma**: a partir de ahora, todo el código en inglés — nombres de clases de dominio, value objects, Commands/Queries, carpetas de módulo (`Announcements`, `CalendarEvents`, `Subjects`, `Assignments`, `ClassExpenses`, `LostItems`...). Esto sustituye la convención anterior de nombrar en español; ver el `AGENTS.md` de la raíz para el acuerdo completo (el código va en inglés, pero commits, `README`, y los propios `AGENTS.md` siguen en español).
- Glosario de dominio: usa `docs/glosario-dominio.md` para traducir términos de negocio de forma consistente (y coherente con el frontend, que consume el mismo glosario). Si aparece un término nuevo, tradúcelo con el mismo criterio y añádelo allí, no lo dupliques aquí.
- Sigue el modelo de dominio ya documentado en `docs/arquitectura-app-coordinacion-escolar.md` (Colegio → CursoEscolar → Clase → Matrícula/Alumno, alcance `colegio`/`clase`, ahora `School`/`SchoolYear`/`Class`/`Enrollment`/`Student`, `scope` `school`/`class`) para nombrar entidades y relaciones nuevas — traduciendo con `docs/glosario-dominio.md`, no inventes un modelo paralelo sin consultarlo antes.

## Base de datos

- **PostgreSQL** vía **EF Core + Npgsql** (`Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL`, referenciados en `SumiClas.csproj`).
- **Migraciones de EF Core desde el primer commit** que introduzca una entidad persistente — no acumules cambios de esquema sin migración correspondiente.
- El `DbContext` (`SumiClasDbContext`) vive en `shared/infrastructure/persistence/` — registra los 12 `DbSet<T>` de agregado y llama a `modelBuilder.ApplyConfigurationsFromAssembly(...)` en `OnModelCreating` (así recoge los 12 mapeos `IEntityTypeConfiguration<T>` de `mappings/` automáticamente; los 5 mapeos de tabla hija se aplican transitivamente desde dentro de esos, ver sección de mapeos).
- **`SumiClasDbContextFactory`** (mismo `shared/infrastructure/persistence/`, implementa `IDesignTimeDbContextFactory<SumiClasDbContext>`) existe **solo** para que la herramienta `dotnet ef` pueda construir el `DbContext` al generar/aplicar migraciones — `Program.cs` todavía no registra `SumiClasDbContext` vía DI (eso es Application/Infrastructure bootstrap, pendiente), así que sin esta factory `dotnet ef` no tendría forma de instanciarlo. Apunta a la instancia local de `docker-compose.yml` (credenciales de desarrollo, no reales). Nunca se usa en tiempo de ejecución real, solo en diseño/CLI.
- Migraciones en `shared/infrastructure/persistence/migrations/`, namespace `SumiClas.Shared.Infrastructure.Persistence.Migrations` (hay que pasarlo explícito con `--namespace`, si no `dotnet ef` lo derivaría en minúsculas de la ruta de carpetas). Comandos para generar una migración nueva (desde `apps/backend/src/SumiClas`):
  ```bash
  dotnet ef migrations add <Nombre> --context SumiClasDbContext --output-dir shared/infrastructure/persistence/migrations --namespace SumiClas.Shared.Infrastructure.Persistence.Migrations
  ```
  Ya existe una primera migración (`InitialCreate`, generada y aplicada) con las 17 tablas de los 12 agregados + 5 tablas hijas.
  **Cuidado con `SumiClasDbContextModelSnapshot.cs`**: al generar la *primera* migración, `dotnet ef` puede colocarlo en una ruta espuria anidada (`SumiClas/Shared/Infrastructure/Persistence/Migrations/`, con un `SumiClas/` de más y en PascalCase) en vez de en `shared/infrastructure/persistence/migrations/` junto a los otros dos ficheros — pasó la primera vez. Revisa dónde ha quedado y muévelo si hace falta; el contenido en sí (namespace incluido) sale correcto, solo la ubicación del fichero falla.
- **El agente sí puede ejecutar `dotnet build` y `dotnet ef`**: la máquina tiene instalado el SDK .NET 10 (10.0.401) y `dotnet ef` 10.0.12, así que el agente puede generar/aplicar migraciones directamente. Tras generar una migración, revisa los ficheros resultantes (en especial la ubicación del `ModelSnapshot`, ver aviso de arriba).

### Arranque automático de migraciones (`apps/backend/api/Bootstrap/`)

- `InfrastructureServiceCollectionExtensions.AddSumiClasInfrastructure(configuration)` registra `SumiClasDbContext` vía `AddDbContext` con Npgsql, leyendo la cadena de conexión de `ConnectionStrings:SumiClasDb` (config estándar de ASP.NET Core, no una convención propia).
- `WebApplicationMigrationExtensions.ApplySumiClasMigrations()` llama a `Database.Migrate()` al arrancar **en todos los entornos, sin distinguir Development de producción** — decisión explícita del usuario (revirtió una primera versión que lo limitaba a `IsDevelopment()`). Ambas se invocan desde `Program.cs` (`builder.Services.AddSumiClasInfrastructure(...)` y `app.ApplySumiClasMigrations()`), que se mantiene como orquestador fino, sin la lógica en sí.
- `ConnectionStrings:SumiClasDb` vive en `apps/backend/api/appsettings.Development.json`, con las mismas credenciales de desarrollo de `docker-compose.yml` (`Host=localhost;Port=5432;Database=sumiclas;Username=sumiclas;Password=sumiclas_dev`) — no son secretos reales, mismo razonamiento que en `docker-compose.yml` y en `AGENTS.md` raíz. No hay nada en `appsettings.json` (base/producción): la cadena de conexión de un entorno real todavía no está decidida (ver "Pendiente de decidir") — si se despliega a un entorno real antes de decidir eso, `ApplySumiClasMigrations()` se ejecutará igualmente ahí (ahora ya no distingue por entorno), así que hará falta una `ConnectionStrings:SumiClasDb` real en ese momento o fallará al arrancar.

### Mapeos EF Core (`shared/infrastructure/persistence/mappings/`)

**Excepción deliberada a la regla de `shared/`**: aunque cada mapeo (`IEntityTypeConfiguration<T>`) referencia el `Domain` de un único módulo (p. ej. `SchoolConfiguration` solo conoce `SumiClas.Schools.Domain`), todos los mapeos viven centralizados aquí en vez de en el `Infrastructure/` de cada módulo — decisión explícita del usuario, no un descuido. Esto invierte la dirección de dependencia habitual (`shared` pasa a depender de `Domain` de todos los módulos, en vez de que los módulos dependan de `shared`); si en el futuro esto genera fricción real (por ejemplo, un `DbContext` que ya no quiere conocer todos los módulos a la vez), revisarlo. Los repositorios (`IStudentRepository` → `StudentRepository`, etc.) sí siguen la regla general: van en el `Infrastructure/` de su propio módulo, no aquí — aquí solo va la configuración de mapeo entidad↔tabla.

**Un mapeo por tabla, no por agregado**: un agregado con colecciones de value objects o de entidades hijas en tabla propia (vía `OwnsMany` + `.ToTable(...)`) genera más de una tabla, y cada una tiene su propio archivo de mapeo — no se mete la configuración de la tabla hija como lambda inline dentro del mapeo del agregado.

Las 12 tablas de agregado sí implementan `IEntityTypeConfiguration<T>` (`SchoolConfiguration`, `SchoolYearConfiguration`, `ClassConfiguration`, `EnrollmentConfiguration`, `StudentConfiguration`, `UserConfiguration`, `AnnouncementConfiguration`, `CalendarEventConfiguration`, `SubjectConfiguration`, `AssignmentConfiguration`, `ClassExpenseConfiguration`, `LostItemConfiguration`) — pensadas para registrarse vía `ApplyConfigurationsFromAssembly` cuando exista el `DbContext`.

Las 5 tablas hijas (`GuardianLinkConfiguration` → `StudentGuardians`, `ClassRoleAssignmentConfiguration` → `UserClassRoles`, `AnnouncementReadReceiptConfiguration` → `AnnouncementReadReceipts`, `AssignmentCommentConfiguration` → `AssignmentComments`, `ExpenseParticipantConfiguration` → `ClassExpenseParticipants`) **NO** implementan `IEntityTypeConfiguration<T>` — a propósito. Un value object o entidad hija solo existe dentro de su agregado (`OwnsOne`/`OwnsMany`); si su mapeo implementara `IEntityTypeConfiguration<T>`, `ApplyConfigurationsFromAssembly` lo registraría también como entidad independiente, y EF Core lanzaría una excepción en tiempo de ejecución al construir el modelo ("no puede ser owned y no-owned a la vez"). En su lugar, cada uno expone un método `Configure(OwnedNavigationBuilder<TOwner, TDependent> builder)` (nombre del propietario y del tipo dependiente explícitos, nunca genérico) que incluye **todo** lo de esa tabla, incluida la FK sombra hacia el propietario (`builder.WithOwner().HasForeignKey(...)`) — y solo se invoca a mano desde el `OwnsMany(...)` del agregado propietario, nunca por asembly-scanning. Ejemplo real: `StudentConfiguration` (tabla `Students`) hace `builder.OwnsMany(s => s.Guardians, guardian => new GuardianLinkConfiguration().Configure(guardian));` para la tabla `StudentGuardians`, sin nada más.

17 archivos en total (12 + 5). Los value objects que van en columnas de la misma tabla del agregado en vez de tabla propia (`StudentName` en `Students`, `Email` en `Users`, `EventDate` en `CalendarEvents` — `OwnsOne` sin `.ToTable(...)`) no cuentan aquí: no son una tabla distinta, así que su configuración sí va inline dentro del mapeo del agregado, sin archivo propio.

Convenciones seguidas en los mapeos ya creados, a seguir en los que se añadan después:

- **Enums** → `.HasConversion<string>()` con `.HasMaxLength(30)` (columna legible en la BBDD, no un `int` opaco).
- **`decimal`** (`ClassExpense.TotalAmount`) → `.HasPrecision(10, 2)` explícito, nunca precisión implícita.
- **Value objects singulares en la misma tabla** (`StudentName` en `Student`, `Email` en `User`, `EventDate` en `CalendarEvent`) → `OwnsOne` inline, sin `.ToTable(...)`.
- **Colecciones de value objects o entidades hijas en tabla propia** (`GuardianLink`, `ClassRoleAssignment`, `ExpenseParticipant`, `AnnouncementReadReceipt`, `AssignmentComment`) → `OwnsMany`, mapeo delegado a su propia clase (no `IEntityTypeConfiguration<T>`, ver regla de arriba), que incluye `.ToTable(...)` y la FK sombra. Para value objects (sin `Id` propio) la clave es compuesta y hay que declararla explícitamente (FK sombra del propietario + la propiedad natural del VO, p. ej. `("StudentId", "GuardianId")`); para entidades hijas (`AnnouncementReadReceipt`, `AssignmentComment`, que sí heredan `Id` de `Entity`) es `HasKey(x => x.Id)` normal.
- **Navegación sobre propiedad de solo lectura respaldada por campo privado** (todas las colecciones anteriores se exponen como `IReadOnlyCollection<T>` calculado desde un `List<T>` privado, p. ej. `Guardians => _guardians.AsReadOnly()`): tras el `OwnsMany`, en el mapeo del agregado, `builder.Navigation(x => x.Colección).UsePropertyAccessMode(PropertyAccessMode.Field)` — EF Core no puede usar un setter que no existe, así que accede al campo directamente.
- **`AggregateRoot.DomainEvents`** (y cualquier propiedad calculada como `SchoolYear.IsArchived` o `EventDate.IsRecurringAnnual`) → siempre `builder.Ignore(...)`, en cualquier mapeo nuevo. Si no se ignoran, EF Core intenta mapearlas como si fueran datos reales.
- **Guid, `DateOnly`, `TimeOnly?`, `DateTimeOffset`/`DateTimeOffset?`** → sin configuración especial, Npgsql los mapea de forma nativa a `uuid`/`date`/`time`/`timestamptz`.
- **`string`**: `HasMaxLength(n)` cuando hay un límite natural razonable (nombres, títulos, etiquetas, códigos); sin límite (columna `text`) para contenido libre (`Body`, `Description`, `Text` de comentarios).
- **No se han añadido índices únicos** (p. ej. `User.Email`, `School.OfficialCode`) todavía — la unicidad sigue en la sección "Pendiente de decidir" más abajo, no metas una constraint de BBDD para eso sin acordarlo antes, ya que quien construya la futura `Application` necesita saber si esa unicidad se comprueba antes del `INSERT` o se delega en una excepción de BBDD.

**Constructor adicional solo para EF Core** (`Entity`/`AggregateRoot` en `shared/domain/`, y en `Enrollment`, `Student`, `CalendarEvent`, `User`): EF Core necesita un constructor cuyos parámetros mapeen 1:1 con propiedades persistidas — pero hay dos formas de no cumplir esa condición, no solo una:
1. Un parámetro que no corresponde a ninguna propiedad (`Enrollment` recibe `IClock clock`; `Student` recibe un `GuardianLink firstGuardian` suelto en vez de la colección completa).
2. **Un parámetro que corresponde a una propiedad owned (`OwnsOne`/`OwnsMany`)** — EF Core nunca enlaza un parámetro de constructor a una navegación owned, ni aunque el nombre coincida exactamente con la propiedad (`CalendarEvent` recibe `EventDate eventDate`; `User` recibe `Email email`). Este segundo caso es fácil de pasar por alto porque el constructor "parece" válido a simple vista — solo se descubre al ejecutar `dotnet ef migrations add` y ver el error `No suitable constructor was found` señalando justo el parámetro owned.

La solución en ambos casos es la misma: un constructor privado **sin parámetros**, exclusivamente para que EF Core materialice la entidad por reflexión (nunca se llama desde código de dominio) — por eso `Entity` y `AggregateRoot` también tienen un constructor protegido sin parámetros, además del que recibe `Guid id`. Si añades un agregado nuevo con un constructor real que tenga un parámetro de cualquiera de los dos tipos, sigue el mismo patrón (constructor privado vacío + comentario explicando por qué existe, con `null!` en las propiedades de tipo referencia no anulables) en vez de cambiar la forma del constructor real solo para complacer a EF Core — y, si el agregado tiene alguna propiedad owned, dalo por hecho de entrada en vez de esperar a que `dotnet ef` lo señale.

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
- Estrategia de migraciones de EF Core en producción (auto-aplicar al arrancar, como ya hace `ApplySumiClasMigrations()` en todos los entornos, vs. paso de despliegue separado) — ver sección "Base de datos".
- Cadena de conexión de un entorno real (no local): `ConnectionStrings:SumiClasDb` solo existe hoy en `appsettings.Development.json` con las credenciales de `docker-compose.yml`.
