---
name: frontend-architecture
description: "Estructura de src/ del frontend (components, context, hooks, lib, pages, modules), capa de datos por módulo sin capa Application, nomenclatura y estilo (Tailwind, lucide-react, alias @). Úsala al crear archivos, módulos, hooks o componentes, o al dudar de dónde va algo."
---

# Arquitectura del frontend: estructura, nomenclatura y estilo

Aplica a `apps/frontend/sumiclas-web/**`. Las reglas obligatorias, la tabla de skills y lo pendiente de decidir están en `apps/frontend/AGENTS.md`; las convenciones comunes de idioma, commits y ramas, en el `AGENTS.md` de la raíz.

## Estructura de `src/`

```
src/
  components/
    app/          → componentes de UI de una pantalla/feature concreta (formularios, modales, tarjetas, desplegables). Se queda plano, sin subcarpetas por módulo.
    layout/        → AppLayout, Navbar, Reveal — armazón de la aplicación
    landing/        → secciones de la página pública (Home)
    onboarding/       → pasos del flujo de alta (colegio/clase/alumno)
  context/         → Contextos de React (ej. ChildContext.ts + ChildProvider.tsx: split definición/provider)
  data/           → solo `featureMeta.ts`, el icono y el color de cada función de la página pública (landing). Los datos mock de las pantallas ya se sustituyeron por `modules/<module>/` (ver la skill `frontend-api-integration`); no añadas datos de pantalla aquí
  hooks/           → hooks reutilizables (useTheme, useChildContext...)
  lib/            → utilidades sin estado (date.ts, auth.ts, roles.ts, colorClasses.ts)
  pages/           → componentes de página, uno por ruta de React Router (Announcements.tsx, Calendar.tsx, Homework.tsx, Expenses.tsx, LostAndFound.tsx...)
  modules/
    <module>/      → capa de datos de un módulo (announcements, calendar, homework, expenses, lostAndFound...), sin UI
      domain/       → un fichero por tipo (nombre del fichero = nombre del tipo): el contrato `I<Entidad>Repository.ts` (solo la interfaz) y sus tipos de dominio (`LostItem.ts`, `NewLostItem.ts`...)
      infrastructure/ → implementación concreta del repositorio (usa el cliente compartido de modules/Shared/Infrastructure)
  modules/Shared/domain/  → tipos de dominio compartidos entre módulos (p. ej. `PageRequest.ts`)
  modules/Shared/Infrastructure/  → cliente HTTP generado con NSwag (GeneratedApiClient.ts, configuración .nswag y ClientBase.ts) más el plumbing propio (ApiClient.ts, apiDate.ts); compartido por todos los módulos, ver la skill `frontend-api-integration`
```

`components/app/` no se reorganiza por módulo: la UI (formularios, tarjetas, modales) se queda donde está hoy. Solo la capa de datos (sin UI) vive en `modules/<module>/`, como carpeta de primer nivel — no anidada dentro de `components/`, porque `Domain`/`Infrastructure` no son componentes React. Esto además mantiene el frontend consistente con la estructura del backend (`SumiClas/<Module>/{Domain,Application,Infrastructure}` también como carpetas de primer nivel).

**Sin capa de Application en el frontend** (a diferencia del backend): aquí no hay lógica de negocio que justifique una capa de aplicación propia. El reparto de responsabilidades es:
- **UI (`components/app/`, `pages/`)**: comprobaciones ligeras orientadas a experiencia de usuario (campos obligatorios, formato, etc.) directamente en el formulario/pantalla — no hace falta una capa intermedia para esto.
- **Backend (`Domain` de cada módulo)**: las comprobaciones de negocio que de verdad importan (autoritativas) — nunca confíes solo en la validación del frontend.

No añadas una carpeta `application/` en `modules/<module>/` sin que se pida explícitamente; no es un olvido, es la arquitectura acordada.

## Nomenclatura

- Inglés en todo el código: nombres de componentes, archivos, props, hooks propios y variables (`AnnouncementForm`, `eventsForDay`, `className`, `useChildContext`). Ver `docs/glosario-dominio.md` para traducir términos de negocio de forma consistente.
- Un componente por archivo, nombre de archivo = nombre del componente en PascalCase.
- Las interfaces (contratos/puertos, p. ej. los repositorios en `domain/`) llevan prefijo `I` (`IUserRepository`) y el fichero se llama igual (`domain/IUserRepository.ts`); su implementación contra la API se llama `NSwag<Entidad>Repository` (`NSwagUserRepository`, en `infrastructure/NSwagUserRepository.ts`) porque usa el cliente generado por NSwag; si algún día otra implementación usara otra fuente, llevaría su propio prefijo. Los tipos de datos (`Child`, `Profile`) no llevan prefijo y van **un fichero por tipo en `domain/`** (nombre del fichero = nombre del tipo, p. ej. `domain/Profile.ts`): el contrato `I<Entidad>Repository.ts` contiene solo la interfaz, y los tipos compartidos entre módulos (p. ej. `PageRequest`) viven en `modules/Shared/domain/`.
- Tipos/props inline en la firma del componente (`{ prop1, prop2 }: { prop1: X; prop2: Y }`), no interfaces `Props` separadas salvo que el tipo se reutilice en más de un sitio.

## Estilo

- Tailwind utility-first en todo; no hay CSS Modules ni styled-components. No introduzcas ninguno de los dos sin acordarlo antes.
- Iconos: `lucide-react` exclusivamente.
- Sin librería de i18n: los **textos de la UI** van en español directamente en el JSX (no hay previsto soporte multi-idioma) — esto no cambia con la nueva convención de idioma del código, que solo afecta a identificadores/archivos, no al contenido renderizado.
- Alias de rutas: importa con `@/...` (equivale a `src/...`), no uses rutas relativas largas (`../../../`).
