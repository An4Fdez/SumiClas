# AGENTS.md — SumiClas (raíz del monorepo)

Este archivo aplica a todo el repositorio. Existen archivos `AGENTS.md` más específicos en:
- `apps/backend/AGENTS.md`
- `apps/frontend/AGENTS.md`

Los tests no tienen `AGENTS.md` propio: cómo se escriben y ejecutan los del backend (`apps/test/backend`) está en la skill `backend-testing`, y los del frontend (`apps/test/frontend`) todavía no existen.

Si un archivo más específico entra en conflicto con este, gana el más específico para esa carpeta.

## Qué es este proyecto

SumiClas es una app de coordinación escolar (sustituto del grupo de WhatsApp de clase) en fase de TFM/Beta. Contexto de producto, roles, modelo de datos y reglas de privacidad de menores: ver `docs/arquitectura-app-coordinacion-escolar.md`. Hay también `docs/diseno-pantalla-inicial.md` y `docs/investigacion-requisitos-sumiclas.md`.

**Nota sobre el documento de arquitectura**: recomienda React + Next.js (PWA) sobre un backend .NET 10 Minimal API + PostgreSQL. Para el backend esa recomendación se sigue (.NET 10 + EF Core/PostgreSQL, ver `apps/backend/AGENTS.md`). Para el frontend, en cambio, **Next.js/PWA queda descartado de forma definitiva**: el stack oficial es Vite + React (ver `apps/frontend/AGENTS.md`). No propongas SSR, PWA, service worker ni migración a Next.js. El frontend ya habla con el backend real (autenticación con JWT en memoria + cookie de refresh, y buena parte de las pantallas sobre la API); el dashboard ya usa los contadores reales de la API y su resumen semanal con IA también sale de la API (sin clave de la IA la tarjeta muestra un error con el botón «Generar resumen»). Supabase está instalado como dependencia del frontend pero no se usa en ningún sitio.

## Estructura del repositorio

```
apps/
  backend/
    api/        → SumiClas.Api (ASP.NET Core Web API, net10.0)
    src/        → SumiClas (librería de dominio/aplicación/infraestructura, net10.0), carpetas en inglés (shared/domain/application/infrastructure)
    tools/      → herramientas puntuales (el importador del catálogo de colegios, pendiente de borrar; ver la skill `backend-territorial-catalog`)
  frontend/
    sumiclas-web/  → Vite + React + TypeScript + Tailwind (habla con la API real por el cliente NSwag generado)
  test/
    backend/     → tests del backend (proyecto SumiClas.Tests: NUnit, Moq, AwesomeAssertions y Testcontainers; ver la skill `backend-testing`)
    frontend/    → tests del frontend, Vitest + React Testing Library (la carpeta aún no existe: todavía no hay tests del frontend)
docs/           → documentación de producto/arquitectura (y la presentación en docs/presentacion)
terraform/      → infraestructura de Azure (PostgreSQL Flexible Server y App Service en Windows) definida con Terraform
scripts/        → scripts auxiliares (demo-data: cargador de datos de demostración y export-sql.mjs, que genera los SQL de la demo y el de refresco de fechas; deploy: publish-prod.ps1, que publica en el App Service de Azure; el agente no lo ejecuta, lo lanza el usuario)
SumiClas.slnx   → solución que agrupa los proyectos anteriores
.claude/skills/ → skills del proyecto (ver "Skills del proyecto")
```

## Skills del proyecto

Los `AGENTS.md` solo llevan lo que hay que leer siempre. El detalle de cada tema (patrones, convenciones, decisiones acordadas y huecos conocidos) está repartido en **skills**, una carpeta por tema con su `SKILL.md` en `.claude/skills/<nombre>/`. Cada una empieza por una descripción que dice cuándo usarla.

- **Antes de escribir o cambiar código** de `apps/backend/**` o `apps/frontend/**`, abre el `AGENTS.md` de esa carpeta, busca en su tabla «Si vas a… → Lee la skill» las que apliquen a la tarea y léelas enteras. Casi siempre son varias.
- Las skills se leen por su ruta (`.claude/skills/<nombre>/SKILL.md`), así que sirven con cualquier agente aunque no las cargue por su cuenta.
- Si una skill y el código no coinciden, coméntalo antes de asumir cuál de los dos manda; si cambias algo que una skill describe, **actualiza esa skill** en el mismo cambio.
- Backend (`apps/backend/AGENTS.md`): `backend-architecture`, `backend-domain`, `backend-use-cases`, `backend-cross-module`, `backend-endpoints`, `backend-errors`, `backend-read-queries`, `backend-specifications`, `backend-persistence`, `backend-auth`, `backend-authorization`, `backend-territorial-catalog`, `backend-weekly-summary`, `backend-testing` y `backend-status-notes`.
- Frontend (`apps/frontend/AGENTS.md`): `frontend-architecture`, `frontend-api-integration`, `frontend-auth-session`, `frontend-ui-patterns` y `frontend-screens`.

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
- `docker-compose.yml` levanta solo PostgreSQL (`postgres:17-alpine`, puerto `5432`, credenciales de desarrollo en claro en el propio archivo — no son secretos reales, ver `apps/backend/AGENTS.md`). Esto es infraestructura de **desarrollo local únicamente**; el despliegue real (producción) es en Azure (decisión del 2026-10-24) y se define con Terraform en `terraform/` (ver "Despliegue" en el `Readme.md` y "Pendiente de decidir" en `apps/backend/AGENTS.md`). El backend usa esta base de datos en desarrollo (cadena de conexión `ConnectionStrings:SumiClasDb` en `appsettings.Development.json` y migraciones EF Core aplicadas al arrancar).
- **Clave de firma del JWT (obligatoria para arrancar la API)**: la API no arranca sin `Jwt:SigningKey` (≥ 32 caracteres) y no va en ningún `appsettings*.json`. En local, una sola vez: `dotnet user-secrets set "Jwt:SigningKey" "<≥32 chars>" --project apps/backend/api/SumiClas.Api.csproj`. En otros entornos, la variable de entorno `Jwt__SigningKey`. Ver la skill `backend-auth`.
- **Clave de la IA del resumen semanal (opcional)**: sin `Ai:ApiKey` la función queda apagada (la tarjeta del dashboard muestra un error con «Generar resumen» cuando la semana tiene novedades) y el resto de la app funciona igual. No va en ningún `appsettings*.json` ni en ningún archivo del repositorio. En local, una sola vez y en tu propia terminal: `dotnet user-secrets set "Ai:ApiKey" "<clave de Anthropic>" --project apps/backend/api/SumiClas.Api.csproj`. En otros entornos, la variable de entorno `Ai__ApiKey` (en Azure, ajuste de la aplicación escrito a mano en el portal; no se usa Key Vault, decisión del 2026-10-27). Ver la skill `backend-weekly-summary`.
- Frontend (Vite): `http://localhost:4004` (HTTP; `vite.config.ts` no activa HTTPS, y el proxy llama a la API por HTTPS con `secure: false`).
- Backend (perfil `https` en `launchSettings.json`): `https://localhost:7064`.
- El proxy de Vite reenvía `/api/` a `https://localhost:7064` por defecto (el perfil `https` de `launchSettings.json`); se puede cambiar con la variable `API_PROXY_TARGET` (ver `vite.config.ts`). Web y API comparten así origen y no hay CORS.
- **Datos de demostración**: `scripts/demo-data/` (Node, sin dependencias, fuera de `SumiClas.slnx`) carga por la API real unas familias, clases y contenido inventados, en español, para presentar la web (`node scripts/demo-data/seed.mjs load|reset|verify`). Las cuentas y la contraseña (`Demo1234`) son falsas y están en el `Readme.md` a propósito; el script se niega por defecto a hablar con una API que no sea local (solo `--allow-remote` y `https`, decisión del usuario del 2026-10-28; cuidado: en remoto la contraseña pública permite entrar a cualquiera) y añade él mismo el prefijo `/api` a las rutas (hay que tocarlo si cambia el prefijo). Las fechas son relativas a un único día ancla (`--day`, por defecto hoy; ventana de 6 días antes a 14 después): las de eventos, deberes y cumpleaños salen de la API, y las de publicación de avisos, objetos perdidos y gastos se colocan con un `UPDATE` local por `docker exec` (la API no deja fijarlas). Un aviso centinela con fecha exactamente igual al ancla permite a un script SQL de refresco saber en qué día está la demo. `reset` borra solo lo creado por la demo.

## Convenciones comunes

- **Idioma**: desde esta decisión en adelante, **el código va en inglés** — nombres de archivo, carpetas, componentes, clases, variables, tipos, casos de uso, y comentarios de código (cuando hagan falta). En **español** se quedan: el `README`, todos los `AGENTS.md` (este incluido), los mensajes de commit, y la documentación de `docs/`. Esto es una reversión deliberada de la convención anterior ("español en todo"); no la reintroduzcas por iniciativa propia. Ver el glosario de dominio español→inglés en `apps/backend/AGENTS.md` y `apps/frontend/AGENTS.md`.
- **Texto visible para el usuario final de la app** (botones, títulos, mensajes en la UI): sigue en **español**, porque los usuarios reales (familias/profesores) son hispanohablantes. Solo el código cambia de idioma, no el contenido que se renderiza en pantalla.
- **Comentarios en código**: por defecto, no escribas comentarios (ver preferencia general de estilo). Si alguna vez son necesarios para explicar un porqué no obvio, en inglés (a partir de ahora).
- **Commits**: frase descriptiva en español, sin prefijos tipo Conventional Commits (`feat:`, `fix:`...). Ejemplos reales del historial: "Realización de la pantalla de gastos", "Cambio en los componentes de fecha y hora para adaptar al modo elegido (claro/oscuro)". Un commit, un cambio coherente. **Nunca** se añade `Co-Authored-By` ni ninguna otra atribución de IA ("Generated with...", firmas de asistentes) en commits, PRs, código ni documentación, aunque la herramienta lo sugiera por defecto; si un subagente va a commitear, díselo expresamente.
- **Ramas**: se trabaja commiteando directo a `main` (no hay ramas de feature ni PRs en este proyecto). No crees ramas por tu cuenta salvo que el usuario lo pida explícitamente para una tarea concreta.
- **Estilo/formato**:
  - **Sin números mágicos** (decisión del usuario, 2026-10-24): todo literal numérico con significado (límites, plazos, periodos, tamaños, reintentos...) va en una constante con nombre (`const` en C#, `const` en TypeScript), en el sitio que lo posee: p. ej. `SummaryPeriod.MinDays/DefaultDays/MaxDays`, `ApiExceptionHandler.RetryAfterSeconds`. **Quedan fuera de la regla** (decisión del usuario, 2026-10-25): los códigos de estado HTTP (`500`, `401`, `409`...) y las longitudes máximas o mínimas de variables, campos o parámetros (`[StringLength(200)]`, `HasMaxLength(320)`, `HasPrecision(10, 2)`, `[Range]` de un importe): se escriben tal cual. Los mensajes que citan un límite lo construyen con la constante, no con el número escrito. En los tests, los datos que representan un valor del dominio (un periodo, un límite, un instante) también llevan nombre (`REQUESTED_DAYS`, `JUST_OUTSIDE`) y los resultados esperados se derivan de los datos de prueba (`unread.Length`) en lugar de repetir un número; el 0 y el 1 evidentes (un índice, `Length - 1`) no necesitan constante. El código anterior a esta regla puede conservar algún literal: se corrige al tocarlo, no en una pasada aparte.
  - Frontend: ESLint + `typescript-eslint` (`apps/frontend/sumiclas-web/eslint.config.js`) es la única verificación de estilo configurada. No hay Prettier.
  - Backend: no hay `dotnet format` ni analizadores de estilo configurados todavía. Carpetas de capa en inglés (`shared/application`, `shared/domain`, `shared/infrastructure`). **Regla obligatoria: tipos explícitos, nunca `var`**, en `src`, `api` y los tests (detalle y cómo cumplirla en `apps/backend/AGENTS.md`).
  - No hay hooks de pre-commit (ni Husky ni equivalente) en este repo.

## Reglas generales

**Antes de dar por terminada una tarea:**
- Si tocaste el frontend (`apps/frontend/sumiclas-web`): ejecuta `pnpm typecheck` y `pnpm lint` (o `npm run typecheck` / `npm run lint`, equivalentes mientras exista el lockfile de npm) y que pasen sin errores.
- Si tocaste el backend (`apps/backend/**`): ejecuta `dotnet build` sobre la solución o el proyecto afectado y que compile sin errores.
- Si tocaste el backend, ejecuta además sus tests: `dotnet test apps/test/backend/SumiClas.Tests` y que pasen todos (los que necesitan PostgreSQL arrancan un contenedor con Docker; detalle en la skill `backend-testing`). Cuando existan tests del frontend en `apps/test/frontend`, su ejecución se añadirá aquí como paso obligatorio.

**No toques sin permiso explícito:**
- La estructura de la solución: `SumiClas.slnx`, los `.csproj` (`SumiClas.Api.csproj`, `SumiClas.csproj`) y el `.esproj` del frontend. Si una tarea requiere añadir un proyecto nuevo o cambiar referencias, confírmalo antes con el usuario.
- Secretos y configuración local: `appsettings.*.Local.json`, `.env` y cualquier archivo que `.gitignore` ya marque como local/sensible. Hay conexión real a PostgreSQL local (credenciales de desarrollo de `docker-compose.yml`, no secretas) y una clave de firma de JWT que vive solo en User Secrets o en `Jwt__SigningKey`: nunca la escribas en un archivo del repositorio. Supabase no se usa. Trata como sensible por defecto cualquier otro secreto que aparezca.
- No hay `.env.example` ni equivalente que mantener actualizado por ahora; cuando se conecte un backend/BD real, esto habrá que revisarlo y documentarlo.

## Referencias
- Arquitectura y dominio: `docs/arquitectura-app-coordinacion-escolar.md`
- Diseño de pantalla inicial: `docs/diseno-pantalla-inicial.md`
- Investigación de requisitos: `docs/investigacion-requisitos-sumiclas.md`
- Glosario de dominio español→inglés (para nombrar código): `docs/glosario-dominio.md`
