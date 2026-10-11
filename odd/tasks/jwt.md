# JWT, refresh token rotatorio en cookie y CORS

## Objetivo
Sustituir la identidad falsificable `X-User-Id` por tokens JWT firmados (access de 15 min en memoria) más un refresh token rotatorio en cookie httpOnly (guardado con hash en la base de datos), con cierre de sesión real, autenticación obligatoria por defecto y CORS restrictivo.

## Fuente de verdad
`docs/specs/jwt-authentication.md` (desarrollo guiado por especificación: explicación, requisitos RF-01..RF-21, escenarios S-01..S-28, diseño, tareas y matriz de trazabilidad). No se implementa nada que no esté en ese documento; si cambia un requisito, se edita primero la especificación.

## Decisiones (usuario)
- Access token de vida corta en memoria + refresh token rotatorio en cookie httpOnly, guardado con hash.
- Producción en un solo dominio con proxy `/api` (mismo origen); CORS restrictivo y configurable.
- Email + contraseña (ya implementado); magic link fuera.
- Un agregado por módulo: `RefreshToken` vive en su propio módulo `RefreshTokens` y `Authentication` se queda solo con `UserCredential`. Un caso de uso toca únicamente el repositorio de su módulo; lo demás va por `IQueryBus` o por eventos.
- `Infrastructure` solo depende de `Domain`: los puertos (`IPasswordHasher`, `IAccessTokenGenerator`, `IRefreshTokenGenerator`) y `JwtOptions` viven en `Domain`.
- `AccessToken` es un value object de `Domain` (`ValueObject` + `Create` → `Result`).
- `User` y `UserCredential` se unifican en un solo agregado y una sola tabla (1:1, mismo ciclo de vida); el módulo `Authentication` se elimina. El token (`RefreshTokens`) sigue siendo un módulo aparte.
- El login pasa a ser `RefreshTokenCreate` (módulo `RefreshTokens`), que verifica la contraseña con la query `AccountCredentialVerify` (módulo `Authentication`, con el rehash dentro del handler).

## Restricciones
- Seguir `apps/backend/AGENTS.md` y `apps/frontend/AGENTS.md`.
- Ningún token, hash ni clave en los logs; el access token nunca en `localStorage`/`sessionStorage`.
- Un `Result` fallido revierte la unidad de trabajo, así que la detección de reutilización y el logout devuelven un resultado exitoso con un resultado de dominio que el endpoint traduce a 401 (RNF-02).
- Fuera de alcance: autorización por rol y pertenencia a la clase, recuperación de contraseña, bloqueo de cuenta, limpieza de tokens caducados, infraestructura de tests HTTP y de frontend.

## Tareas (ver la sección 10 de la especificación: archivos, dependencias y escenarios)
- [x] J0 Guardar la especificación en `docs/specs/jwt-authentication.md` y crear esta ficha (commit `9c769c4`).
- [x] J1a Dominio `RefreshToken`, especificación de consulta, puerto del repositorio, errores, `RefreshTokenInvalid` → 401 y tests unitarios (commits `2cbe60b`, `193be03`).
- [x] J1b `JwtOptions` con validación, emisor del access token, generador del refresh token, paquete JwtBearer, claves de configuración y tests (commit `cf3474b`).
- [x] J1c Mapeo EF, `RefreshTokenRepository` y migración `RefreshTokens` (commits `5f7bec8`, `d372970`).
- [x] J1d Reestructurar por módulos y capas: módulo `RefreshTokens` (Domain, Application, Infrastructure), puertos y `JwtOptions` a `Domain`, `AccessToken` como value object, `JwtOptions` validado al arrancar en `AddSumiClasJwt` (commits `cf9cffc` y `8225d58`; el reparto de los ficheros tuvo que unir parte de J1d, J1e y J2a para que cada commit compile).
- [x] J1e Unificar `Authentication` en `User` (una sola tabla `Users`; `UserRegister`, `UserPasswordVerify`, `Password`, `IPasswordHasher` en `Users`; módulo `Authentication` eliminado) con la migración `MergeUserCredentialsIntoUsers`, que para con un mensaje claro si hay usuarios sin credencial (commit `8225d58`; migración probada en una base de datos desechable: guarda, copia el hash y el `Down` lo restaura).
- [x] J2a Casos de uso de sesión `RefreshTokenCreate`, `RefreshTokenRotate` y `RefreshTokenRevoke` con tests (commit `8225d58`). El login todavía no pone la cookie: eso es J2b.
- [x] J1f Todos los casos de uso pasan a `Query|Command` + clase con el nombre de la acción (`Create`, `Finder`, `Mark`…) que implementa `IUseCase` + `Handler` que solo delega; la convención de DI registra por `IUseCase` y un test de guarda lo protege (commits `7fd1ab3` y `080c0e2`; 260 tests).
- [ ] J2b API: JwtBearer, política por defecto y lista de anónimos, ProblemDetails 401/403, `JwtUserProvider` (se borra `HeaderUserProvider`), endpoints de refresh y logout con cookie, CORS, limitador de refresh y esquema Bearer en OpenAPI.
- [ ] J3a Regenerar el cliente NSwag; `tokenStore`, `authHttp` (una sola llamada de refresco compartida, locks, reintento), refresh/logout en el repositorio y retirada de `X-User-Id`.
- [ ] J3b `AuthProvider`/`useAuth`, `RequireSession`, Login, Register (registro y después login), cerrar sesión y mensajes.
- [ ] J4 Verificación E2E real (API y Postgres para S-01..S-20, Chrome headless para S-21..S-28), limpieza de datos, documentación final, quitar las notas de «identidad provisional» y espejo en Engram.

## Criterios de aceptación
Todos los escenarios S-01..S-28 tienen evidencia registrada (matriz de la sección 11 de la especificación); `dotnet build`, `dotnet test`, `pnpm typecheck` y `pnpm lint` limpios; cada commit revisado y compilando por sí solo.

## Progreso
Especificación y J1a–J1f commiteados (260 tests; cada pieza revisada por la revisión nativa). Falta J2b en adelante.

## Siguiente paso
Seguimientos que salieron de las revisiones, a cerrar en J2b/J4: (1) concurrencia en `RefreshTokenRotate`: dos refrescos simultáneos con el mismo token pueden crear dos sucesores activos (valorar un token de concurrencia `xmin` en `RefreshToken` y qué respuesta dar); (2) el rehash dentro de `UserPasswordVerify` solo se ha probado con mocks: comprobarlo contra la base de datos real en el E2E de J4; (3) tests de la validación de `JwtOptions` (pendiente de tu permiso para referenciar `SumiClas.Api` desde el proyecto de tests); (4) el login hoy guarda un refresh token que nadie usa hasta que J2b ponga la cookie. Después: J2b.
