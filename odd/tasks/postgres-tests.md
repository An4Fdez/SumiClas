# Tests de infraestructura contra PostgreSQL real, con Testcontainers

## Objetivo
Probar contra un PostgreSQL de verdad solo lo delicado de la persistencia, que hoy se prueba únicamente con excepciones simuladas o en memoria: que las migraciones se aplican desde cero, que las claves duplicadas y las carreras de concurrencia se traducen a las excepciones del proyecto, que la paginación por cursor usa el orden real de PostgreSQL para los `uuid`, y que las especificaciones se traducen a SQL como se espera.

## Decisiones (usuario, 2026-10-23)
- Se usa Testcontainers (paquete `Testcontainers.PostgreSql`), con permiso expreso para añadirlo al `.csproj` del proyecto de tests. Cada ejecución arranca su propio contenedor `postgres:17-alpine`, el mismo de `docker-compose.yml`, y lo destruye al terminar. No se toca la base de desarrollo `sumiclas`.
- Solo lo más delicado, no todos los repositorios.

## Diseño
- Un `[SetUpFixture]` arranca un contenedor por ejecución y aplica las migraciones una vez a una base plantilla; cada test crea su base con `CREATE DATABASE ... TEMPLATE ...` (instantáneo) y la borra al terminar, así los tests no se pisan.
- Si Docker no está disponible los tests de esta carpeta se saltan con un aviso claro, de modo que `dotnet test` siga en verde en una máquina sin Docker; en CI se espera Docker.
- Seguir el patrón de los demás tests: `<Algo>Should`, `snake_case`, sin `var`, `this._campo`, máximo 120 columnas, Object Mothers para los datos, mocks y preparación en métodos `Should...` (el `SetUp` solo limpia), y verificación mutando producción.

## Tareas
- [x] T1 Paquete + fixture del contenedor y de las bases por test; migraciones desde cero y sin cambios pendientes del modelo.
- [x] T2 Clave duplicada (`DuplicateKeyException` con el nombre de la restricción) y concurrencia optimista de `Sessions` (`xmin` → `ConcurrencyConflictException`).
- [ ] T3 Orden real de `uuid` en la paginación por cursor y traducción de especificaciones a SQL.
- [ ] T4 Documentación (`AGENTS.md`).

## Criterios de aceptación
- Con Docker arriba: build limpio y todos los tests en verde con las semillas por defecto, 21 y 808; ningún `var`; ninguna línea de test de más de 120 columnas.
- Sin Docker: los tests de PostgreSQL se saltan con un mensaje claro y el resto sigue en verde.
- Barrido de mutaciones sobre producción (quitar el índice único, el token `xmin`, el orden de `uuid`, un filtro de una especificación) hace fallar algún test.
- Revisión nativa por pieza y un commit por pieza.

## Progreso y evidencia
- T1 (commiteada `8111014`): único cambio de `.csproj`, autorizado: `Testcontainers.PostgreSql` 4.16.0 en `SumiClas.Tests`. En `Shared/Infrastructure/Persistence/`: `PostgresServer` (arranca un contenedor `postgres:17-alpine` por ejecución, crea la base plantilla `sumiclas_template` con las 10 migraciones aplicadas y entrega a cada test su propia base, copiada de la plantilla o vacía, con `Pooling=false` para poder usarla como plantilla), `TestDatabase` (contextos de EF sobre esa base; se borra al terminar con `DROP DATABASE ... WITH (FORCE)`), `PostgresServerSetUp` (`[SetUpFixture]` que lo arranca y lo para) y `MigrationsShould` (3 tests: las migraciones se aplican a una base vacía y quedan todas aplicadas; `HasPendingModelChanges` es falso, es decir, el modelo coincide con el snapshot; hay una tabla por cada entidad del modelo). Sin estado en el fixture: cada test pide su base con `await using`. Hallazgo de diseño, corregido a tiempo: la primera versión tragaba cualquier excepción del arranque como "Docker no disponible" y habría saltado los tests ante un cambio de modelo sin migración, ocultando un defecto real. Ahora solo el fallo de arrancar el contenedor salta los tests (con el motivo en el mensaje) y una migración que falla rompe toda la ejecución. Barrido: 2 mutaciones detectadas (ancho de una columna y nombre de una tabla sin su migración). 900 tests en el proyecto. El camino sin Docker se comprobó forzando un fallo al arrancar el contenedor (una imagen que no existe): los 3 tests se saltan con el motivo en el mensaje y no fallan; la variable `DOCKER_HOST` no sirve para simularlo, porque Testcontainers cae a la tubería de Docker Desktop.
- T2 (sin commitear): `UniqueKeysShould` (4 tests) y `OptimisticConcurrencyShould` (3), contra PostgreSQL real y con los datos de los Object Mothers de usuarios y sesiones. Claves: dos usuarios con el mismo correo lanzan `DuplicateKeyException` con la restricción `IX_Users_Email`; el mismo id dos veces, con `PK_Users`; dos usuarios distintos se guardan; y tras un duplicado la base solo conserva el primero. Concurrencia: dos contextos que rotan el mismo refresh token de una sesión, el segundo en guardar lanza `ConcurrencyConflictException`, la base conserva el sucesor del primero y no el del segundo, y dos rotaciones seguidas sí se guardan. Barrido: 6 mutaciones nuevas detectadas (quitar `unique` de la migración del índice, cambiar su nombre, el `SqlState` y el nombre de la restricción en el detector, quitar `LastRotatedAt` de `Session.Rotate` —el fallo histórico por el que la rotación no tocaba la raíz y el token `xmin` no se comprobaba— y quitar el `catch` de la concurrencia en el contexto), 8 en total con las de migraciones. Sin cambios en producción. 907 tests en el proyecto.

## Siguiente paso
T3: orden real de `uuid` en la paginación y traducción de especificaciones a SQL.
