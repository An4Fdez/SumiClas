# Modificar y eliminar eventos del calendario, deberes, gastos y objetos perdidos (solo su autor)

## Objetivo
Continúa `odd/tasks/announcement-edit-delete.md` (hecha y commiteada en `8435dbe` y `7c53edc`). El usuario pidió las mismas opciones, con las mismas condiciones, en el calendario, los deberes, los gastos y los objetos perdidos: el usuario con el que ha iniciado sesión puede **modificar y eliminar lo que él mismo publicó**, y solo eso. Todo el contenido se sigue viendo igual para todas las familias.

## Decisiones
- **Mismas condiciones que en los avisos:** solo el autor (`AuthorId` es el usuario actual; si no, 403 con la excepción `ChangeForbidden` de cada módulo); el usuario actual se pide lo primero; 404 si no existe; no se puede modificar ni eliminar algo de un curso archivado (`SchoolYear.Archived`, 400; si el contenido no guarda clase, no se comprueba); eliminar es definitivo y arrastra lo que cuelga del elemento; cada elemento guarda `UpdatedAt` (nulo si nunca se editó) y la pantalla muestra «editado»; la respuesta lleva `IsAuthor` para que la pantalla solo ofrezca los botones al autor (el servidor lo hace cumplir).
- **Marcas al editar (decisión del usuario):** los **deberes** vuelven a quedar pendientes (se borran las marcas de «hecho» de todas las familias; los comentarios se conservan); los **gastos conservan los pagos** ya hechos; el calendario y los objetos perdidos no tienen marcas por familia que cambiar (la marca de «recuperado» se conserva).
- **Qué se puede modificar:**
  - Evento del calendario: tipo (Viaje, Reunión, Actividad u Otro), título, fecha, hora, lugar y coste. **Los cumpleaños no se modifican** (`CalendarEventExceptions.BirthdayChangeForbidden`, 403), pero su autor sí puede eliminarlos.
  - Deber: asignatura (de la misma clase), título, descripción y fecha de entrega. Sigue siendo oficial o propuesta según se creó.
  - Gasto: concepto, importe y evento relacionado. Los participantes no cambian.
  - Objeto perdido: descripción y estado (Busco o Encontrado). La foto no se cambia.
- **Qué cuelga de lo que se elimina:** los comentarios y las marcas de «hecho» de un deber, los participantes y pagos de un gasto. Si se elimina un evento, los avisos y gastos que lo enlazaban quedan sin evento relacionado (los buscadores ya lo toleran).
- **Sin tests nuevos, por decisión del usuario («y sin test aunque solo sea esta vez»).** Los tests existentes se mantienen compilando y en verde; la comprobación es la compilación, el typecheck, el lint y la verificación de extremo a extremo con una API real, una base desechable y la web.
- **Una sola migración** `UpdatedAtOfEventsAssignmentsExpensesAndLostItems` añade la columna nula `UpdatedAt` a las cuatro tablas.
- **API:** `PUT /calendar-events/{id}`, `PUT /assignments/{id}`, `PUT /class-expenses/{id}`, `PUT /lost-items/{id}` y los cuatro `DELETE` equivalentes (204).

## Tareas
- [x] R1 Backend, calendario: `CalendarEvent.UpdatedAt` y `Update`, casos de uso `CalendarEventUpdate` y `CalendarEventDelete`, respuesta con `UpdatedAt` e `IsAuthor`.
- [x] R2 Backend, deberes: `Assignment.UpdatedAt` y `Update` (borra las marcas de «hecho»), casos de uso, respuesta.
- [x] R3 Backend, gastos: `ClassExpense.UpdatedAt` y `Update`, casos de uso, respuesta.
- [x] R4 Backend, objetos perdidos: `LostItem.UpdatedAt` y `Update`, casos de uso, respuesta.
- [x] R5 Migración, endpoints `PUT` y `DELETE` de los cuatro y cliente NSwag regenerado.
- [x] R6 Frontend: repositorios, hooks y pantallas de calendario, deberes, gastos y objetos perdidos con «Editar», «Eliminar» (confirmación) y «editado».
- [x] R7 Verificación de extremo a extremo (API y pantalla) y skills al día.
- [x] R8 Un gasto no se puede eliminar si alguna familia ya ha pagado (decisión del usuario): el servidor lo rechaza con una excepción propia del módulo (400, con tests primero, a diferencia del resto de esta ficha, porque la excepción «sin test» era solo para la primera tanda), y la pantalla lo avisa en el detalle del gasto (sin botón «Eliminar» y con un mensaje que cuenta cuántas familias han pagado). Editar sigue permitido y conserva los pagos.

## Criterios de aceptación
- Solo el autor modifica o elimina; cualquier otro usuario, incluido un delegado, recibe 403 y no ve los botones; sin sesión, 401.
- Editar un deber borra el «hecho» de todas las familias; editar un gasto conserva los pagos.
- Un cumpleaños no se puede modificar, pero su autor puede eliminarlo.
- Suite del backend en verde, `var` vacío, `pnpm typecheck` y `pnpm lint` sin errores, sin estilos en línea nuevos.
- La base de desarrollo del usuario solo recibe la migración (columnas nulas) al arrancar la API.

## Ruta
Un solo escritor, en orden R1 a R7, sin tests nuevos. Commit por bloque (backend, frontend) tras el «sí» del usuario.

## Progreso y evidencia
- R1-R5 (backend): `UpdatedAt` e `IsAuthor` en calendario, deberes, gastos y objetos perdidos; casos de uso `Update` y `Delete` de cada módulo (usuario primero, 404, solo el autor con `ChangeForbidden` 403, curso activo); editar un deber borra las marcas de «hecho» y conserva los comentarios, editar un gasto conserva los pagos, un cumpleaños no se modifica (`BirthdayChangeForbidden` 403) pero su autor puede eliminarlo; una migración `UpdatedAtOfEventsAssignmentsExpensesAndLostItems` con columnas nulas; ocho endpoints `PUT` y `DELETE` con metadatos; cliente NSwag regenerado solo con añadidos. R6 (frontend): componente común `AuthorActions` (editar, eliminar con confirmación y error) usado por los cinco detalles, avisos incluidos; formularios en modo edición; «editado»; aviso en deberes de que vuelven a quedar pendientes. R7: verificación de extremo a extremo hecha por el escritor con API de pruebas y base desechable (API 78 de 78, pantalla 33 de 33 y avisos 12 de 12). Comprobado por el orquestador: backend y tests compilan sin errores, suite 1279 correctos y 0 fallos (tests recompilados de verdad: el escritor reconoció que un recuento intermedio fue con un DLL antiguo), la API compila, `var` vacío, `pnpm typecheck` y `pnpm lint` sin errores y solo queda el `style` de `Reveal`. Sin tests nuevos, por decisión del usuario; un único cambio en tests (`CalendarEventResponseMother` pasa `null, false` a los campos nuevos) para que compile. Skills actualizadas: `backend-status-notes`, `backend-authorization`, `frontend-screens` y `frontend-ui-patterns`. No verificado por el orquestador: la verificación de extremo a extremo ni las capturas. Revisión nativa: no solicitada.
- R8: un gasto con algún pago ya hecho no se puede eliminar. `ClassExpense.HasPayments` (propiedad calculada, sin migración), `ClassExpenseExceptions.HasPayments` (código `ClassExpense.HasPayments`, 400, aprobada en `BadRequestExceptionCatalog` como `SchoolYear.Archived`), comprobación en `ClassExpenseDelete` tras la del autor y antes de la clase y el curso; en la pantalla `AuthorActions` admite `deleteBlockedReason` (sin botón «Eliminar» y con una nota en singular o plural; «Editar» sigue disponible) y el mensaje de error del servidor se hace visible al aparecer. 6 tests nuevos vistos en rojo, 3 mutaciones detectadas; suite 1285 correctos, recompilada de verdad por el orquestador; `var` vacío; extremo a extremo del escritor: API 20 de 20 y pantalla 14 de 14. Sin migración nueva. Revisión nativa: no solicitada.

## Siguiente paso
Commits: logo (aparte), backend y frontend. Reiniciar la API de Rider para que se aplique la migración.
