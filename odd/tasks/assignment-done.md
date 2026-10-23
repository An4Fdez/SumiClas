# Marca «Hecho» de los deberes persistida en el servidor

## Objetivo
Que la marca «Hecho» de un deber deje de vivir en el `localStorage` del dispositivo y pase a ser del usuario en el servidor, para que la vea en cualquier dispositivo y no se pierda al limpiar el navegador.

## Decisiones (propuestas, siguen el patrón ya existente)
- Misma forma que el «leído» de los avisos: entidad hija del agregado `Assignment` (`AssignmentCompletion`: `UserId` y `CompletedAt`), con un máximo de una por usuario. A diferencia del aviso, la marca es reversible, así que hay dos operaciones idempotentes: marcar y desmarcar.
- Es una marca personal: cada usuario solo ve y cambia la suya; el resto de la clase no sabe quién ha hecho qué (privacidad de menores).
- No exige curso activo (marcar un deber antiguo como hecho no escribe en la clase). Sí exige usuario existente y pertenencia a la clase del deber.
- Endpoints: `PUT /assignments/{assignmentId}/done` y `DELETE /assignments/{assignmentId}/done`, ambos 204. La respuesta de la lista de deberes añade `Done` (bool, del usuario actual).
- Frontend: el estado pasa a venir de la API con actualización optimista y vuelta atrás si falla; el hook `useDoneAssignments` y su `localStorage` se eliminan. Las marcas guardadas hoy en el navegador no se migran (eran una comodidad por dispositivo).

## Tareas
- [x] D1 Dominio, casos de uso y persistencia: `AssignmentCompletion`, `Assignment.MarkAsDoneBy/UnmarkAsDoneBy`, excepciones, `AssignmentDoneMark`, `AssignmentDoneUnmark`, `Done` en `AssignmentResponse` y en el buscador, mapeo EF y migración `AssignmentCompletions`, con sus tests y Mothers.
- [x] D2 API: endpoints `PUT` y `DELETE /assignments/{id}/done` y cliente NSwag regenerado.
- [x] D3 Frontend: repositorio, hook y pantalla de deberes contra la API; se retira el `localStorage`.
- [x] D4 Documentación (`AGENTS.md` de backend y frontend) y verificación de extremo a extremo.

## Criterios de aceptación
- Marcar y desmarcar son idempotentes y solo afectan al usuario actual; otro usuario de la clase no ve la marca; un usuario ajeno a la clase recibe 403; un deber inexistente, 404.
- La lista de deberes devuelve `Done` correcto para el usuario actual.
- Backend: build limpio, todos los tests en verde con las semillas por defecto, 21 y 808; ningún `var`; ninguna línea de test de más de 120 columnas; mutación de producción detectada.
- Frontend: `pnpm typecheck` y `pnpm lint` limpios; la marca sobrevive a recargar y a cambiar de dispositivo.
- Revisión nativa por pieza y un commit por pieza, directo a `main`, sin atribución de IA.

## Progreso y evidencia
- Exploración hecha: patrón de `AnnouncementReadReceipt` / `AnnouncementReadMark`, `AssignmentCommentAdd`, `AssignmentFinderByClass`, mapeos de `Assignment` y `useDoneAssignments`.
- D1 (`fd7cb41`): dominio, dos casos de uso, `Done` en el buscador, mapeo y migración (`AssignmentCompletions`, sin índice único, igual que los recibos de lectura de los avisos; marcar dos veces a la vez no puede dar un 409 espurio y `IsDoneBy`/`UnmarkAsDoneBy` toleran duplicados). Hallazgo: la entidad nueva sin mapeo hizo fallar el test de «sin cambios pendientes del modelo», por eso D1 incluye la persistencia. Tests: `AssignmentDoneMarkShould` (8), `AssignmentDoneUnmarkShould` (6), uno nuevo en `AssignmentFinderByClassShould` y `AssignmentDoneMarksShould` (3, contra PostgreSQL real: insertar, borrar solo la marca de un usuario y volver a marcar sobre un agregado ya guardado). Barrido: 13 mutaciones de dominio y casos de uso y 2 de mapeo detectadas; `UsePropertyAccessMode(Field)` es equivalente (EF ya enlaza `_completions` por convención). 936 tests con las semillas por defecto, 21 y 808. La migración NO se ha aplicado a la base de desarrollo (se aplica al arrancar la API).
- D2 (`4e55fb0`): `PUT` y `DELETE /assignments/{assignmentId}/done` (204, `AssignmentDoneMark`/`AssignmentDoneUnmark`) en `AssignmentsEndpoints.cs`; la autenticación obligatoria ya la cubre `ProtectedEndpointsShould`, que recorre todos los endpoints del host. Cliente NSwag regenerado (`assignmentDoneMark`, `assignmentDoneUnmark` y `done` en `AssignmentResponse`) con la API temporal apuntando a una base desechable (`sumiclas_openapi_scratch`, borrada después) para no migrar la base de desarrollo. La revisión nativa avisó (WARNING) de que el contrato HTTP de los dos endpoints no tenía test; se añade `AssignmentDoneEndpointsShould` (2 tests con el host real y `ISender` simulado: 204, comando y verbo correctos), con 3 mutaciones detectadas (comando cambiado, `Results.Ok` en el DELETE, `PUT` por `POST`). 938 tests en verde (semillas por defecto, 21 y 808), `pnpm typecheck` y `pnpm lint` limpios.
- D3 (`c023497`): `Assignment.done`, `markAsDone`/`markAsNotDone` en `IAssignmentRepository` y `NSwagAssignmentRepository`; `useClassAssignments.setDone` con actualización optimista, vuelta atrás con mensaje si falla, un solo cambio en vuelo por deber y guarda de generación; `Homework` lee `assignment.done` y muestra `doneError`. Se elimina `useDoneAssignments` y su `localStorage`. `pnpm typecheck` y `pnpm lint` limpios. No hay tests de frontend (sin infraestructura); la comprobación en navegador real queda en D4.
- D4: `AGENTS.md` de backend (sección de la marca resuelta, sin índice único) y de frontend (el hueco PENDIENTE pasa a descripción de lo hecho) actualizados. **Hallazgo grave en la verificación**: al registrar usuarios la API respondía 201 sin guardar nada. Causa: `UnitOfWorkBehavior` exigía `IRequest<TResponse>` y un `IRequest` simple no lo cumple en MediatR 12, así que todos los comandos sin respuesta quedaban fuera de la transacción (registro, deberes, avisos, gastos…). Arreglo aparte, commit `0f21263`, con tests que recorren todos los comandos y consultas del contenedor real. Verificación de extremo a extremo con la API real y un Chrome headless por CDP: 15 comprobaciones por API (idempotencia, marca personal, 403 de un ajeno, 404, 401) y 17 en la interfaz (marca optimista, persiste tras recargar con el almacenamiento vaciado, desmarcar, sin red se revierte con mensaje y no deja nada en el servidor, otra familia no ve la marca). Los datos de prueba (usuarios `e2e-done-*@example.test`, una clase, un curso escolar, dos alumnos y tres deberes) se borraron de la base de desarrollo con autorización del usuario; no se tocó nada más.

## Siguiente paso
Tarea cerrada. Siguientes puntos del MVP: dashboard real y despliegue.
