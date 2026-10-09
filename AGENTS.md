# AGENTS.md — SumiClas (raíz del monorepo)

Este archivo aplica a todo el repositorio. Existen (o existirán) archivos `AGENTS.md` más específicos en:
- `apps/backend/AGENTS.md`
- `apps/frontend/AGENTS.md`
- `apps/test/AGENTS.md` (pendiente: `apps/test` está vacía por ahora; contendrá `apps/test/backend` y `apps/test/frontend`)

Si un archivo más específico entra en conflicto con este, gana el más específico para esa carpeta.

## Qué es este proyecto

SumiClas es una app de coordinación escolar (sustituto del grupo de WhatsApp de clase) en fase de TFM/Beta. Contexto de producto, roles, modelo de datos y reglas de privacidad de menores: ver `docs/arquitectura-app-coordinacion-escolar.md`. Hay también `docs/diseno-pantalla-inicial.md` y `docs/investigacion-requisitos-sumiclas.md`.

**Nota sobre el documento de arquitectura**: recomienda React + Next.js (PWA) sobre un backend .NET 10 Minimal API + PostgreSQL. Para el backend esa recomendación se sigue (.NET 10 + EF Core/PostgreSQL, ver `apps/backend/AGENTS.md`). Para el frontend, en cambio, **Next.js/PWA queda descartado de forma definitiva**: el stack oficial es Vite + React (ver `apps/frontend/AGENTS.md`). No propongas SSR, PWA, service worker ni migración a Next.js. El backend todavía no está conectado al frontend (Supabase está instalado como dependencia pero sin usar).

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
- `docker-compose.yml` levanta solo PostgreSQL (`postgres:17-alpine`, puerto `5432`, credenciales de desarrollo en claro en el propio archivo — no son secretos reales, ver `apps/backend/AGENTS.md`). Esto es infraestructura de **desarrollo local únicamente**; el despliegue/hosting real (producción) sigue sin decidir (ver "Pendiente de decidir" en `apps/backend/AGENTS.md`). El backend todavía no tiene `DbContext` ni cadena de conexión configurada — este contenedor deja la BBDD lista para cuando se construya esa pieza, pero `dotnet run` hoy no la usa todavía.
- Frontend (Vite): `https://localhost:4004`.
- Backend (perfil `https` en `launchSettings.json`): `https://localhost:7064`.
- El proxy de Vite reenvía `/api/` a `http://localhost:4000` — este puerto no coincide con el perfil `https` de `launchSettings.json`; si tocas la integración frontend↔backend, confírmalo con el usuario antes de asumir cuál es el puerto correcto.

## Convenciones comunes

- **Idioma**: desde esta decisión en adelante, **el código va en inglés** — nombres de archivo, carpetas, componentes, clases, variables, tipos, casos de uso, y comentarios de código (cuando hagan falta). En **español** se quedan: el `README`, todos los `AGENTS.md` (este incluido), los mensajes de commit, y la documentación de `docs/`. Esto es una reversión deliberada de la convención anterior ("español en todo"); no la reintroduzcas por iniciativa propia. Ver el glosario de dominio español→inglés en `apps/backend/AGENTS.md` y `apps/frontend/AGENTS.md`.
- **Texto visible para el usuario final de la app** (botones, títulos, mensajes en la UI): sigue en **español**, porque los usuarios reales (familias/profesores) son hispanohablantes. Solo el código cambia de idioma, no el contenido que se renderiza en pantalla.
- **Comentarios en código**: por defecto, no escribas comentarios (ver preferencia general de estilo). Si alguna vez son necesarios para explicar un porqué no obvio, en inglés (a partir de ahora).
- **Commits**: frase descriptiva en español, sin prefijos tipo Conventional Commits (`feat:`, `fix:`...). Ejemplos reales del historial: "Realización de la pantalla de gastos", "Cambio en los componentes de fecha y hora para adaptar al modo elegido (claro/oscuro)". Un commit, un cambio coherente. **Nunca** se añade `Co-Authored-By` ni ninguna otra atribución de IA ("Generated with...", firmas de asistentes) en commits, PRs, código ni documentación, aunque la herramienta lo sugiera por defecto; si un subagente va a commitear, díselo expresamente.
- **Ramas**: se trabaja commiteando directo a `main` (no hay ramas de feature ni PRs en este proyecto). No crees ramas por tu cuenta salvo que el usuario lo pida explícitamente para una tarea concreta.
- **Estilo/formato**:
  - Frontend: ESLint + `typescript-eslint` (`apps/frontend/sumiclas-web/eslint.config.js`) es la única verificación de estilo configurada. No hay Prettier.
  - Backend: no hay `dotnet format` ni analizadores de estilo configurados todavía. Carpetas de capa en inglés (`shared/application`, `shared/domain`, `shared/infrastructure`).
  - No hay hooks de pre-commit (ni Husky ni equivalente) en este repo.

## Reglas generales

**Antes de dar por terminada una tarea:**
- Si tocaste el frontend (`apps/frontend/sumiclas-web`): ejecuta `pnpm typecheck` y `pnpm lint` (o `npm run typecheck` / `npm run lint`, equivalentes mientras exista el lockfile de npm) y que pasen sin errores.
- Si tocaste el backend (`apps/backend/**`): ejecuta `dotnet build` sobre la solución o el proyecto afectado y que compile sin errores.
- Cuando exista contenido en `apps/test/backend` o `apps/test/frontend`, añadir aquí la ejecución de esos tests como paso obligatorio antes de terminar (según la zona tocada).

**No toques sin permiso explícito:**
- La estructura de la solución: `SumiClas.slnx`, los `.csproj` (`SumiClas.Api.csproj`, `SumiClas.csproj`) y el `.esproj` del frontend. Si una tarea requiere añadir un proyecto nuevo o cambiar referencias, confírmalo antes con el usuario.
- Secretos y configuración local: `appsettings.*.Local.json`, `.env` y cualquier archivo que `.gitignore` ya marque como local/sensible. Ahora mismo el proyecto usa datos mock y no hay conexión real a base de datos ni a Supabase, así que no debería haber secretos reales todavía — si aparecen, trátalos como sensibles por defecto.
- No hay `.env.example` ni equivalente que mantener actualizado por ahora; cuando se conecte un backend/BD real, esto habrá que revisarlo y documentarlo.

## Referencias
- Arquitectura y dominio: `docs/arquitectura-app-coordinacion-escolar.md`
- Diseño de pantalla inicial: `docs/diseno-pantalla-inicial.md`
- Investigación de requisitos: `docs/investigacion-requisitos-sumiclas.md`
- Glosario de dominio español→inglés (para nombrar código): `docs/glosario-dominio.md`
