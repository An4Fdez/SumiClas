# Excepciones de dominio en lugar del patrón Result

## Objetivo
Eliminar por completo el patrón `Result`/`Error` del backend y gestionar los fallos esperados con excepciones de dominio. Después, ajustar el frontend para retirar la compatibilidad temporal.

## Decisiones (usuario)
- Se elimina `Result`, `Result<T>`, `Error` y `ResultExtensions` del backend. Los casos de uso devuelven el valor o lanzan.
- `DomainException(code, message)` y nada más: sin estado HTTP, sin enum de categorías y sin categorías compartidas (`NotFoundException`...). Cada excepción hereda directamente de `DomainException`.
- Cada módulo tiene una clase estática `<Modulo>Exceptions` con una clase anidada por error (`ClassExceptions.NotFound`), con constructor vacío que pasa código y mensaje al base. Se lanza con `throw new ClassExceptions.NotFound();` y los tests comprueban el tipo.
- Lo transversal (sesión, paginación, concurrencia y clave duplicada) va en `SharedExceptions`, `AuthenticationExceptions` y `PaginationExceptions`, en `shared/domain`.
- **El estado HTTP se declara en el endpoint**: `Maps<TException>(estado)` guarda un metadato y documenta la respuesta en OpenAPI, y hay perfiles reutilizables (`RequiresSession`, `RequiresClassMembership`, `RequiresClassRepresentative`...). El `DomainExceptionHandler` (proyecto `SumiClas.Api`) lee el metadato del endpoint en curso y escribe el `ProblemDetails` con el mismo `code`; si la excepción no está declarada en el endpoint responde 500 y lo registra como error. Riesgo asumido: un perfil desactualizado produce un 500.
- Una pregunta devuelve `bool` o `null`; una exigencia (`...Ensure`) lanza. Afecta a los cinco sitios donde hoy se ramifica por el fallo (`SessionRotate`, `SchoolYearArchive`, `ClassExpenseCreate`, `UserPasswordVerify`).
- `SessionRotate` ("persistir y fallar"): el endpoint captura la reutilización y envía un segundo comando que revoca la familia en su propia transacción. Costes asumidos: la regla vive en el endpoint y detectar y revocar no son atómicos.
- Se mantiene compatible el contrato con el frontend actual (mismo `code`, mismo estado, mismo `ProblemDetails`) hasta la fase final.

## Cifras de partida
95 ficheros de `src`, 3 de `api` y 75 de tests usan `Result`; unas 270 ramas `Result.Failure(...)`, unas 190 comprobaciones `IsFailure`/`IsSuccess` y 371 aserciones `result.Error`; 139 errores en los catálogos `*Errors.cs`.

## Fases
- [x] F1 Infraestructura, con `Result` aún vivo: `DomainException`, `DomainExceptionStatus` y `Maps<T>`, `DomainExceptionHandler` registrado en `Program.cs`, con sus tests.
- [x] F2 Puente temporal: `UnitOfWorkBehavior` acepta resultado fallido y excepción; `OrThrow()` para que lo convertido consuma lo antiguo.
- [ ] F3 Módulo a módulo, de las hojas a la raíz (dominio, casos de uso, endpoints con sus perfiles y tests en cada pieza): territorio y colegios; `SchoolYears` y `Classes`; `Subjects` y `Students`; `Users` y `Sessions`; `Enrollments`, `Announcements` y `CalendarEvents`; `Assignments`, `ClassExpenses` y `LostItems`.
- [ ] F4 Limpieza: borrar `Result`, `Error`, `ResultExtensions` y el puente; actualizar `UseCaseConventionShould` y el `AGENTS.md`.
- [ ] F5 Frontend: retirar la compatibilidad temporal y ajustar el consumo de errores.

## Criterios de aceptación
- En cada pieza: `dotnet build` limpio y `dotnet test` en verde con varias semillas, y el contrato de la API sin cambios hasta F5.
- Cada fase termina con su commit y revisión nativa por pieza.

## Progreso y evidencia
- F1 hecha (sin commitear): `DomainException` en `shared/domain`; en `SumiClas.Api`: `DomainExceptionStatus` (metadato), `Maps<TException>(estado)` (guarda el metadato y documenta la respuesta una sola vez por estado), `DomainExceptionHandler` (lee el endpoint de `IExceptionHandlerFeature`, porque el middleware lo borra de la petición; sin declarar responde 500 sin código ni mensaje y lo registra como error) y `AddSumiClasExceptionHandling` + `app.UseExceptionHandler()` en `Program.cs`. 14 tests nuevos, incluidos 3 sobre el pipeline real (API mínima en un puerto efímero, sin paquetes nuevos) y 11 mutaciones del manejador y de `Maps`: todas fallan. Efecto lateral: `AddProblemDetails` hace que una excepción no controlada responda un 500 con `ProblemDetails`. Sin ningún cambio de contrato: aún no se lanza ninguna excepción de dominio. Una excepción que no es de dominio la registra como Error el propio middleware de ASP.NET (el handler devuelve `false`); un test del pipeline real lo vigila con `CapturingLoggerProvider`. 798 tests en verde.

- F1 commiteada: `7f92534`.
- F2 hecha (sin commitear): `UnitOfWorkBehavior` ya no exige `TResponse : Result`. Un comando que devuelve su valor falla lanzando; clave duplicada y conflicto de concurrencia pasan a `SharedExceptions.AlreadyExists` y `SharedExceptions.ConcurrencyConflict` (mismo código y mensaje que `SharedErrors`) y un manejador de eventos fallido a `ResultFailureException`; con `Result` el comportamiento no cambia. `OrThrow()` (para `Result` y `Result<T>`) lanza `ResultFailureException`, y `DomainExceptionHandler` la responde con el estado que daba `ResultExtensions` sin necesidad de `Maps`, de modo que el contrato no cambia. Todo esto (`ResultFailureException`, `OrThrow`, la rama del manejador y la bifurcación del comportamiento) se borra en F4. 14 tests nuevos y 12 mutaciones, todas detectadas. 812 tests en verde (semilla 21 y aleatoria), API compila.
- Regla para F3: al convertir un módulo, la misma pieza adapta a quienes lo llaman desde módulos aún sin convertir (un `Result` que consumía su fallo ahora recibe una excepción, que en un endpoint sin `Maps` sería un 500).

## Siguiente paso
F3: primera pieza, territorio y colegios.
