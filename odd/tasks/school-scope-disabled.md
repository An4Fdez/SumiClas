# Publicar a nivel de colegio queda cerrado hasta tener los roles de admin y de profesor

## Objetivo
Que nadie pueda publicar avisos, eventos del calendario ni objetos perdidos con ámbito colegio en esta versión, y dejarlo todo preparado y documentado para abrirlo cuando existan los roles de admin del colegio y de profesor.

## Problema
El §3 del documento de arquitectura reserva la publicación "oficial" a nivel colegio al admin del colegio. Hoy basta con ser delegado de cualquier clase, y cualquiera puede crearse una clase y ser delegado (alta abierta, §7.1). Así cualquier usuario con sesión puede publicar a todas las familias de un colegio. No existe el rol de admin del colegio y asignarlo bien exige una verificación que la Beta no tiene.

## Decisiones (usuario, 2026-10-23)
- Se cierra el ámbito colegio en los tres módulos que lo tienen: avisos, eventos del calendario y objetos perdidos. Quedan solo los de clase hasta la versión con los roles de admin y de profesor.
- Se mantiene el modelo: `AnnouncementScope.School`, la columna `SchoolId` ya migrada y las lecturas no se tocan. Lo que se cierra es la política de quién puede publicar.
- El alta abierta de cursos y clases se queda como pide el §7.1; con este cierre su riesgo se limita a la clase del propio delegado.

## Diseño
- Una exigencia entre módulos `UserSchoolPublishEnsure` (`Users/Application`, `IQuery<Unit>`, como `UserClassRepresentativeEnsure`), que hoy siempre lanza `UserExceptions.SchoolPublishForbidden` (403 por la convención de sufijos). Es el punto exacto donde se comprobará el rol de admin cuando exista.
- Los tres casos de uso la envían por el bus justo después de resolver el colegio, antes de crear el agregado de ámbito colegio. El código de ese camino sigue vivo y probado.
- El mensaje explica que publicar a nivel de colegio llegará en una versión posterior con los roles de admin y de profesor.

## Tareas
- [x] T1 Backend: `UserSchoolPublishEnsure` + excepción, los tres casos de uso, tests (incluidos los del camino colegio, que ahora preparan la exigencia), barrido de mutaciones.
- [ ] T2 Frontend: ocultar el ámbito colegio en los formularios de avisos, eventos y objetos perdidos; `pnpm typecheck` y `pnpm lint`.
- [ ] T3 Documentación: `apps/backend/AGENTS.md`, `apps/frontend/AGENTS.md`, nota en el §4 de `docs/arquitectura-app-coordinacion-escolar.md` y esta ficha, con el porqué y qué hay que hacer para abrirlo.

## Criterios de aceptación
- Build limpio y todos los tests en verde con tres semillas; ningún `var` (regla del backend).
- Publicar con ámbito colegio da 403 con el `code` `User.SchoolPublishForbidden` en los tres casos de uso, sin guardar nada.
- Revisión nativa por pieza y un commit por pieza.

## Progreso y evidencia
- T1 (sin commitear): `UserSchoolPublishEnsure` (`Users/Application`, sin dependencias: devuelve una tarea fallida con `UserExceptions.SchoolPublishForbidden`, código `User.SchoolPublishForbidden`, 403 por la convención de sufijos; no hace falta tocar ningún endpoint) y su envío por el bus en `AnnouncementCreate`, `CalendarEventCreate` y `LostItemCreate`, justo después de resolver el colegio. Los tests del camino colegio ahora preparan la exigencia (id concreto en el principal y cualquiera en los periféricos) y cada módulo gana tres: prohibido sin guardar nada, pregunta por el autor y el colegio correctos antes de guardar, y no pregunta si el ámbito es clase. Tests: el caso de uso nuevo y su mother. 869 tests en verde (13 nuevos). A raíz de un aviso de la revisión, un test resuelve la consulta a través del registro real de la aplicación (`AddSumiClasApplication` y `ISender`) para probar el cableado, y el 403 de la excepción queda fijado en el test de convención de estados. Barrido: 15 mutaciones detectadas (quitar la llamada, cambiar colegio o usuario, preguntar antes de tener el colegio, dejar pasar, quitar la excepción del manejador y alterar el código). Ningún `var`.

## Siguiente paso
T2 (frontend) y T3 (documentación).
