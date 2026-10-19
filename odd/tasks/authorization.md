# Autorización por pertenencia a la clase y por rol

## Objetivo
Que cada caso de uso expuesto por la API compruebe que quien lo ejecuta tiene derecho a hacerlo: pertenecer a la clase para leer o escribir en ella y tener el rol adecuado para lo que solo puede hacer el delegado. Cierra el hueco principal de seguridad del backend (cualquier usuario con sesión lee y escribe datos de cualquier clase, incluidos datos de menores).

## Contexto y decisiones
- Fuente: `docs/arquitectura-app-coordinacion-escolar.md` §3 (roles) y §7.3 (privacidad), y la sección "REVISIÓN COMPLETA PENDIENTE" de `apps/backend/AGENTS.md`.
- Hoy solo existen los roles de clase `ClassRepresentative` y `Participant` (`ClassRoleType`); no hay profesor ni administrador de colegio.
- **Miembro de una clase** = delegado de esa clase o tutor de un alumno con matrícula confirmada en ella (es la definición que ya usa `UserClassMemberFinderByClass`).
- Quien no es miembro recibe 403 (`Forbidden`, ya mapeado en `ResultExtensions`); la comprobación va en el caso de uso, no en el endpoint, para que los tests de casos de uso la cubran.
- Los casos de uso que otros casos de uso consultan por el bus (`ClassFinderById`, `UserFinderById`...) no se protegen: la autorización va en los casos de uso que llama un endpoint.

## Decisiones del usuario (2026-10-19)
- Hoy la app solo da funcionalidad a delegados y tutores; el delegado es el rol superior: **crea los avisos, los eventos oficiales y los gastos** de su clase.
- Matrículas: matricular (`EnrollmentCreate`) y confirmar (`EnrollmentConfirm`) a un alumno solo lo hace su tutor; sugerir continuidad (`EnrollmentSuggestedCreate`) ya exige ser delegado de la clase nueva.

- Cualquier miembro de la clase puede crear **objetos perdidos**, comentar y proponer deberes (doc §3). Los **cumpleaños** son un caso de uso aparte (`CalendarEventBirthdayCreate`): los crea el tutor de ese alumno; restringir `CalendarEventCreate` a delegados no los afecta.

## Decisiones tomadas por defecto (cambiables)
- Contenido de ámbito colegio (avisos y eventos): no existe el rol administrador de colegio. Lectura para cualquier miembro de una clase de ese colegio y publicación por un delegado de cualquier clase de ese colegio.
- Marcar pagos (`ClassExpensePaidMark`/`UnpaidMark`): el delegado.

## Tareas
- [x] A0 Caso de uso `UserClassAccessEnsure` (módulo Users): `Result` de éxito si el usuario es delegado de la clase o tutor de un alumno con matrícula confirmada; si no, `User.ClassAccessForbidden` (403). Con tests.
- [ ] A1 Lecturas por clase: `AnnouncementFinderByClass`, `AssignmentFinderByClass`, `CalendarEventFinderByClass`, `ClassExpenseFinderByClass`, `LostItemFinderByClass`, `SubjectFinderByClass`, `EnrollmentFinderByClass` y las lecturas de alumnos de una clase. Cada una consulta A0 con el usuario actual y devuelve `Unauthenticated` sin usuario. Con tests (y mutación).
- [ ] B1 Escrituras por pertenencia: `AssignmentCreate`, `AssignmentCommentAdd`, `ClassExpenseCreate` (y sus participantes deben ser de la clase), `LostItemCreate`, `CalendarEventBirthdayCreate` (solo el tutor del alumno), `AnnouncementReadMark`.
- [ ] B2 Escrituras por rol: `AnnouncementCreate` y `CalendarEventCreate` de clase (solo delegado), `SubjectCreate`, `ClassRename`, `ClassExpensePaidMark`/`UnpaidMark`, marcas de objetos perdidos, `SchoolYearArchive`/`SchoolYearCreate`, `EnrollmentCreate`/`EnrollmentConfirm`/`EnrollmentSuggestedCreate` (solo sobre alumnos propios).
- [ ] C Frontend: tratar el 403 en las pantallas que leen datos de clase; actualizar `AGENTS.md` (backend y raíz) y quitar el aviso de "no exponer esta API".

## Criterios de aceptación
- Un usuario que no es miembro de una clase recibe 403 en todos sus GET y escrituras, y los tests de cada caso de uso lo prueban (con mutación: quitar la comprobación hace fallar un test).
- Los casos de uso siguen sin comprobar nada cuando los llama otro caso de uso por el bus.
- `dotnet build` limpio y `dotnet test` en verde con varias semillas.

## Progreso y evidencia
- A0 hecho (sin commitear): `UserClassAccessEnsure` con 7 tests (representante de la clase, tutor con matrícula confirmada, matrícula solo sugerida, alumno de otra clase, representante de otra clase, otro usuario aunque la clase tenga delegado, ni tutor ni delegado). 8 mutaciones del caso de uso: sobrevivía una (quitar el id del usuario de la búsqueda de delegado, que dejaría pasar a cualquiera si la clase tiene delegado), cubierta con un test nuevo. 711 tests en verde con dos semillas. Los helpers `ShouldFindEnrollments`/`ShouldFindGuardians` pasan a `UsersUnitTestCase` porque los usan dos fixtures.

## Siguiente paso
A1: añadir la comprobación a los GET por clase, empezando por `AnnouncementFinderByClass`.
