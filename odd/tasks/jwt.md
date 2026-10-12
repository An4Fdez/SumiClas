# JWT, refresh token rotatorio en cookie y CORS

## Objetivo
Sustituir la identidad falsificable `X-User-Id` por tokens JWT firmados (access de 15 min en memoria) más un refresh token rotatorio en cookie httpOnly (guardado con hash en la base de datos), con cierre de sesión real, autenticación obligatoria por defecto y sin CORS (mismo origen).

## Fuente de verdad
`docs/specs/jwt-authentication.md` (desarrollo guiado por especificación: explicación, requisitos RF-01..RF-21, escenarios S-01..S-28, diseño, tareas y matriz de trazabilidad). No se implementa nada que no esté en ese documento; si cambia un requisito, se edita primero la especificación.

## Decisiones (usuario)
- Access token de vida corta en memoria + refresh token rotatorio en cookie httpOnly, guardado con hash.
- Producción en un solo dominio con proxy `/api` (mismo origen). Decisión posterior: **no se configura CORS** (ni en producción ni en desarrollo, donde el proxy de Vite mantiene el mismo origen); el navegador bloquea por defecto lo que venga de otro origen, que ya es lo restrictivo. Si algún día la web se sirve desde otro origen habrá que añadirlo.
- Email + contraseña (ya implementado); magic link fuera.
- Un agregado por módulo: `RefreshToken` vive en su propio módulo `RefreshTokens`. Un caso de uso toca únicamente el repositorio de su módulo; lo demás va por `IQueryBus` o por eventos.
- El agregado de la sesión se llama `Session` (módulo `Sessions`, raíz = sesión, hijas = `RefreshToken`); `AccessToken` es value object y el JWT se genera por un puerto de `Domain`. Decidido tras descartar `Auth` (no es una raíz de agregado y rompería la regla de nombres).
- `Infrastructure` solo depende de `Domain`: los puertos (`IPasswordHasher`, `IAccessTokenGenerator`, `IRefreshTokenGenerator`) y `JwtOptions` viven en `Domain`.
- `AccessToken` es un value object de `Domain` (`ValueObject` + `Create` → `Result`).
- `User` y `UserCredential` se unifican en un solo agregado y una sola tabla (1:1, mismo ciclo de vida); el módulo `Authentication` se elimina. El token (`RefreshTokens`) sigue siendo un módulo aparte.
- El login pasa a ser `RefreshTokenCreate` (módulo `RefreshTokens`), que verifica la contraseña con la query `UserPasswordVerify` (módulo `Users`, con el rehash dentro del `UseCase`).

## Restricciones
- Seguir `apps/backend/AGENTS.md` y `apps/frontend/AGENTS.md`.
- Ningún token, hash ni clave en los logs; el access token nunca en `localStorage`/`sessionStorage`.
- Un `Result` fallido revierte la unidad de trabajo, así que la detección de reutilización y el logout devuelven un resultado exitoso con un resultado de dominio que el endpoint traduce a 401 (RNF-02).
- Fuera de alcance: autorización por rol y pertenencia a la clase, recuperación de contraseña, bloqueo de cuenta, limpieza de tokens caducados, infraestructura de tests HTTP y de frontend.
- Decisión del usuario: recuperación y cambio de contraseña, bloqueo de cuenta por intentos, rol dentro del token y limpieza periódica de sesiones caducadas **no son necesarios para el MVP** (consecuencias asumidas: una contraseña olvidada se resuelve a mano, el limitador de 10 por minuto por IP es la única defensa contra fuerza bruta, y la tabla de sesiones solo crece despacio).
- Decisión del usuario: **solo HTTPS** (sin `UseHttpsRedirection`; no se publica ningún puerto HTTP) y **`/openapi` no es anónimo ni en Development** (exige Bearer; el cliente NSwag se genera descargando el documento con un token en J3a; Swagger UI no puede leerlo).
- Regla práctica del usuario para registrar servicios: si seguiría haciendo falta en un proyecto de consola que reutilice el dominio → infraestructura; si solo sirve para recibir peticiones HTTP → capa API.

## Tareas (ver la sección 10 de la especificación: archivos, dependencias y escenarios)
- [x] J0 Guardar la especificación en `docs/specs/jwt-authentication.md` y crear esta ficha (commit `9c769c4`).
- [x] J1a Dominio `RefreshToken`, especificación de consulta, puerto del repositorio, errores, `RefreshTokenInvalid` → 401 y tests unitarios (commits `2cbe60b`, `193be03`).
- [x] J1b `JwtOptions` con validación, emisor del access token, generador del refresh token, paquete JwtBearer, claves de configuración y tests (commit `cf3474b`).
- [x] J1c Mapeo EF, `RefreshTokenRepository` y migración `RefreshTokens` (commits `5f7bec8`, `d372970`).
- [x] J1d Reestructurar por módulos y capas: módulo `RefreshTokens` (Domain, Application, Infrastructure), puertos y `JwtOptions` a `Domain`, `AccessToken` como value object, `JwtOptions` validado al arrancar en `AddSumiClasJwt` (commits `cf9cffc` y `8225d58`; el reparto de los ficheros tuvo que unir parte de J1d, J1e y J2a para que cada commit compile).
- [x] J1e Unificar `Authentication` en `User` (una sola tabla `Users`; `UserRegister`, `UserPasswordVerify`, `Password`, `IPasswordHasher` en `Users`; módulo `Authentication` eliminado) con la migración `MergeUserCredentialsIntoUsers`, que para con un mensaje claro si hay usuarios sin credencial (commit `8225d58`; migración probada en una base de datos desechable: guarda, copia el hash y el `Down` lo restaura).
- [x] J2a Casos de uso de sesión `RefreshTokenCreate`, `RefreshTokenRotate` y `RefreshTokenRevoke` con tests (commit `8225d58`). El login todavía no pone la cookie: eso es J2b.
- [x] J1f Todos los casos de uso pasan a `Query|Command` + clase con el nombre de la acción (`Create`, `Finder`, `Mark`…) que implementa `IUseCase` + `Handler` que solo delega; la convención de DI registra por `IUseCase` y un test de guarda lo protege (commits `7fd1ab3` y `080c0e2`; 260 tests).
- [x] J1g Agregado `Session` (módulo `Sessions`) en lugar de `RefreshToken`/`RefreshTokens`: la raíz es la sesión (lo que hoy es la «familia») y `RefreshToken` pasa a entidad hija; revocar y detectar reutilización son comportamiento del agregado; token de concurrencia `xmin` en `Session` (la revisión señaló que probablemente no protege la rotación: ver seguimiento 1); casos de uso `SessionCreate`/`SessionRotate`/`SessionRevoke`; migración de datos de `RefreshTokens` a `Sessions`; tests y documentación (commits `1aee3d1`, `9cc8fcd` y `2aee133`: módulo y migración, tests, y el repositorio inyectado llamado siempre `repository`).
- [x] J1g2 `Configuration` en lugar de `Bootstrap` en la API (commit `cef780a`) y duraciones como constantes de las entidades con `JwtOptions` en `Infrastructure` (commit `f201be4`).
- [x] J1h Concurrencia de la rotación (probada con dos contextos contra PostgreSQL: sin tocar la raíz la carrera no se detectaba y quedaban 2 sucesores activos; con `LastRotatedAt` el segundo guardado falla y queda 1): la raíz `Session` debe modificarse en cada rotación (`LastRotatedAt`) para que el token `xmin` se compruebe, traducir `DbUpdateConcurrencyException` a `ConcurrencyConflictException` → 409, migración de la columna y prueba real con dos contextos contra PostgreSQL.
- [x] J2b API: JwtBearer, política por defecto y lista de anónimos, ProblemDetails 401/403, `JwtUserProvider` (se borra `HeaderUserProvider`), endpoints de refresh y logout con cookie (`RefreshCookie`), `SessionResponse` con `RefreshTokenExpiresAt`, limitador `auth-refresh` y esquema Bearer en OpenAPI. Sin CORS (decisión del usuario: web y API en el mismo origen). Hecho y verificado: 335 tests y prueba de humo real (S-01..S-17, S-19, S-20) con base de datos desechable; registros de DI reorganizados (`AddSumiClasInfrastructure` con los generadores y `JwtOptions`, `AddSumiClasApiSecurity` para lo web). Commits: `95a5208` (contrato), `21c0047` (API), `640a142` (tests) y `697a9fa` (documentación).
- [x] J3a Regenerar el cliente NSwag; `tokenStore`, `authHttp` (una sola llamada de refresco compartida, locks, reintento), refresh/logout en el repositorio y retirada de `X-User-Id`. Hecho (commits `f3519c8` script del generador, `573f4d1` cliente regenerado, `d59fecd` capa de sesión y módulos `sessions`/`users`, `78e9bb2` documentación): `NewSession`/`NewUser` en lugar de `Credentials`/`AccountRegistration`; el arranque restaura la sesión con la cookie y un manejador provisional redirige a `/login`, que J3b sustituye por `AuthProvider`.
- [ ] J3b `AuthProvider`/`useAuth`, `RequireSession`, Login, Register (registro y después login), cerrar sesión y mensajes.
- [ ] J4 Verificación E2E real (API y Postgres para S-01..S-20, Chrome headless para S-21..S-28), limpieza de datos, documentación final, quitar las notas de «identidad provisional» y espejo en Engram.

## Criterios de aceptación
Todos los escenarios S-01..S-28 tienen evidencia registrada (matriz de la sección 11 de la especificación); `dotnet build`, `dotnet test`, `pnpm typecheck` y `pnpm lint` limpios; cada commit revisado y compilando por sí solo.

## Progreso
Especificación y J1a–J1f commiteados (260 tests; cada pieza revisada por la revisión nativa). Falta J2b en adelante.

## Siguiente paso
Seguimientos que salieron de las revisiones, a cerrar en J2b/J4: (1) concurrencia en `SessionRotate`: resuelta en J1h; en J2b el endpoint `/auth/refresh` debe tratar el 409 `Shared.ConcurrencyConflict` como la carrera benigna entre pestañas (el frontend reintenta una vez, RF-19); (1b) la poda de tokens reemplazados hace más de 24 h convierte un reintento muy antiguo en `Invalid` en vez de `Reused` (decisión aceptada, anotada en `AGENTS.md`); (2) el rehash dentro de `UserPasswordVerify` solo se ha probado con mocks: comprobarlo contra la base de datos real en el E2E de J4; (3) tests de la validación de `JwtOptions` (pendiente de tu permiso para referenciar `SumiClas.Api` desde el proyecto de tests); (4) el login hoy guarda un refresh token que nadie usa hasta que J2b ponga la cookie. Después: J2b. (5) frontend: el arranque espera al refresco sin tope de tiempo (si la API no responde no se pinta nada; J3b lo cambia por una pantalla de carga), `authHttp`/`refreshSession` sin tests automáticos (no hay Vitest todavía; solo hay una prueba desechable), y el generador del cliente confunde un 401 durante la espera con un timeout.
