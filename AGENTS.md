# AGENTS.md — SumiClas (raíz del monorepo)

Este archivo aplica a todo el repositorio. Existen (o existirán) archivos `AGENTS.md` más específicos en:
- `apps/backend/AGENTS.md`
- `apps/frontend/AGENTS.md`
- `apps/test/AGENTS.md` (pendiente: `apps/test` está vacía por ahora; contendrá `apps/test/backend` y `apps/test/frontend`)

Si un archivo más específico entra en conflicto con este, gana el más específico para esa carpeta.

## Qué es este proyecto

SumiClas es una app de coordinación escolar (sustituto del grupo de WhatsApp de clase) en fase de TFM/Beta. Contexto de producto, roles, modelo de datos y reglas de privacidad de menores: ver `docs/arquitectura-app-coordinacion-escolar.md`. Hay también `docs/diseno-pantalla-inicial.md` y `docs/investigacion-requisitos-sumiclas.md`.

**Nota sobre el documento de arquitectura**: recomienda React + Next.js (PWA) sobre un backend .NET 10 Minimal API + PostgreSQL. Para el backend esa recomendación se sigue (.NET 10 + EF Core/PostgreSQL, ver `apps/backend/AGENTS.md`). Para el frontend, en cambio, **Next.js/PWA queda descartado de forma definitiva**: el stack oficial es Vite + React (ver `apps/frontend/AGENTS.md`). No propongas SSR, PWA, service worker ni migración a Next.js. El frontend ya habla con el backend real (autenticación con JWT en memoria + cookie de refresh, y buena parte de las pantallas sobre la API); el dashboard ya usa los contadores reales de la API (solo el resumen semanal con IA sigue con un mock). Supabase está instalado como dependencia del frontend pero no se usa en ningún sitio.

## Estructura del repositorio

```
apps/
  backend/
    api/        → SumiClas.Api (ASP.NET Core Web API, net10.0)
    src/        → SumiClas (librería de dominio/aplicación/infraestructura, net10.0), carpetas en inglés (shared/domain/application/infrastructure)
  frontend/
    sumiclas-web/  → Vite + React + TypeScript + Tailwind (sin backend real conectado aún)
  test/
    backend/     → tests del backend (vacío por ahora)
    frontend/    → tests del frontend, Vitest + React Testing Library (vacío por ahora)
docs/           → documentación de producto/arquitectura
SumiClas.slnx   → solución que agrupa los proyectos anteriores
```

## Instalación y arranque

### Requisitos previos
- **.NET 10 SDK** (los `.csproj` fijan `TargetFramework net10.0`).
- **Node.js** (versión compatible con pnpm/Vite; no hay `engines` fijado en `package.json` todavía).
- **pnpm** — es el gestor de paquetes oficial del frontend (el backend ya invoca `pnpm run dev` vía `SpaProxy` en `SumiClas.Api.csproj`, y hay `pnpm-workspace.yaml`).
  - Existe un `package-lock.json` residual en `apps/frontend/sumiclas-web` junto al `pnpm-lock.yaml`. Es un resto de npm: **no lo uses para instalar ni lo actualices**; puede eliminarse en una limpieza futura si el usuario lo confirma.
- **Docker** (Docker Desktop o equivalente con `docker compose`) — solo para levantar la instancia local de PostgreSQL (`docker-compose.yml` en la raíz), no para el resto de la app.

### Arrancar en desarrollo
**Flujo habitual del usuario (IDE):** abrir `SumiClas.slnx` en Visual Studio/Rider y ejecutar el proyecto `SumiClas.Api`. El `SpaProxy` configurado en el `.csproj` arranca automáticamente `pnpm run dev` en el frontend.

**Equivalente en terminal (el que debe usar el agente, que no tiene IDE):**
```bash
# PostgreSQL (desde la raíz del repo, solo la primera vez o si el contenedor no está corriendo)
docker compose up -d

# Backend (desde la raíz del repo)
dotnet run --project apps/backend/api/SumiClas.Api.csproj

# Frontend (en otra terminal)
cd apps/frontend/sumiclas-web
pnpm install   # solo la primera vez / tras cambios en dependencias
pnpm dev
```
- `docker-compose.yml` levanta solo PostgreSQL (`postgres:17-alpine`, puerto `5432`, credenciales de desarrollo en claro en el propio archivo — no son secretos reales, ver `apps/backend/AGENTS.md`). Esto es infraestructura de **desarrollo local únicamente**; el despliegue/hosting real (producción) sigue sin decidir (ver "Pendiente de decidir" en `apps/backend/AGENTS.md`). El backend usa esta base de datos en desarrollo (cadena de conexión `ConnectionStrings:SumiClasDb` en `appsettings.Development.json` y migraciones EF Core aplicadas al arrancar).
- **Clave de firma del JWT (obligatoria para arrancar la API)**: la API no arranca sin `Jwt:SigningKey` (≥ 32 caracteres) y no va en ningún `appsettings*.json`. En local, una sola vez: `dotnet user-secrets set "Jwt:SigningKey" "<≥32 chars>" --project apps/backend/api/SumiClas.Api.csproj`. En otros entornos, la variable de entorno `Jwt__SigningKey`. Ver "Autenticación de la API" en `apps/backend/AGENTS.md`.
- Frontend (Vite): `https://localhost:4004`.
- Backend (perfil `https` en `launchSettings.json`): `https://localhost:7064`.
- El proxy de Vite reenvía `/api/` a `https://localhost:7064` por defecto (el perfil `https` de `launchSettings.json`); se puede cambiar con la variable `API_PROXY_TARGET` (ver `vite.config.ts`). Web y API comparten así origen y no hay CORS.

## Convenciones comunes

- **Idioma**: desde esta decisión en adelante, **el código va en inglés** — nombres de archivo, carpetas, componentes, clases, variables, tipos, casos de uso, y comentarios de código (cuando hagan falta). En **español** se quedan: el `README`, todos los `AGENTS.md` (este incluido), los mensajes de commit, y la documentación de `docs/`. Esto es una reversión deliberada de la convención anterior ("español en todo"); no la reintroduzcas por iniciativa propia. Ver el glosario de dominio español→inglés en `apps/backend/AGENTS.md` y `apps/frontend/AGENTS.md`.
- **Texto visible para el usuario final de la app** (botones, títulos, mensajes en la UI): sigue en **español**, porque los usuarios reales (familias/profesores) son hispanohablantes. Solo el código cambia de idioma, no el contenido que se renderiza en pantalla.
- **Comentarios en código**: por defecto, no escribas comentarios (ver preferencia general de estilo). Si alguna vez son necesarios para explicar un porqué no obvio, en inglés (a partir de ahora).
- **Commits**: frase descriptiva en español, sin prefijos tipo Conventional Commits (`feat:`, `fix:`...). Ejemplos reales del historial: "Realización de la pantalla de gastos", "Cambio en los componentes de fecha y hora para adaptar al modo elegido (claro/oscuro)". Un commit, un cambio coherente. **Nunca** se añade `Co-Authored-By` ni ninguna otra atribución de IA ("Generated with...", firmas de asistentes) en commits, PRs, código ni documentación, aunque la herramienta lo sugiera por defecto; si un subagente va a commitear, díselo expresamente.
- **Ramas**: se trabaja commiteando directo a `main` (no hay ramas de feature ni PRs en este proyecto). No crees ramas por tu cuenta salvo que el usuario lo pida explícitamente para una tarea concreta.
- **Estilo/formato**:
  - **Sin números mágicos** (decisión del usuario, 2026-10-24): todo literal numérico con significado (límites, plazos, periodos, tamaños, reintentos, códigos de estado...) va en una constante con nombre (`const` en C#, `const` en TypeScript), en el sitio que lo posee: p. ej. `SummaryPeriod.MinDays/DefaultDays/MaxDays`, `ApiExceptionHandler.RetryAfterSeconds`. Los mensajes que citan un límite lo construyen con la constante, no con el número escrito. En los tests, los datos que representan un valor del dominio (un periodo, un límite, un instante) también llevan nombre (`REQUESTED_DAYS`, `JUST_OUTSIDE`) y los resultados esperados se derivan de los datos de prueba (`unread.Length`) en lugar de repetir un número; el 0 y el 1 evidentes (un índice, `Length - 1`) no necesitan constante. El código anterior a esta regla puede conservar algún literal: se corrige al tocarlo, no en una pasada aparte.
  - Frontend: ESLint + `typescript-eslint` (`apps/frontend/sumiclas-web/eslint.config.js`) es la única verificación de estilo configurada. No hay Prettier.
  - Backend: no hay `dotnet format` ni analizadores de estilo configurados todavía. Carpetas de capa en inglés (`shared/application`, `shared/domain`, `shared/infrastructure`). **Regla obligatoria: tipos explícitos, nunca `var`**, en `src`, `api` y los tests (detalle y cómo cumplirla en `apps/backend/AGENTS.md`).
  - No hay hooks de pre-commit (ni Husky ni equivalente) en este repo.

## Reglas generales

**Antes de dar por terminada una tarea:**
- Si tocaste el frontend (`apps/frontend/sumiclas-web`): ejecuta `pnpm typecheck` y `pnpm lint` (o `npm run typecheck` / `npm run lint`, equivalentes mientras exista el lockfile de npm) y que pasen sin errores.
- Si tocaste el backend (`apps/backend/**`): ejecuta `dotnet build` sobre la solución o el proyecto afectado y que compile sin errores.
- Cuando exista contenido en `apps/test/backend` o `apps/test/frontend`, añadir aquí la ejecución de esos tests como paso obligatorio antes de terminar (según la zona tocada).

**No toques sin permiso explícito:**
- La estructura de la solución: `SumiClas.slnx`, los `.csproj` (`SumiClas.Api.csproj`, `SumiClas.csproj`) y el `.esproj` del frontend. Si una tarea requiere añadir un proyecto nuevo o cambiar referencias, confírmalo antes con el usuario.
- Secretos y configuración local: `appsettings.*.Local.json`, `.env` y cualquier archivo que `.gitignore` ya marque como local/sensible. Hay conexión real a PostgreSQL local (credenciales de desarrollo de `docker-compose.yml`, no secretas) y una clave de firma de JWT que vive solo en User Secrets o en `Jwt__SigningKey`: nunca la escribas en un archivo del repositorio. Supabase no se usa. Trata como sensible por defecto cualquier otro secreto que aparezca.
- No hay `.env.example` ni equivalente que mantener actualizado por ahora; cuando se conecte un backend/BD real, esto habrá que revisarlo y documentarlo.

## Referencias
- Arquitectura y dominio: `docs/arquitectura-app-coordinacion-escolar.md`
- Diseño de pantalla inicial: `docs/diseno-pantalla-inicial.md`
- Investigación de requisitos: `docs/investigacion-requisitos-sumiclas.md`
- Glosario de dominio español→inglés (para nombrar código): `docs/glosario-dominio.md`
