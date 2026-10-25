# Comentarios que sobran y restos pequeños de la auditoría

## Objetivo
La regla del proyecto es no escribir comentarios salvo un porqué no obvio (en inglés). La auditoría del 2026-10-25 encontró comentarios que solo describen lo que el código ya dice, y algunos restos pequeños. Se quitan los claros y se deja para decidir con el usuario lo que es cuestión de criterio.

## Alcance autorizado
- C1 `apps/backend/api/Program.cs`: comentarios de plantilla («Add services to the container.», el enlace de OpenAPI, «Configure the HTTP request pipeline.», el que describe lo que no hay) y líneas en blanco sobrantes. Se conservan los que explican un porqué.
- C2 Frontend: los comentarios JSX que solo describen el marcado (`components/landing/{Hero,Features,FinalCTA,Problem,SocialProof,SummarySection}.tsx`, `components/layout/Navbar.tsx` incluido el de 5 líneas, que además menciona `localStorage` cuando lo que lee es la info del tutor). Los JSDoc que explican un porqué (el `generation` de los hooks) se quedan.
- C3 Restos: la frase de `backend-domain` que dice que faltan los casos de uso de asignación de delegados (ya existen) y las líneas de más de 120 columnas que quedan en tests (`AnnouncementFinderByClassShould`, `ApplicationRegistrationShould`, `WeeklySummaryFingerprintShould`).
- Fuera de alcance, a decidir con el usuario: los ~551 comentarios `///` del backend, muchos de «qué» (por ejemplo en `Pagination` y `UnitOfWorkBehavior`).

## Tareas
- [x] C1 Program.cs.
- [x] C2 Comentarios JSX del frontend.
- [x] C3 Frase de `backend-domain` y líneas largas de tests.

## Criterios de aceptación
- Ningún comportamiento cambia: `dotnet build` y `dotnet test` en verde (1245), `pnpm typecheck` y `pnpm lint` sin errores, y el marcado renderizado del frontend es el mismo (solo desaparecen comentarios).
- Un comentario solo se borra si describe lo que el código ya dice; si explica un porqué, se queda.

## Ruta
Un escritor delegado hace C1, C2 y C3 en orden. Sin tests nuevos (son borrados de comentarios y reformateo): los existentes y el typecheck son la red de seguridad. Commit tras el «sí» del usuario.

## Progreso y evidencia
- C1: `Program.cs` sin los comentarios de plantilla ni 5 líneas en blanco; se conservan los tres que explican un porqué. C2: 23 comentarios JSX que solo describían el marcado quitados de la landing y `Navbar`, comprobado con un script que compara cada fichero con su versión anterior; el de 5 líneas de `Navbar` era cierto (`getGuardianInfo()` lee de `localStorage`) y queda en 2 líneas con el porqué. C3: `backend-domain` dice lo que existe y que faltan la edición y el borrado del contenido (no hay casos de uso `Update` ni `Delete`); las 4 líneas de más de 120 columnas de los tests resueltas. Suite 1245 correctos; `var` vacío; `pnpm typecheck` y `pnpm lint` sin errores. Fuera, a decidir: los unos 551 comentarios `///` del backend. Revisión nativa: no solicitada.

## Siguiente paso
Hecho. Queda a decidir con el usuario qué hacer con los `///` del backend.
