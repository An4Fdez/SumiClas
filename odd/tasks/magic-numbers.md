# Números mágicos pendientes tras la auditoría

## Objetivo
Dar nombre a los literales numéricos con significado que la auditoría del 2026-10-25 encontró en backend, api y frontend, ya con el recorte que el usuario hizo a la regla ese mismo día.

## Alcance autorizado
- Backend (`apps/backend/src`), API (`apps/backend/api`) y frontend (`apps/frontend/sumiclas-web/src`). Los tests no se tocan («en los tests da más igual»), salvo que un test deba seguir compilando o dejar de repetir un valor que ahora tiene constante pública.
- Quedan fuera de la regla y no se tocan: los códigos de estado HTTP y las longitudes de variables, campos o parámetros (`StringLength`, `HasMaxLength`, `HasPrecision`, `Range` de un importe), incluidos los mensajes que las citan.
- Un valor se nombra en el sitio que lo posee, y los mensajes que citan un límite lo construyen con la constante.

## Tareas
- [x] M1 Backend: `CalendarEvents/Domain/EventDate.cs` (el 12 de los meses y el año 2000 para `DaysInMonth`) y `shared/application/PageCursor.cs` (las 2 partes del cursor). También el `1..12` del mensaje de `CalendarEventExceptions` si cita el mismo rango.
- [x] M2 API: `Configuration/ConfigureJwtBearerOptions.cs` (`ClockSkew` de 30 s) y `Configuration/RateLimitingServiceCollectionExtensions.cs` (límites por defecto 10 y 30 y ventana de 60), más el código de error `"Authentication.TooManyRequests"` repetido, que pasa a `ApiErrorCodes`.
- [x] M3 Frontend: `lib/date.ts` (60, 24, 7 y los milisegundos), `lib/money.ts` (100 de los céntimos), `lib/subjectColor.ts` (31 del hash) y `lib/schoolYear.ts` (mes y día del fin de curso).

## Criterios de aceptación
- Los literales listados ya no están sueltos y cada constante tiene un nombre que dice qué es.
- Mismo comportamiento: tests del backend en verde, y `pnpm typecheck` y `pnpm lint` sin errores en el frontend.
- Ningún test cambia de resultado; `grep \bvar\b` vacío en el backend.

## Ruta
Escritores delegados: uno para backend y api (una sola compilación) y otro para el frontend, en paralelo porque tocan árboles distintos. Refactor sin comportamiento nuevo: los tests existentes son la red de seguridad. Commit por tarea solo tras el «sí» del usuario.

## Progreso y evidencia
- M1 (backend): `EventDate.FirstMonth`, `LastMonth` y `LeapReferenceYear`, `PageCursor.PartCount`; el mensaje de mes inválido se construye con las constantes. M2 (api): `ConfigureJwtBearerOptions.ClockSkewSeconds`, límites y ventana del limitador de peticiones, `UnknownClient` y `ApiErrorCodes.AuthenticationTooManyRequests`. M3 (frontend): constantes de tiempo en `date.ts` (con el helper `daysSinceMonday` y `DAYS_PER_WEEK` en el calendario), `CENTS_PER_EURO`, `HASH_MULTIPLIER` y el inicio y fin del curso. Fuera por la excepción del usuario: códigos HTTP y longitudes de campos. Comprobado: suite 1245 correctos, `var` vacío, `pnpm typecheck` y `pnpm lint` sin errores, y `date.ts` idéntico a HEAD en 460 casos de calendario y semana y 56 de las demás funciones de `lib`. Sin tests tocados. Ruta: dos escritores delegados en paralelo y un ajuste mío en `date.ts`. Revisión nativa: no solicitada.

## Siguiente paso
Las tres tareas están hechas. Quedan los Mothers de los tests, los comentarios y los puntos dudosos de la auditoría, a la espera de orden.
