# Poner al día la documentación que ya no coincide con el código

## Objetivo
La auditoría del 2026-10-25 encontró skills y `AGENTS.md` que describen algo distinto de lo que hay en el código, y tres decisiones vigentes que no están escritas en ningún sitio. Se corrige la documentación (no el código) tras comprobar cada dato en el código.

## Alcance autorizado
Solo documentación: `.claude/skills/*/SKILL.md`, `AGENTS.md` (raíz, backend y frontend) y `docs/` si algún dato lo exige. Nada de código. Se documenta lo que el código hace hoy; no se cambia el comportamiento para que coincida con la documentación.

## Tareas
- [x] G1 `Pagination.Resolve`: `backend-read-queries` dice que lo llama el `Handler`, y los buscadores (`AnnouncementFinderByClass`, `AssignmentFinderByClass`, `ClassExpenseFinderByClass`, `LostItemFinderByClass`) lo llaman dentro de la acción. Se escribe lo que hay.
- [x] G2 Cifras y descripciones antiguas: «5 tablas hijas», «17 archivos», «12 agregados» (`backend-persistence`, `backend-architecture`), la carpeta `data/` de mocks que `frontend-architecture` describe y casi ya no existe, y la frase del `AGENTS.md` raíz que dice que `apps/test` está vacía y que `apps/test/AGENTS.md` está pendiente (hay más de mil tests en `apps/test/backend`).
- [x] G3 Tres cosas vigentes sin documentar: el módulo `Dashboard` (solo `Application`, compone los demás) en el mapa de módulos; el `Guid.NewGuid()` de `WeeklySummaryGenerate` como excepción al «el id lo aporta el cliente» (no hay id en el `POST`); y el `fetch` directo a pwnedpasswords de `lib/validation.ts` como excepción a «nada de `fetch` a mano» (no es la API de SumiClas, usa k-anonimato y falla en abierto).

## Criterios de aceptación
- Cada dato corregido se ha comprobado en el código (conteo, grep o lectura) y la skill lo dice igual.
- Ningún texto movido o reescrito pierde información que siga siendo cierta.
- Las skills conservan su `name` y su `description` válidos entre comillas dobles, y las referencias cruzadas siguen apuntando a algo que existe.

## Ruta
Un escritor delegado hace G1, G2 y G3 (solo documentación, varios ficheros). Sin tests: la comprobación es estructural (grep de las frases antiguas, lectura de los cambios, cabeceras de las skills). Commit tras el «sí» del usuario.

## Progreso y evidencia
- G1: `backend-read-queries` dice que `Pagination.Resolve` lo llama la acción `Finder` de los cuatro buscadores paginados y que el `Handler` solo delega. G2: `backend-persistence` y `backend-architecture` sin cifras fijas (un mapeo por tabla; 17 `DbSet`, 17 mapeos de agregado y 7 de tabla hija comprobados), `frontend-architecture` y `frontend-api-integration` con `src/data` reducido a `featureMeta.ts`, y el `AGENTS.md` raíz con la estructura real (`apps/test/backend` con tests, sin `apps/test/frontend` ni `apps/test/AGENTS.md`, con `tools/`) y `dotnet test` como paso al tocar el backend. G3: `Dashboard` en el mapa de módulos como excepción (solo `Application`), `Guid.NewGuid()` de `WeeklySummaryGenerate` en `backend-domain` y el `fetch` de pwnedpasswords en `frontend-api-integration`. Comprobado: solo cambian `.md`, frontmatter válido en las 20 skills, ninguna frase antigua queda. No cambiado: `backend-domain` dice que faltan los casos de uso de asignar delegados y ya existen; `Shared/Infrastructure` frente a `domain` en mayúsculas. Revisión nativa: no solicitada.

## Siguiente paso
Hecho. Queda corregir la frase de `backend-domain` sobre la asignación de delegados.
