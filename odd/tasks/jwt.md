# JWT, refresh token rotatorio en cookie y CORS

## Objetivo
Sustituir la identidad falsificable `X-User-Id` por tokens JWT firmados (access de 15 min en memoria) más un refresh token rotatorio en cookie httpOnly (guardado con hash en la base de datos), con cierre de sesión real, autenticación obligatoria por defecto y CORS restrictivo.

## Fuente de verdad
`docs/specs/jwt-authentication.md` (desarrollo guiado por especificación: explicación, requisitos RF-01..RF-21, escenarios S-01..S-28, diseño, tareas y matriz de trazabilidad). No se implementa nada que no esté en ese documento; si cambia un requisito, se edita primero la especificación.

## Decisiones (usuario)
- Access token de vida corta en memoria + refresh token rotatorio en cookie httpOnly, guardado con hash.
- Producción en un solo dominio con proxy `/api` (mismo origen); CORS restrictivo y configurable.
- Email + contraseña (ya implementado); magic link fuera.

## Restricciones
- Seguir `apps/backend/AGENTS.md` y `apps/frontend/AGENTS.md`.
- Ningún token, hash ni clave en los logs; el access token nunca en `localStorage`/`sessionStorage`.
- Un `Result` fallido revierte la unidad de trabajo, así que la detección de reutilización y el logout devuelven un resultado exitoso con un resultado de dominio que el endpoint traduce a 401 (RNF-02).
- Fuera de alcance: autorización por rol y pertenencia a la clase, recuperación de contraseña, bloqueo de cuenta, limpieza de tokens caducados, infraestructura de tests HTTP y de frontend.

## Tareas (ver la sección 10 de la especificación: archivos, dependencias y escenarios)
- [x] J0 Guardar la especificación en `docs/specs/jwt-authentication.md` y crear esta ficha (hecho, sin commitear).
- [ ] J1a Dominio `RefreshToken`, especificación de consulta, puerto del repositorio, errores, `RefreshTokenInvalid` → 401 y tests unitarios.
- [ ] J1b `JwtOptions` con validación, emisor del access token, generador del refresh token, paquete JwtBearer, claves de configuración y tests.
- [ ] J1c Mapeo EF, `RefreshTokenRepository` y migración `RefreshTokens`.
- [ ] J2a Casos de uso `AccountLogin` (sesión + refresh), `AccountTokenRefresh`, `AccountLogout` y tests.
- [ ] J2b API: JwtBearer, política por defecto y lista de anónimos, ProblemDetails 401/403, `JwtUserProvider` (se borra `HeaderUserProvider`), endpoints de refresh y logout con cookie, CORS, limitador de refresh y esquema Bearer en OpenAPI.
- [ ] J3a Regenerar el cliente NSwag; `tokenStore`, `authHttp` (una sola llamada de refresco compartida, locks, reintento), refresh/logout en el repositorio y retirada de `X-User-Id`.
- [ ] J3b `AuthProvider`/`useAuth`, `RequireSession`, Login, Register (registro y después login), cerrar sesión y mensajes.
- [ ] J4 Verificación E2E real (API y Postgres para S-01..S-20, Chrome headless para S-21..S-28), limpieza de datos, documentación final, quitar las notas de «identidad provisional» y espejo en Engram.

## Criterios de aceptación
Todos los escenarios S-01..S-28 tienen evidencia registrada (matriz de la sección 11 de la especificación); `dotnet build`, `dotnet test`, `pnpm typecheck` y `pnpm lint` limpios; cada commit revisado y compilando por sí solo.

## Progreso
Especificación escrita y aprobada por el usuario, con la explicación de cómo funciona añadida. Ningún código implementado.

## Siguiente paso
El usuario revisa `docs/specs/jwt-authentication.md`; después se empieza J1a (escritor de backend delegado).
