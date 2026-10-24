# Gestión de errores no controlados en la API

## Objetivo
Que cualquier error que no sea una `DomainException` también responda con un `ProblemDetails` coherente, con un `code` estable que el cliente pueda distinguir y con el `traceId` en la respuesta y en el log, en lugar del 500 genérico de ASP.NET o de una respuesta vacía.

## Decisiones (acordadas con el usuario)
- Un solo manejador: `DomainExceptionHandler` se renombra a `ApiExceptionHandler` y absorbe el resto de casos (opción 1 de la conversación); se vuelve a valorar partirlo si pasa de unas seis o siete ramas.
- Las excepciones de dominio siguen igual (estado declarado por el endpoint o por la convención del sufijo, log a `Information`).
- Los códigos nuevos no son `DomainException`: son de la capa API (`ApiErrorCodes`) para no llevar conceptos de infraestructura al dominio ni tocar el catálogo de la convención.
- Ramas nuevas: `BadHttpRequestException` (su propio estado, `Shared.BadRequest`), base de datos no disponible (503 con `Retry-After`, `Shared.ServiceUnavailable`) y el resto (500, `Shared.UnexpectedError`, sin detalles internos, log a `Error` con el `traceId`).
- Las respuestas 4xx/5xx sin cuerpo (404 de una ruta inexistente, 405...) pasan a ser `ProblemDetails` con `UseStatusCodePages()`.
- Fuera de esta ficha: el frontend (`ErrorBoundary`, `traceId` visible, tiempo de espera), enviar errores del navegador al servidor o a Sentry y que `/healthy` compruebe la base de datos.

## Tareas
- [x] E1 `ApiExceptionHandler` (renombrado) con las ramas nuevas, `ApiErrorCodes` y `ApiProblem`, con sus tests.
- [ ] E2 `UseStatusCodePages()` en `Program.cs` para las respuestas vacías, con su test.
- [ ] E3 Documentación en `apps/backend/AGENTS.md` y cierre.

## Criterios de aceptación
- Una excepción inesperada responde 500 con `code` `Shared.UnexpectedError`, `traceId` y sin mensaje ni pila internos; el log lleva el `traceId` y la excepción.
- Un fallo transitorio de la base de datos (una `NpgsqlException` transitoria: conexión caída o rechazada, timeout de comando o de conexión, también envuelta en otra excepción) responde 503 con `Retry-After`.
- Una petición abandonada por el cliente no se registra como error: lo resuelve el middleware del framework antes de llamar a los manejadores (comprobado con una sonda sobre el pipeline real; una rama propia para el 499 resultó código muerto y se quitó).
- Un JSON mal formado o un tipo de contenido no admitido responde con su 4xx y `ProblemDetails`, también en Development.
- Una ruta inexistente responde 404 con `ProblemDetails`.
- Las excepciones de dominio responden exactamente igual que antes.
- Backend: build limpio, todos los tests en verde con las semillas por defecto, 21 y 808; ningún `var`; ninguna línea de test de más de 120 columnas; mutaciones de producción detectadas.
- Revisión nativa por pieza y un commit por pieza, directo a `main`, sin atribución de IA.

## Progreso y evidencia
- Exploración hecha: `DomainExceptionHandler`, `DomainProblem`, `DomainExceptionConvention` y su catálogo de tests, `DomainExceptionPipelineShould` (ya prueba el 500 y el log de error de una excepción no de dominio), `ExceptionHandlingRegistrationShould` y `DomainExceptionHandlerShould`.
- E1 (sin commitear): `DomainExceptionHandler` pasa a `ApiExceptionHandler` (git mv) con `ApiErrorCodes` y `ApiProblem` (`DomainProblem` delega en él). Ramas: dominio (igual que antes), `BadHttpRequestException` (su 4xx, `Shared.BadRequest`, sin el mensaje interno), fallo transitorio de Npgsql en cualquier punto de la cadena de excepciones internas (503 con `Retry-After` de 10 s, `Shared.ServiceUnavailable`) y el resto (500, `Shared.UnexpectedError`, mensaje genérico, log de error con el `traceId` de la respuesta). Tests: `ApiExceptionHandlerShould` (+7 casos), `ExceptionPipelineShould` (+2, con el middleware real; renombrado de `DomainExceptionPipelineShould`), `InfrastructureExceptionMother` y `CapturingLoggerProvider` guarda ahora el mensaje. 8 mutaciones del manejador detectadas (transitorio ignorado, sin recorrer las excepciones internas, sin `Retry-After`, estado de la petición mala, log sin excepción, log sin `traceId`, mensaje interno filtrado y código erróneo). 952 tests con las semillas por defecto, 21 y 808.

## Siguiente paso
E2.
