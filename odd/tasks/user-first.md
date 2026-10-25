# El usuario actual se pide primero en los casos de uso

## Objetivo
`backend-cross-module` dice que `IUserProvider.GetCurrentUserId()` se pide lo primero, antes de validar nada, para que quien no tiene sesión reciba siempre «sin sesión» y no un «no encontrado» o un error de validación. La auditoría del 2026-10-25 encontró cuatro casos de uso que no lo cumplen y no figuran entre las excepciones documentadas en `backend-authorization`. Se arreglan para que cumplan la regla.

## Alcance autorizado
Los cuatro casos de uso de la auditoría: `Enrollments/Application/EnrollmentCreate.cs`, `Enrollments/Application/EnrollmentConfirm.cs`, `SchoolYears/Application/SchoolYearArchive.cs` y `CalendarEvents/Application/CalendarEventFinderByClass.cs`, más sus tests y las skills si cambian lo que describen. Si el escritor encuentra otros casos de uso fuera de las excepciones documentadas, los lista en el informe sin tocarlos.

## Tareas
- [x] U1 `EnrollmentCreate`: pide el usuario antes de `Enrollment.Create(...)`.
- [x] U2 `EnrollmentConfirm`: pide el usuario antes de `GetByIdAsync`.
- [x] U3 `SchoolYearArchive`: pide el usuario antes de `GetByIdAsync`, de modo que un anónimo recibe «sin sesión» y no «no encontrado».
- [x] U4 `CalendarEventFinderByClass`: pide el usuario antes de `ClassFinderById`. Antes de cambiarlo se comprueba si `backend-authorization` documenta ese orden como excepción aceptada (existencia de la clase antes del usuario); si es una excepción documentada para ese caso, se deja y se explica.

## Criterios de aceptación
- Un anónimo (sin usuario actual) recibe siempre `Unauthenticated` en esos casos de uso, también cuando el id no existe o los datos no son válidos; un test por caso de uso lo prueba (RED primero).
- El resto del comportamiento no cambia: mismos errores para quien sí tiene sesión; suite completa en verde, `var` vacío.
- `backend-authorization` y `backend-cross-module` dicen lo mismo que el código.

## Ruta
Un escritor delegado hace U1–U4 en orden, con RED primero en cada una. Commit tras el «sí» del usuario.

## Progreso y evidencia
- U1-U4: `EnrollmentCreate`, `EnrollmentConfirm`, `SchoolYearArchive` y `CalendarEventFinderByClass` piden `GetCurrentUserId()` lo primero (ninguno era una excepción documentada). Un test nuevo por caso de uso, visto en rojo por el motivo esperado (recibía `IdRequired`, `EnrollmentExceptions.NotFound`, `SchoolYearExceptions.NotFound` y `ClassExceptions.NotFound`) y luego en verde. Mutación por caso (volver al orden anterior) detectada por el test nuevo. Suite 1249 correctos (1245 + 4); `var` vacío. `backend-cross-module` corregida (decía que `EnrollmentCreate` no usa el usuario actual). Fuera, a decidir: `AssignmentFinderByClass`, `ClassExpenseFinderByClass` y `LostItemFinderByClass` empiezan por `Pagination.Resolve` (un anónimo con parámetros inválidos recibe 400) y `SubjectFinderByClass` comprueba la clase antes del usuario. Revisión nativa: no solicitada.

## Siguiente paso
Hecho. Quedan a decidir los cuatro buscadores citados.
