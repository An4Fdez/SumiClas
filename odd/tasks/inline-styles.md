# Quitar los estilos en línea del logo y de los menús flotantes

## Objetivo
El frontend se estiliza con Tailwind, y quedaban 6 usos de `style={{ }}` en JSX. El usuario pidió quitarlos; tras la propuesta decidió **solo los que no cambian nada visual**: el logo y los tres menús flotantes. El retardo de `Reveal` se queda como está (con su `style`), porque pasarlo a clases exige mover `.reveal` a `@layer components` y redondear los retardos, y el usuario no lo quiere ahora.

## Alcance autorizado
- `src/components/layout/Logo.tsx`: `letterSpacing` y `fontVariationSettings` pasan a clases de Tailwind (`tracking-[-0.03em]`, `tracking-[-0.02em]`, `[font-variation-settings:'SOFT'_50,'opsz'_100]`) y se quitan los `tracking-tight` que competían con el estilo en línea.
- Un único componente `FloatingPanel` (en `src/components/app/`) que centraliza la parte repetida de `DatePicker`, `TimePicker` y `SelectDropdown`: portal, posición respecto al botón, y escucha de scroll y resize. Coloca el panel con variables CSS (`--panel-top`, `--panel-left`, `--panel-width`) escritas con `style.setProperty` desde un `useLayoutEffect`, y la clase `fixed top-[var(--panel-top)] left-[var(--panel-left)] w-[var(--panel-width)]`. Los tres componentes lo usan y no queda ningún `style` en su JSX.
- La skill `frontend-ui-patterns` (y `frontend-architecture` si lista los componentes) se actualizan.
- Fuera: `Reveal.tsx` y `index.css` no se tocan; no se añade ninguna regla de ESLint.

## Tareas
- [x] E1 Capturas «antes» (código actual) del logo y de los tres menús abiertos, con Chrome sin cabeza.
- [x] E2 Logo con clases de Tailwind.
- [x] E3 `FloatingPanel` y los tres menús usándolo.
- [x] E4 Capturas «después», comparación píxel a píxel con las de antes, `pnpm typecheck` y `pnpm lint`, y skill actualizada.

## Criterios de aceptación
- `grep "style={{"` en `src` solo encuentra `Reveal.tsx`.
- Las capturas antes y después del logo y de los tres menús abiertos son idénticas (diferencia de píxeles nula o explicada) y los menús siguen abriéndose, colocándose junto a su botón, y reposicionándose al hacer scroll y al cambiar el tamaño de la ventana.
- `pnpm typecheck` y `pnpm lint` sin errores; ninguna clave real en ningún archivo.

## Ruta
Un escritor delegado hace E1–E4 (varios archivos). Sin tests automáticos en el frontend: la red de seguridad son las capturas antes y después y el typecheck. Commit tras el «sí» del usuario.

## Progreso y evidencia
- Logo con `tracking-[-0.03em]`, `tracking-[-0.02em]` y `[font-variation-settings:'SOFT'_50,'opsz'_100]` (sin los `tracking-tight` que competían). Nuevo `FloatingPanel` (portal, posición del botón escrita como variables CSS `--panel-top`, `--panel-left` y `--panel-width` con `style.setProperty` en un `useLayoutEffect`, cierre por clic fuera, scroll y resize) usado por `DatePicker` (mínimo 280), `TimePicker` (mínimo 180) y `SelectDropdown`; las APIs externas no cambian. `grep "style={{"` solo encuentra `Reveal.tsx`. Aclaración: los menús calculan la posición una sola vez al abrirse y se cierran al hacer scroll o resize, como ya hacían; no se recolocan. Comparación píxel a píxel antes y después: 40 parejas (logo en landing y app, fecha, hora y selector abiertos, con scroll y con resize a 1280x800, en modo oscuro y claro), 0 píxeles de diferencia; control negativo con cambios de 1 px detecta diferencia en 38 de 40; dos pasadas del código sin tocar dan 0. `pnpm typecheck` y `pnpm lint` sin errores. Skill `frontend-ui-patterns` actualizada. No se vio en el navegador real, solo en Chrome sin cabeza. Docker Desktop se arrancó para las capturas y se dejó en marcha. Revisión nativa: no solicitada.

## Siguiente paso
Hecho. `Reveal.tsx` mantiene su `style` por decisión del usuario.
