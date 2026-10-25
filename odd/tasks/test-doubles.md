# Quitar los dobles de test escritos a mano

## Objetivo
`backend-testing` dice que los dobles se hacen con Moq y nunca a mano. La auditoría del 2026-10-25 encontró dos que no lo cumplen: `FakeHttpMessageHandler` y `CapturingLoggerProvider`. Se sustituyen por Moq sin perder lo que los tests comprueban hoy.

## Alcance autorizado
Solo esos dos dobles y los tests que los usan, en `apps/test/backend/SumiClas.Tests`. Nada de código de producción. Los Mothers pendientes, los literales de tiempo de los tests y lo demás de la auditoría quedan fuera.

## Tareas
- [x] D1 `WeeklySummaries/Infrastructure/FakeHttpMessageHandler.cs` (cola de respuestas, registro de peticiones, `FirstRequest`, `Hang` y fallos) pasa a un `Mock<HttpMessageHandler>` (con `Protected()`), usado por `AnthropicWeeklySummaryWriterShould`. Se conserva todo lo que hoy se comprueba: cabeceras y cuerpo enviados, orden de respuestas y reintentos, espera sin respuesta y cancelación, y que una petición de más falle el test.
- [x] D2 `Api/CapturingLoggerProvider.cs` pasa a un `Mock<ILogger>` (o de `ILoggerFactory`/`ILoggerProvider`) con verificación del nivel, la excepción y el mensaje, usado por `ApiExceptionHandlerShould` y `ExceptionPipelineShould`. Se conserva lo comprobado, incluido el nivel de cada registro y que la causa se registre con el `traceId`.

## Criterios de aceptación
- Los dos ficheros hechos a mano desaparecen y no queda ningún doble escrito a mano en `apps/test/backend`.
- Los tests siguen comprobando lo mismo: se verifica con una mutación por cada comprobación relevante (p. ej. cambiar una cabecera, el nivel de log, quitar un reintento) que sigue habiendo un test que falla.
- Suite completa en verde (1245), sin `var`, sin literales numéricos nuevos sin nombre.

## Ruta
Un escritor delegado hace D1 y D2 (comparten el proyecto de tests, así que no van en paralelo). Es un refactor de tests: la red de seguridad son los propios tests más las mutaciones. Commit tras el «sí» del usuario.

## Progreso y evidencia
- D1: `FakeHttpMessageHandler` sustituido por `Mock<HttpMessageHandler>` (con `Protected()`) en `AnthropicWeeklySummaryWriterShould`; nuevo `RecordedRequest` (solo datos); el test sin clave comprueba con `Verify(Times.Never())`. D2: `CapturingLoggerProvider` sustituido por `Mock<ILogger>` y `Mock<ILoggerProvider>` en `ApiExceptionHandlerShould` y `ExceptionPipelineShould`, con la utilidad `LoggerMock`. Mutaciones hechas por el escritor, todas detectadas y restauradas: cabecera de la clave y de la versión, campo del cuerpo, método HTTP, reintentos, y nivel, excepción y `traceId` del log. Sin aplicar: que `Times.Never()` del test sin clave falle si se llama al servicio. Suite 1245 correctos, 0 fallos; `var` vacío; sin dobles a mano en `apps/test/backend`; producción intacta. Revisión nativa: no solicitada.

## Siguiente paso
Hecho. Quedan los Mothers de los tests, los comentarios y los puntos dudosos de la auditoría.
