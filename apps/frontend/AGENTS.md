# AGENTS.md — Frontend (SumiClas)

Aplica a `apps/frontend/sumiclas-web/**`. Ver también el `AGENTS.md` de la raíz (convenciones comunes de idioma, commits, ramas y qué no tocar sin permiso).

## Cómo trabajar en el frontend

**El detalle de cada tema vive en skills** (`.claude/skills/<nombre>/SKILL.md`), para no cargar de golpe todo lo que no hace falta: antes de escribir o cambiar código, mira la tabla de abajo y **lee las skills que apliquen a la tarea** (a menudo son varias: una pantalla nueva conectada a la API pide `frontend-architecture`, `frontend-api-integration`, `frontend-ui-patterns` y `frontend-screens`). Si algo del código no coincide con lo que dice una skill, coméntalo antes de asumir cuál de los dos manda.

## Stack (decisión definitiva, no una desviación temporal)

Vite + React 18 + TypeScript + Tailwind CSS v3.4 + React Router v7. El documento de arquitectura (`docs/arquitectura-app-coordinacion-escolar.md`) recomendaba React + Next.js como PWA; **esa recomendación queda descartada para el frontend real**: Vite + React es el stack oficial, confirmado con el usuario. No propongas migrar a Next.js, añadir SSR ni configurar PWA/service worker por iniciativa propia.

## Idioma del código (cambio reciente, aplica retroactivamente)

A partir de esta decisión, **todo el código va en inglés**: nombres de archivo, componentes, carpetas, variables, tipos, props, comentarios. Esto aplica también a lo ya construido — el código existente se está renombrando/traduciendo, no solo el nuevo. Lo único que sigue en español es: el texto que ve el usuario final de la app (labels, botones, mensajes en la UI, porque los usuarios reales son hispanohablantes), los mensajes de commit, y los `AGENTS.md`/`README`.

Para traducir términos de negocio de forma consistente (y coherente con el backend), usa el glosario en `docs/glosario-dominio.md`. Si aparece un término nuevo que no está ahí, tradúcelo con el mismo criterio y añádelo a ese archivo, no lo dupliques aquí.

## Skills del frontend

| Si vas a... | Lee la skill |
| --- | --- |
| crear archivos, módulos, hooks o componentes, o dudar de dónde va algo; nombres y estilo | `frontend-architecture` |
| conectar una pantalla a la API, regenerar el cliente NSwag o crear un repositorio | `frontend-api-integration` |
| tocar el login, el registro, el logout, el refresco de sesión o las rutas privadas | `frontend-auth-session` |
| crear o cambiar un componente, un modal, un selector, un filtro o una pantalla | `frontend-ui-patterns` |
| tocar una pantalla concreta (onboarding, avisos, objetos perdidos, calendario, deberes, gastos, dashboard, resumen semanal, rol en la clase) | `frontend-screens` |

## Reglas que más importan (el detalle está en las skills)

- Nada de `fetch` ni `axios` a mano contra la API: se usa el cliente NSwag generado, y no se edita `GeneratedApiClient.ts` (`frontend-api-integration`).
- La capa de datos vive en `modules/<module>/` con `domain/` e `infrastructure/`, sin capa `application/`, y se inyecta con un contexto de React (`frontend-architecture`).
- Nunca `<select>` ni `<input type="date">` nativos, y todo componente nuevo lleva también sus variantes `dark:` (`frontend-ui-patterns`).
- Privacidad de menores: solo la inicial del apellido y los cumpleaños sin año (`frontend-ui-patterns`).
- El access token solo vive en memoria (`frontend-auth-session`).

## Testing

No hay tests de frontend todavía. Cuando se creen, viven en **`apps/test/frontend/`** (no co-localizados junto al código en `sumiclas-web`), con **Vitest + React Testing Library**.

## Verificación antes de terminar una tarea

Si tocaste `apps/frontend/sumiclas-web/**`, antes de dar la tarea por terminada:
```bash
cd apps/frontend/sumiclas-web
pnpm typecheck   # tsc --noEmit
pnpm lint        # eslint .
```
Ambos deben pasar sin errores. Si la tarea es una feature de UI y hay forma de probarla en el navegador (Claude in Chrome u otro), verifica también visualmente el camino feliz y los casos límite antes de reportar como terminado.

## Pendiente de decidir (no asumas nada aquí sin preguntar)

- Versionado del cliente NSwag generado: hoy comiteado y regenerado con `pnpm generate:client`; ¿se mantiene o pasa a generarse en build?
- Gestión de estados de carga/error de las llamadas a los repositorios: hoy a mano con `useState`; valorar una utilidad común cuando haya más pantallas migradas.
