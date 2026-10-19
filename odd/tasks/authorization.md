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
- [x] A1 Lecturas por clase: `AnnouncementFinderByClass`, `AssignmentFinderByClass`, `CalendarEventFinderByClass`, `ClassExpenseFinderByClass`, `LostItemFinderByClass` y `SubjectFinderByClass` (las únicas que expone la API; `EnrollmentFinderByClass` solo lo usan otros casos de uso). Cada una consulta A0 con el usuario actual y devuelve `Unauthenticated` sin usuario. Con tests (y mutación).
- [x] B1 Escrituras por pertenencia: `AssignmentCreate`, `AssignmentCommentAdd`, `LostItemCreate` (ámbito clase), `AnnouncementReadMark` (aviso de clase) y `CalendarEventBirthdayCreate` (solo el tutor del alumno, con el nuevo `StudentGuardianEnsure`). `ClassExpenseCreate` pasa a B2 porque solo el delegado crea gastos.
- [x] B2a Solo el delegado de la clase: `AnnouncementCreate` y `CalendarEventCreate` (ámbito clase), `ClassExpenseCreate` (y sus participantes deben ser de la clase), `ClassExpensePaidMark`/`UnpaidMark`, `SubjectCreate` y `ClassRename`, con el nuevo `UserClassRepresentativeEnsure`.
- [ ] B2b Tutor del alumno y autor: `EnrollmentCreate` y `EnrollmentConfirm` (solo el tutor del alumno, con `StudentGuardianEnsure`); marcas de objetos perdidos (su autor).
- [ ] B2c Ámbito colegio (avisos y eventos de colegio: delegado de cualquier clase, a confirmar) y cursos (`SchoolYearArchive`, `SchoolYearCreate`: ¿quién puede?).
- [ ] C Frontend: tratar el 403 en las pantallas que leen datos de clase; actualizar `AGENTS.md` (backend y raíz) y quitar el aviso de "no exponer esta API".

## Criterios de aceptación
- Un usuario que no es miembro de una clase recibe 403 en todos sus GET y escrituras, y los tests de cada caso de uso lo prueban (con mutación: quitar la comprobación hace fallar un test).
- Los casos de uso siguen sin comprobar nada cuando los llama otro caso de uso por el bus.
- `dotnet build` limpio y `dotnet test` en verde con varias semillas.

## Progreso y evidencia
- A0 hecho (sin commitear): `UserClassAccessEnsure` con 7 tests (representante de la clase, tutor con matrícula confirmada, matrícula solo sugerida, alumno de otra clase, representante de otra clase, otro usuario aunque la clase tenga delegado, ni tutor ni delegado). 8 mutaciones del caso de uso: sobrevivía una (quitar el id del usuario de la búsqueda de delegado, que dejaría pasar a cualquiera si la clase tiene delegado), cubierta con un test nuevo. 711 tests en verde con dos semillas. Los helpers `ShouldFindEnrollments`/`ShouldFindGuardians` pasan a `UsersUnitTestCase` porque los usan dos fixtures.

- A1 hecho (sin commitear): los 6 lectores comprueban el usuario actual (`Unauthenticated` si no hay) y `UserClassAccessEnsure` tras comprobar que la clase existe; 403 si no pertenece. Cada fixture gana dos tests (acceso prohibido sin leer, sin usuario) y los helpers `ShouldAllowClassAccess`/`ShouldDenyClassAccess` por usuario y clase en su base. 24 mutaciones (quitar el chequeo, usuario o clase equivocados, quitar la autenticación): todas fallan. 722 tests en verde con dos semillas.
- Seguimientos: `AssignmentFinderByClass` llama a `SubjectFinderByClass` por el bus y repite la comprobación con el mismo usuario (una consulta de más); el literal `"Confirmed"` del estado de matrícula se repite en 4 casos de uso (lo señaló la revisión).

- B1 hecho (sin commitear): `StudentGuardianEnsure` (5 tests, 4 mutaciones) y las comprobaciones de pertenencia en 5 escrituras, cada una con su test de acceso prohibido; el cumpleaños sustituye la consulta de alumno por `StudentGuardianEnsure` (que ya incluye la existencia). 15 mutaciones de las comprobaciones: todas fallan. Los avisos de colegio y los objetos perdidos de colegio solo exigen sesión porque no tienen clase contra la que comprobar (`LostItem` no tiene colegio). Dos tests que documentaban el hueco (`not_check_that_the_author_belongs_to_the_class...`) se renombran a `not_require_the_author_to_be_the_class_representative`. 735 tests en verde con dos semillas.
- B2a hecho (sin commitear): `UserClassRepresentativeEnsure` (4 tests, 5 mutaciones) aplicado a 7 casos de uso, cada uno con su test de "no es delegado" y, los que no lo tenían, de "sin sesión"; los participantes de un gasto se comprueban con `UserClassAccessEnsure` (error nuevo `ClassExpense.ParticipantNotInClass`). 28 mutaciones: todas fallan. Tests que documentaban el hueco reescritos (`not_ask_for_the_current_user_nor_add_any_expense` -> `not_add_any_expense`; `not_check_that_the_participants_belong...` -> dos tests de la regla). Se refuerzan los tests de ámbito colegio de `AnnouncementReadMark` y `LostItemCreate` (comprueban el éxito). 750 tests en verde con dos semillas.

## Siguiente paso
B2b y B2c (antes de B2c hay que decidir con el usuario quién publica en ámbito colegio y quién crea o archiva cursos). Antiguo texto: B2: escrituras por rol (solo delegado: avisos y eventos de clase, gastos, asignaturas, renombrar clase, marcas de pago; objetos: marcas por su autor; matrículas: solo el tutor del alumno; cursos).
