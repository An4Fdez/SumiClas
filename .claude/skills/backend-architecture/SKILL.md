---
name: backend-architecture
description: "Estructura del backend de SumiClas (api y src/SumiClas), módulos vertical slice, mapa de módulos, reglas de dependencia entre capas y módulos y nomenclatura. Úsala al crear un módulo o agregado, mover código entre capas, poner nombre a una carpeta o clase, o dudar de dónde va algo."
---

# Arquitectura del backend: módulos, capas y nomenclatura

Aplica a `apps/backend/**`. Las reglas obligatorias, la tabla de skills y lo pendiente de decidir están en `apps/backend/AGENTS.md`; las convenciones comunes de idioma, commits y ramas, en el `AGENTS.md` de la raíz.

## `src/SumiClas/` — dominio y aplicación (vertical slice)

Cada módulo es **autocontenido**: trae su propio `Domain/`, `Application/` e `Infrastructure/`.

**Un agregado tiene exactamente una raíz** (`: AggregateRoot`); todo lo demás que forme parte de ese mismo agregado es una **entidad hija** (`: Entity`, nunca `: AggregateRoot`) o un value object, accesible/mutable solo a través de la raíz — nunca con su propio repositorio ni su propia colección de domain events. Ejemplo real: `Announcement` (raíz) contiene una colección privada de `AnnouncementReadReceipt` (entidad hija — quién ha leído el aviso); se crean y protegen sus invariantes (ej. "como mucho un recibo por usuario") desde un método de la propia raíz (`Announcement.MarkAsReadBy(...)`), nunca instanciando la hija por fuera. Si dudas si algo es una raíz nueva o una entidad hija, pregúntate: ¿tiene sentido consultarlo/guardarlo independientemente de su supuesto "padre"? Si no, es hija.

La regla para decidir si dos **agregados** (cada uno con su propia raíz) van en el mismo módulo o en módulos separados es de **acoplamiento/bounded context**, no "un módulo = un agregado" a rajatabla: los del núcleo estructural (`School`, `SchoolYear`, `Class`, `Enrollment`, `Student`, `User`) están débilmente acoplados entre sí → cada uno en su propio módulo. `Subject` (catálogo de asignaturas) también tiene ciclo de vida propio e independiente de `Assignment` → módulo separado, aunque ambos pertenezcan a la feature "Deberes".

**Nombre de la carpeta de módulo = plural de la raíz del agregado** (`: AggregateRoot`) que contiene, siempre — no el nombre de la pantalla/feature del frontend si difiere: `School`→`Schools`, `SchoolYear`→`SchoolYears`, `CalendarEvent`→`CalendarEvents`, `Assignment`→`Assignments`. Cuando un módulo tiene una única raíz (todos los casos hasta ahora), el nombre de la carpeta es literal y sin ambigüedad; no lo sustituyas por un nombre de feature más "bonito" (`Calendar`, `Homework`) aunque coincida con el nombre de la pantalla del frontend.

Estado real hoy (`Domain/` completo en los 12 módulos + mapeos/DbContext de EF Core, el resto pendiente):
```
SumiClas/
  shared/domain/       → Entity, AggregateRoot, DomainEventBase, ValueObject, DomainException, SharedExceptions, AuthenticationExceptions, PaginationExceptions, IClock
  shared/infrastructure/ → SystemClock
  shared/infrastructure/persistence/mappings/ → un IEntityTypeConfiguration<T> por agregado (los 12), ver sección "Base de datos" (skill `backend-persistence`)
  shared/infrastructure/persistence/ → SumiClasDbContext, SumiClasDbContextFactory (solo diseño/dotnet ef)
  shared/infrastructure/persistence/migrations/ → migración inicial `InitialCreate` ya generada y aplicada (ver sección "Base de datos" (skill `backend-persistence`))
  Sessions/ → sesión: agregado `Session` con sus refresh tokens rotatorios (entidad hija) y access token JWT (Domain + Application + Infrastructure; ver "Sesión con JWT y refresh token (módulo `Sessions`)" (skill `backend-auth`))
  Provinces/, Localities/, PostalCodes/ → catálogo territorial (Domain + Infrastructure, solo lectura; ver "Catálogo territorial" (skill `backend-territorial-catalog`))
  Schools/Domain/
  SchoolYears/Domain/
  Classes/Domain/
  Enrollments/Domain/
  Students/Domain/
  Users/Domain/ → `User` lleva también el hash de la contraseña y `IPasswordHasher`/`Password` (ver "Autenticación con email y contraseña" (skill `backend-auth`)); no hay módulo `Authentication`
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

## Reglas de dependencia entre capas y módulos

- **Dentro de un módulo**: `Domain` no depende de nada externo (ni de `Application`, ni de `Infrastructure`, ni de paquetes de infraestructura como EF Core); `Application` depende solo de su propio `Domain`; `Infrastructure` depende solo del `Domain` de su módulo (y de `shared/domain`) e implementa los contratos (puertos) que viven en ese `Domain`, nunca en `Application`. Los tipos de mensaje de otros módulos (p. ej. `UserResponse`, `UserFinderById.Query`) siguen pudiendo usarse en `Application`.
- **Un módulo = una raíz de agregado**, y un caso de uso solo usa el repositorio de su propio módulo. Lo que pertenece a otro módulo se alcanza con `IQueryBus` (solo mensajes) o con eventos de dominio; nunca inyectando su repositorio ni importando su `Domain`/`Infrastructure`.
- **Entre módulos distintos**: nunca acceso directo al `Domain` o `Infrastructure` de otro módulo. La comunicación entre las capas `Application` de módulos distintos se hace enviando una `Query`/`Command` a través de un **QueryBus implementado con MediatR** (`IMediator`/`ISender.Send(...)`): el `Handler` correspondiente, registrado en la `Application` del módulo dueño, la resuelve. Los módulos no se acoplan por referencia directa a la implementación del otro (solo al tipo del mensaje Query/Command) — nunca se inyecta directamente una interfaz/servicio del módulo dueño en la `Application` del módulo consumidor, y nunca se importan sus clases internas directamente.
- La capa `api` puede depender de `Application` de cualquier módulo (para invocar casos de uso vía MediatR) pero nunca debe contener lógica de negocio ni acceder directamente a `Domain`/`Infrastructure`.

## Nomenclatura

- **Idioma**: a partir de ahora, todo el código en inglés — nombres de clases de dominio, value objects, Commands/Queries, carpetas de módulo (`Announcements`, `CalendarEvents`, `Subjects`, `Assignments`, `ClassExpenses`, `LostItems`...). Esto sustituye la convención anterior de nombrar en español; ver el `AGENTS.md` de la raíz para el acuerdo completo (el código va en inglés, pero commits, `README`, y los propios `AGENTS.md` siguen en español).
- Glosario de dominio: usa `docs/glosario-dominio.md` para traducir términos de negocio de forma consistente (y coherente con el frontend, que consume el mismo glosario). Si aparece un término nuevo, tradúcelo con el mismo criterio y añádelo allí, no lo dupliques aquí.
- Sigue el modelo de dominio ya documentado en `docs/arquitectura-app-coordinacion-escolar.md` (Colegio → CursoEscolar → Clase → Matrícula/Alumno, alcance `colegio`/`clase`, ahora `School`/`SchoolYear`/`Class`/`Enrollment`/`Student`, `scope` `school`/`class`) para nombrar entidades y relaciones nuevas — traduciendo con `docs/glosario-dominio.md`, no inventes un modelo paralelo sin consultarlo antes.
