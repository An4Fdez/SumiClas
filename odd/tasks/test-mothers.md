# Objetos de test construidos a mano: pasarlos a Mothers

## Objetivo
`backend-testing` manda que ningún test construya a mano un `Command`, `Query`, DTO de respuesta, resultado, especificación, opciones u objeto de dominio: cada tipo tiene su `<Tipo>Mother`, que solo expone lo que algún test usa. La auditoría del 2026-10-25 contó unos 70 objetos hechos a mano en los tests. Se pasan a Mothers sin cambiar lo que cada test comprueba.

## Alcance autorizado
Solo `apps/test/backend/SumiClas.Tests`. Nada de código de producción. Los literales de tiempo y de fechas de los tests quedan fuera por decisión del usuario («en los tests da más igual»), salvo los que el propio cambio toque. Los Mothers nuevos exponen solo los métodos que un test usa, y llaman al constructor completo (público) de la entidad, no a su `Create` semántico.

## Tareas
- [x] P1 `WeeklySummaries/Application/WeeklySummaryGenerateShould.cs` (respuestas `WeeklySummaryResponse`, `AnnouncementPeriodResponse`, `CalendarEventPeriodResponse`, `AssignmentPeriodResponse`, `ClassExpenseOpenResponse` y las entradas `WeeklySummaryInput` hechas a mano), `WeeklySummaries/WeeklySummariesUnitTestCase.cs` (`LostItemOpenResponse`) y `Api/DashboardEndpointsShould.cs` (`DashboardSummaryResponse`).
- [x] P2 Especificaciones hechas a mano: `Shared/Infrastructure/Persistence/SpecificationsShould.cs` (unas 15), `KeysetPaginationShould.cs` y `WeeklySummaryPersistenceShould.cs`.
- [x] P3 DTO esperados en las aserciones de los tests de buscadores: Announcements (`AnnouncementPeriodFinderByClassShould`, `AnnouncementUnreadSummaryFinderByClassShould`), Assignments (`AssignmentPeriodFinderByClassShould`, `AssignmentPendingSummaryFinderByClassShould`), CalendarEvents (`CalendarEventPeriodFinderByClassShould`, `CalendarEventUpcomingFinderByClassShould`), ClassExpenses (`ClassExpenseFinderByClassShould`, `ClassExpenseOpenFinderByClassShould`, `ClassExpensePendingSummaryFinderByClassShould`), LostItems (`LostItemRecentSummaryFinderByClassShould`), Dashboard (`DashboardSummaryFinderByClassShould`) y Users (`UserClassRepresentativeAssignShould`, `UserClassFirstRepresentativeAssignOnClassCreateShould` con `ClassRoleAssignment`).

## Criterios de aceptación
- No queda en esos ficheros ninguna construcción a mano de los tipos citados (grep de `new <Tipo>(` fuera de los `*Mother.cs`).
- Cada test comprueba lo mismo que antes: se verifica con al menos una mutación por área en producción que sigue habiendo un test que falla.
- Suite en verde (1245), `var` vacío, sin literales numéricos nuevos sin nombre, ninguna línea nueva de más de 120 columnas.

## Ruta
Tres escritores delegados, uno por tarea y seguidos (comparten el proyecto de tests). Refactor de tests: la red de seguridad son los propios tests más las mutaciones. Commit por tarea tras el «sí» del usuario.

## Progreso y evidencia
- P1: Mothers nuevos `AnnouncementPeriodResponseMother`, `CalendarEventPeriodResponseMother`, `AssignmentPeriodResponseMother`, `ClassExpenseOpenResponseMother`, `LostItemOpenResponseMother`, `DashboardSummaryResponseMother` y `WeeklySummaryResponseMother`; `WeeklySummaryInputMother` gana `weekStart`. `WeeklySummaryGenerateShould`, `WeeklySummariesUnitTestCase` y `DashboardEndpointsShould` ya no construyen nada a mano (grep vacío en esos ficheros); dos líneas de más de 120 columnas ordenadas. Suite 1245 correctos; `var` vacío; producción intacta. Seis mutaciones en producción detectadas por el escritor (cumpleaños, fecha del deber, gastos, objetos perdidos, fin de semana y días del dashboard). Los `new` que quedan en los tests de buscadores son de P3. Revisión nativa: no solicitada.
- P2: 6 Mothers de especificación nuevos (`AnnouncementSpecificationMother`, `AssignmentSpecificationMother`, `ClassExpenseSpecificationMother`, `LostItemSpecificationMother`, `UserSpecificationMother`, `WeeklySummarySpecificationMother`); `SpecificationsShould`, `KeysetPaginationShould` y `WeeklySummaryPersistenceShould` ya no construyen ninguna especificación a mano, y el grep de `new w+Specification(` fuera de los Mothers da 0 en todo `apps/test/backend`. Suite 1245 correctos; `var` vacío; producción intacta. Siete mutaciones en producción detectadas (`classIds` de usuarios, `createdBefore`, no leído, `dueTo`, semana del resumen, comparación keyset y gasto pagado). Observado: el build emite un `CS8602` en `Api/LoggerMock.cs` que viene de la tarea de los dobles. Revisión nativa: no solicitada.
- P3: Mothers nuevos `AnnouncementUnreadSummaryResponseMother`, `AssignmentPendingSummaryResponseMother`, `ClassExpensePendingSummaryResponseMother`, `ExpenseParticipantResponseMother`, `LostItemRecentSummaryResponseMother` y `CalendarEventUpcomingResponseMother`, más una sobrecarga completa de `DashboardSummaryResponseMother`; 13 tests de buscadores y casos de uso dejan de construir DTO a mano (`ClassRoleAssignment` ya tenía Mother). También se arregla el `CS8602` de `LoggerMock` (`Convert.ToString(state) ?? string.Empty`). Suite 1245 correctos; `var` vacío; producción intacta; el build solo emite `CS8618`. Doce mutaciones en producción detectadas, una o dos por área. Revisión nativa: no solicitada.

## Siguiente paso
Las tres tareas están hechas. Queda `new AiOptions()` en `AiOptionsShould` a propósito (es el valor por defecto que se compara).
