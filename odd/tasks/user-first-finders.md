# El usuario actual primero también en los cuatro buscadores que quedaban

## Objetivo
Continúa `odd/tasks/user-first.md`. Quedaban cuatro buscadores que no piden `GetCurrentUserId()` lo primero: `AssignmentFinderByClass`, `ClassExpenseFinderByClass` y `LostItemFinderByClass` empiezan por `Pagination.Resolve(...)`, de modo que un anónimo con `limit` o `cursor` inválidos recibe el 400 de paginación en vez de «sin sesión»; y `SubjectFinderByClass` comprueba la clase antes del usuario sin ser una excepción documentada. El usuario pidió ponerlos también con el usuario primero.

## Alcance autorizado
`Assignments/Application/AssignmentFinderByClass.cs`, `ClassExpenses/Application/ClassExpenseFinderByClass.cs`, `LostItems/Application/LostItemFinderByClass.cs`, `Subjects/Application/SubjectFinderByClass.cs` (o el nombre real del buscador de asignaturas por clase), sus tests y las skills si cambian lo que describen. `AnnouncementFinderByClass` se comprueba por si tiene el mismo orden. Nada más.

## Tareas
- [x] V1 `AssignmentFinderByClass`: usuario antes de `Pagination.Resolve`.
- [x] V2 `ClassExpenseFinderByClass`: usuario antes de `Pagination.Resolve`.
- [x] V3 `LostItemFinderByClass`: usuario antes de `Pagination.Resolve`.
- [x] V4 `SubjectFinderByClass`: usuario antes de `ClassFinderById`.

## Criterios de aceptación
- Un anónimo recibe `Unauthenticated` también con `limit`/`cursor` inválidos (V1-V3) o con una clase inexistente (V4); un test por buscador lo prueba, visto en rojo primero.
- Mismo comportamiento para quien sí tiene sesión: mismos errores de paginación y de clase.
- Suite en verde, `var` vacío; `backend-read-queries`, `backend-authorization` y `backend-cross-module` dicen lo mismo que el código.

## Ruta
Un escritor delegado hace V1–V4 con RED primero. Commit tras el «sí» del usuario.

## Progreso y evidencia
- V1-V4: `AssignmentFinderByClass`, `ClassExpenseFinderByClass`, `LostItemFinderByClass` y `SubjectFinderByClass` piden `GetCurrentUserId()` lo primero (V4 no era una excepción documentada); `AnnouncementFinderByClass` ya cumplía. Un test nuevo por buscador visto en rojo por el motivo esperado (`InvalidLimit` en V1-V3, `ClassExceptions.NotFound` en V4) y luego en verde; mutación por buscador (volver al orden anterior) detectada por el test nuevo. Suite 1253 correctos (1249 + 4); `var` vacío. `backend-cross-module` y `backend-read-queries` actualizadas. Revisados todos los casos de uso de `*/Application`: solo quedan las seis excepciones documentadas en `backend-authorization`. Revisión nativa: no solicitada.

## Siguiente paso
Hecho.
