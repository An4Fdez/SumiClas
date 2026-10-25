---
name: backend-domain
description: "Reglas del Domain del backend: errores como excepciones, Code en inglés y Message en español, método Validate, IClock en vez de la hora del sistema y el id que llega de fuera. Úsala al crear o cambiar un agregado, un value object, un factory method o una excepción de dominio."
---

# Dominio del backend: agregados, excepciones, validaciones, reloj e ids

Aplica a `apps/backend/**`. Las reglas obligatorias, la tabla de skills y lo pendiente de decidir están en `apps/backend/AGENTS.md`; las convenciones comunes de idioma, commits y ramas, en el `AGENTS.md` de la raíz.

## Errores, validaciones y reloj en el Domain

**Manejo de errores en Domain**: un fallo esperado del dominio es una excepción que hereda de `DomainException` (`SumiClas.Shared.Domain`, con `Code` y `Message`). Todo factory method o método de comportamiento que pueda fallar **devuelve su valor y lanza** la excepción del módulo (`Announcement.Create` devuelve el `Announcement`; `MarkAsReadBy` es `void`); no hay `Result` ni `IsFailure`. Las excepciones de cada módulo viven juntas en una clase estática `<Módulo>Exceptions` (ver "Excepciones de dominio y estado HTTP" (skill `backend-errors`)); no hace falta acordar cada excepción nueva, pero sí respetar esa forma.

**Idioma de `Code` vs. `Message`** (de `DomainException`): `Code` es un identificador simbólico de código (en inglés, ej. `"SchoolYear.Archived"`) para que el cliente ramifique sin parsear texto y la API lo devuelva en el `ProblemDetails` — no cambia. `Message` es el texto que puede acabar mostrándose al usuario final de la app (directo o como base de un mensaje en la respuesta HTTP), así que va **en español**, igual que el resto de texto de cara al usuario (ver `AGENTS.md` de la raíz). Cada `*Exceptions.cs` nuevo debe seguir este mismo reparto: `Code` en inglés, `Message` en español.

**Validaciones en Domain**: si un factory method tiene más de una validación de parámetros, se extraen a un `private static void Validate(...)` que lanza la primera excepción (patrón ya usado en todos los agregados: `School`, `SchoolYear`, `Class`, `Student`...); el factory solo llama a `Validate` y devuelve el agregado. No se dejan varios `if` de validación inline en el factory.


**Fechas/horas en Domain — `IClock`, nunca `DateTimeOffset.UtcNow` directo**: ningún agregado, value object o evento de dominio llama a `DateTimeOffset.UtcNow` (ni `DateTime.Now`) directamente — eso acopla el dominio a un reloj real y hace imposible testear comportamiento dependiente del tiempo. En su lugar:
- `IClock` (`SumiClas.Shared.Domain`, con `DateTimeOffset UtcNow { get; }`) es la abstracción que se pasa como parámetro a cualquier factory method o método de comportamiento que necesite "ahora" (ej. `Enrollment.Create(studentId, classId, IClock clock)`, `SchoolYear.Archive(IClock clock)`).
- `SystemClock` (`SumiClas.Shared.Infrastructure`, en `shared/infrastructure/`) es la única implementación real (`DateTimeOffset.UtcNow`), y la única clase de todo el dominio donde ese literal puede aparecer. `SystemClock` ya está registrado como singleton de `IClock` en `AddSumiClasInfrastructure()`; en tests se sustituye por un `IClock` de prueba con una fecha fija.
- No añadas una segunda forma de obtener la hora actual (otro helper, otra clase) sin acordarlo antes — `IClock` es la única vía.

## El id se genera fuera del Domain

**El id se genera fuera del Domain** y llega en el `Command` (`Guid Id`). Los factory methods reciben `id` como primer parámetro y lanzan `<Agregado>Exceptions.IdRequired` si es `Guid.Empty`. El command no devuelve nada (`IRequest`), porque el llamador ya conoce el id.

> **Patrón de id aportado desde fuera (completo):** los agregados del modelo de negocio lo siguen (`School.Register`, `SchoolYear.Create`, `Class.Create`, `Student.Register`, `User.Register`, `Enrollment.Create`/`CreateSuggested`, `Subject.Create`, `Announcement.Create`, `CalendarEvent.Create`/`CreateBirthday`, `Assignment.Create`, `ClassExpense.Create` y `LostItem.Create`): `id` como primer parámetro y `<Agregado>.IdRequired`. Ningún agregado genera su id con `Guid.NewGuid()` dentro del Domain (salvo `Session`, ver abajo); en un caso de uso hay dos excepciones, y ninguna en el Domain: `EnrollmentSuggestedCreate`, que lo genera porque crea muchas filas de una vez y no hay un id por fila que aporte el cliente, y `WeeklySummaryGenerate`, que crea el `WeeklySummary` con `Guid.NewGuid()` porque el `POST /weekly-summaries` no lleva id (el resumen es único por clase y semana y lo garantiza el índice único, que responde 409 a la petición que llega segunda). Los casos de uso de **creación** están hechos para esos módulos, junto con la asignación y la renuncia de delegado (`UserClassRepresentativeAssign`, `UserClassRepresentativeResign`) y algunos cambios puntuales (`ClassRename`, `SchoolYearArchive`, `StudentGuardianRemove` y las marcas de leído, hecho, pagado y recuperado); faltan la edición y el borrado del contenido de avisos, eventos, deberes, gastos y objetos perdidos (ver la skill `backend-status-notes`).

> **Excepción deliberada: `Session`.** `Session` es la única raíz que genera su propio id dentro de `Create` (`Session.Create(userId, tokenHash, clock, lifetime)`, sin `IdRequired`). La regla de "id desde fuera" existe para que el id aportado por el cliente en el cuerpo del POST haga que un reintento responda 409; el id de la sesión nunca lo aporta un cliente (se crea en el login), así que esa razón no aplica.
