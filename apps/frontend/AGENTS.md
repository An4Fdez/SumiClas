# AGENTS.md — Frontend (SumiClas)

Aplica a `apps/frontend/sumiclas-web/**`. Ver también el `AGENTS.md` de la raíz (convenciones comunes de idioma, commits, ramas y qué no tocar sin permiso).

## Stack (decisión definitiva, no una desviación temporal)

Vite + React 18 + TypeScript + Tailwind CSS v3.4 + React Router v7. El documento de arquitectura (`docs/arquitectura-app-coordinacion-escolar.md`) recomendaba React + Next.js como PWA; **esa recomendación queda descartada para el frontend real**: Vite + React es el stack oficial, confirmado con el usuario. No propongas migrar a Next.js, añadir SSR ni configurar PWA/service worker por iniciativa propia.

Hoy no hay backend conectado: todos los datos vienen de mocks en `src/data/*.ts`. Ver la sección "Datos y backend real" más abajo para cómo se conectará.

## Idioma del código (cambio reciente, aplica retroactivamente)

A partir de esta decisión, **todo el código va en inglés**: nombres de archivo, componentes, carpetas, variables, tipos, props, comentarios. Esto aplica también a lo ya construido — el código existente se está renombrando/traduciendo, no solo el nuevo. Lo único que sigue en español es: el texto que ve el usuario final de la app (labels, botones, mensajes en la UI, porque los usuarios reales son hispanohablantes), los mensajes de commit, y los `AGENTS.md`/`README`.

Para traducir términos de negocio de forma consistente (y coherente con el backend), usa el glosario en `docs/glosario-dominio.md`. Si aparece un término nuevo que no está ahí, tradúcelo con el mismo criterio y añádelo a ese archivo, no lo dupliques aquí.

## Estructura de `src/`

```
src/
  components/
    app/          → componentes de UI de una pantalla/feature concreta (formularios, modales, tarjetas, desplegables). Se queda plano, sin subcarpetas por módulo.
    layout/        → AppLayout, Navbar, Reveal — armazón de la aplicación
    landing/        → secciones de la página pública (Home)
    onboarding/       → pasos del flujo de alta (colegio/clase/alumno)
  context/         → Contextos de React (ej. ChildContext.ts + ChildProvider.tsx: split definición/provider)
  data/           → datos mock + tipos (se sustituye módulo a módulo por modules/<module>/, ver más abajo)
  hooks/           → hooks reutilizables (useTheme, useChildContext...)
  lib/            → utilidades sin estado (date.ts, auth.ts, roles.ts, colorClasses.ts)
  pages/           → componentes de página, uno por ruta de React Router (Announcements.tsx, Calendar.tsx, Homework.tsx, Expenses.tsx, LostAndFound.tsx...)
  modules/
    <module>/      → capa de datos de un módulo (announcements, calendar, homework, expenses, lostAndFound...), sin UI
      domain/       → interfaz/contrato del repositorio + tipos de dominio del módulo
      infrastructure/ → implementación concreta del repositorio (usa el cliente compartido de shared/infrastructure/sumiclas-client)
  shared/
    infrastructure/
      sumiclas-client/  → cliente HTTP generado con NSwag Studio a partir del OpenAPI del backend (compartido por todos los módulos, ver más abajo)
```

`components/app/` no se reorganiza por módulo: la UI (formularios, tarjetas, modales) se queda donde está hoy. Solo la capa de datos (sin UI) vive en `modules/<module>/`, como carpeta de primer nivel — no anidada dentro de `components/`, porque `Domain`/`Infrastructure` no son componentes React. Esto además mantiene el frontend consistente con la estructura del backend (`SumiClas/<Module>/{Domain,Application,Infrastructure}` también como carpetas de primer nivel).

**Sin capa de Application en el frontend** (a diferencia del backend): aquí no hay lógica de negocio que justifique una capa de aplicación propia. El reparto de responsabilidades es:
- **UI (`components/app/`, `pages/`)**: comprobaciones ligeras orientadas a experiencia de usuario (campos obligatorios, formato, etc.) directamente en el formulario/pantalla — no hace falta una capa intermedia para esto.
- **Backend (`Domain` de cada módulo)**: las comprobaciones de negocio que de verdad importan (autoritativas) — nunca confíes solo en la validación del frontend.

No añadas una carpeta `application/` en `modules/<module>/` sin que se pida explícitamente; no es un olvido, es la arquitectura acordada.

## Datos y backend real

- **Hoy**: todo son mocks en `src/data/*.ts`, sin llamadas de red.
- **Cliente HTTP**: generado por **NSwag Studio** a partir del OpenAPI del backend (ya expuesto por `Program.cs`), vive en `src/shared/infrastructure/sumiclas-client` y es compartido por todos los módulos. No escribas `fetch`/`axios` a mano contra los endpoints — usa el cliente NSwag.
- **Patrón repositorio**, en `src/modules/<module>/`:
  - `domain/` — interfaz/contrato del repositorio (ej. `AnnouncementRepository`) + tipos de dominio propios del módulo. No depende de `infrastructure` ni del cliente NSwag.
  - `infrastructure/` — implementación concreta del repositorio (ej. `AnnouncementRepositoryHttp`) que usa el cliente compartido de `shared/infrastructure/sumiclas-client` para hacer las llamadas reales.
  - No se usa TanStack Query ni ninguna librería de caché de peticiones para esto.
- **Inyección de la implementación**: mediante un Contexto de React (mismo patrón que `ChildContext`/`ChildProvider`), no como singleton importado directamente ni instanciado a mano en el componente. Esto permite sustituir la implementación por un mock en los tests de `apps/test/frontend`. Componentes y hooks (en `components/app/`) consumen el repositorio a través de un hook propio (p. ej. `useAnnouncementRepository()`), nunca instanciando `AnnouncementRepositoryHttp` directamente.
- Los mocks actuales de `src/data/*.ts` se sustituyen por `modules/<module>/` **módulo a módulo**, no todos a la vez.
- **Pendiente de decidir**: si el archivo del cliente NSwag generado se comitea en git (regenerado a mano con NSwag Studio cuando cambia el contrato) o se regenera en cada build; y cómo se gestionan los estados de carga/error de cada llamada al repositorio (a mano con `useState`, alguna utilidad propia, u otra cosa). No asumas ninguna de las dos cosas sin confirmarlo primero.

## Patrones de UI ya establecidos (síguelos, no los reinventes)

- **Modales**: `fixed inset-0 z-50 flex items-end sm:items-center justify-center bg-slate-900/40 backdrop-blur-sm` + tarjeta `rounded-t-3xl sm:rounded-3xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-xl`. Usa el truco del ref `mouseDownOnBackdrop` (solo cierra si el `mousedown` **y** el `click` caen sobre el propio fondo) para que seleccionar texto y arrastrar fuera del modal no lo cierre.
- **Desplegables/selectores personalizados**: nunca uses `<select>` nativo ni `<input type="date"/"time">` nativos — su popup no se puede estilar (bordes cuadrados, no respeta el modo oscuro). Usa/reutiliza `SelectDropdown`, `DatePicker` y `TimePicker` (`src/components/app/`), que renderizan su panel vía `createPortal` a `document.body` para no quedar recortados por el `overflow-y-auto` de un modal contenedor.
- **Filtros de página** (no dentro de modal): trigger tipo pastilla (`rounded-full`) + panel `absolute` con `onBlur` que cierra si el foco sale del contenedor (no necesitan portal al no estar dentro de un modal con scroll).
- **Estado local "activado/desactivado" sobre datos base** (leído/no leído, hecho, recuperado...): un `Set<string>` de ids "conmutados" que se aplica en XOR sobre el valor base, no dupliques el dato completo en estado local.
- **Permisos por rol**: `useChildContext()` expone `currentRole`, `children`, `currentChild`, `otherGuardians`, `setCurrentChildId`. Compara `currentRole === 'classRepresentative'` para contenido oficial vs `'participant'` para propuestas/comentarios.
- **Modo oscuro**: `useTheme` alterna la clase `dark` en `document.documentElement` (`darkMode: 'class'` en `tailwind.config.js`). Todo componente nuevo necesita sus variantes `dark:` a la par que las claras, nunca solo un tema.
- **Privacidad de menores**: nunca mostrar apellido completo (solo inicial), nunca pedir/guardar año de nacimiento (cumpleaños solo día+mes), fotos de objetos perdidos con checkbox de confirmación explícita antes de publicar.

## Nomenclatura

- Inglés en todo el código: nombres de componentes, archivos, props, hooks propios y variables (`AnnouncementForm`, `eventsForDay`, `className`, `useChildContext`). Ver `docs/glosario-dominio.md` para traducir términos de negocio de forma consistente.
- Un componente por archivo, nombre de archivo = nombre del componente en PascalCase.
- Tipos/props inline en la firma del componente (`{ prop1, prop2 }: { prop1: X; prop2: Y }`), no interfaces `Props` separadas salvo que el tipo se reutilice en más de un sitio.

## Estilo

- Tailwind utility-first en todo; no hay CSS Modules ni styled-components. No introduzcas ninguno de los dos sin acordarlo antes.
- Iconos: `lucide-react` exclusivamente.
- Sin librería de i18n: los **textos de la UI** van en español directamente en el JSX (no hay previsto soporte multi-idioma) — esto no cambia con la nueva convención de idioma del código, que solo afecta a identificadores/archivos, no al contenido renderizado.
- Alias de rutas: importa con `@/...` (equivale a `src/...`), no uses rutas relativas largas (`../../../`).

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

- Versionado del cliente NSwag generado: ¿comiteado y regenerado a mano, o generado en build?
- Gestión de estados de carga/error de las llamadas a los repositorios (a mano con `useState`, utilidad propia, u otra cosa).
- Nomenclatura exacta de interfaz vs. implementación del repositorio (se usa `AnnouncementRepository` / `AnnouncementRepositoryHttp` como ejemplo ilustrativo, no como convención ya fijada).
