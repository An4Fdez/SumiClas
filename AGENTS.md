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
    src/        → SumiClas (librería de dominio/aplicación/infraestructura, net10.0)
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

### Arrancar en desarrollo
**Flujo habitual del usuario (IDE):** abrir `SumiClas.slnx` en Visual Studio/Rider y ejecutar el proyecto `SumiClas.Api`. El `SpaProxy` configurado en el `.csproj` arranca automáticamente `pnpm run dev` en el frontend.

**Equivalente en terminal (el que debe usar el agente, que no tiene IDE):**
```bash
# Backend (desde la raíz del repo)
dotnet run --project apps/backend/api/SumiClas.Api.csproj

# Frontend (en otra terminal)
cd apps/frontend/sumiclas-web
pnpm install   # solo la primera vez / tras cambios en dependencias
pnpm dev
```
- Frontend (Vite): `https://localhost:4004`.
- Backend (perfil `https` en `launchSettings.json`): `https://localhost:7064`.
- El proxy de Vite reenvía `/api/` a `http://localhost:4000` — este puerto no coincide con el perfil `https` de `launchSettings.json`; si tocas la integración frontend↔backend, confírmalo con el usuario antes de asumir cuál es el puerto correcto.

## Convenciones comunes

- **Idioma**: español en todo — identificadores, comentarios (cuando hagan falta), documentación y mensajes de commit. Esto es intencional y coincide con el código y el historial existentes; no traduzcas a inglés por iniciativa propia.
- **Comentarios en código**: por defecto, no escribas comentarios (ver preferencia general de estilo). Si alguna vez son necesarios para explicar un porqué no obvio, en español.
- **Commits**: frase descriptiva en español, sin prefijos tipo Conventional Commits (`feat:`, `fix:`...). Ejemplos reales del historial: "Realización de la pantalla de gastos", "Cambio en los componentes de fecha y hora para adaptar al modo elegido (claro/oscuro)". Un commit, un cambio coherente.
- **Ramas**: se trabaja commiteando directo a `main` (no hay ramas de feature ni PRs en este proyecto). No crees ramas por tu cuenta salvo que el usuario lo pida explícitamente para una tarea concreta.
- **Estilo/formato**:
  - Frontend: ESLint + `typescript-eslint` (`apps/frontend/sumiclas-web/eslint.config.js`) es la única verificación de estilo configurada. No hay Prettier.
  - Backend: no hay `dotnet format` ni analizadores de estilo configurados todavía. Sigue el estilo ya presente en el código (carpetas `Shared/Aplicacion`, `Shared/Dominio`, `Shared/Infraestructura` en español).
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
