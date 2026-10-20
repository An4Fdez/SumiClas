# Autorización por pertenencia a la clase y por rol

## Objetivo
Que cada caso de uso expuesto por la API compruebe que quien lo ejecuta tiene derecho a hacerlo: pertenecer a la clase para leer o escribir en ella y tener el rol adecuado para lo que solo puede hacer el delegado. Cierra el hueco principal de seguridad del backend (cualquier usuario con sesión lee y escribe datos de cualquier clase, incluidos datos de menores).

## Contexto y decisiones
- Fuente: `docs/arquitectura-app-coordinacion-escolar.md` §3 (roles) y §7.3 (privacidad), y la antigua sección "REVISIÓN COMPLETA PENDIENTE" de `apps/backend/AGENTS.md` (ahora "Autorización").
- Hoy solo existen los roles de clase `ClassRepresentative` y `Participant` (`ClassRoleType`); no hay profesor ni administrador de colegio.
- **Miembro de una clase** = delegado de esa clase o tutor de un alumno con matrícula confirmada en ella (es la definición que ya usa `UserClassMemberFinderByClass`).
- Quien no es miembro recibe 403 (`Forbidden`, ya mapeado en `ResultExtensions`); la comprobación va en el caso de uso, no en el endpoint, para que los tests de casos de uso la cubran.
- Los casos de uso que otros casos de uso consultan por el bus (`ClassFinderById`, `UserFinderById`...) no se protegen: la autorización va en los casos de uso que llama un endpoint.

## Decisiones del usuario (2026-10-19)
- Hoy la app solo da funcionalidad a delegados y tutores; el delegado es el rol superior: **crea los avisos, los eventos oficiales y los gastos** de su clase.
- Matrículas: matricular (`EnrollmentCreate`) y confirmar (`EnrollmentConfirm`) a un alumno solo lo hace su tutor; sugerir continuidad (`EnrollmentSuggestedCreate`) ya exige ser delegado de la clase nueva.

- Cualquier miembro de la clase puede crear **objetos perdidos**, comentar y proponer deberes (doc §3). Los **cumpleaños** son un caso de uso aparte (`CalendarEventBirthdayCreate`): los crea el tutor de ese alumno; restringir `CalendarEventCreate` a delegados no los afecta.

## Decisiones del usuario (2026-10-19, 2)
- Archivar un curso: como mínimo un delegado de una clase de ese curso.
- Crear avisos y eventos: el delegado; en el futuro se podrá cambiar y, si hubiera profesor, le tocaría a él.

## Decisiones abiertas
- (Resuelto por la tarea S) Los avisos, eventos y objetos perdidos de ámbito colegio **no guardan a qué colegio pertenecen** (ni clase ni colegio): se muestran en todas las clases de todos los colegios. Para que pertenezcan a un colegio hay que guardar su `SchoolId` (cambio de modelo y migración) y filtrar los buscadores por el colegio de la clase. Hasta entonces publicar en ámbito colegio sigue abierto a cualquier usuario con sesión.

## Decisiones tomadas por defecto (cambiables)
- Contenido de ámbito colegio (avisos y eventos): no existe el rol administrador de colegio. Lectura para cualquier miembro de una clase de ese colegio y publicación por un delegado de cualquier clase de ese colegio.
- Marcar pagos (`ClassExpensePaidMark`/`UnpaidMark`): el delegado.

## Tareas
- [x] A0 Caso de uso `UserClassAccessEnsure` (módulo Users): `Result` de éxito si el usuario es delegado de la clase o tutor de un alumno con matrícula confirmada; si no, `User.ClassAccessForbidden` (403). Con tests.
- [x] A1 Lecturas por clase: `AnnouncementFinderByClass`, `AssignmentFinderByClass`, `CalendarEventFinderByClass`, `ClassExpenseFinderByClass`, `LostItemFinderByClass` y `SubjectFinderByClass` (las únicas que expone la API; `EnrollmentFinderByClass` solo lo usan otros casos de uso). Cada una consulta A0 con el usuario actual y devuelve `Unauthenticated` sin usuario. Con tests (y mutación).
- [x] B1 Escrituras por pertenencia: `AssignmentCreate`, `AssignmentCommentAdd`, `LostItemCreate` (ámbito clase), `AnnouncementReadMark` (aviso de clase) y `CalendarEventBirthdayCreate` (solo el tutor del alumno, con el nuevo `StudentGuardianEnsure`). `ClassExpenseCreate` pasa a B2 porque solo el delegado crea gastos.
- [x] B2a Solo el delegado de la clase: `AnnouncementCreate` y `CalendarEventCreate` (ámbito clase), `ClassExpenseCreate` (y sus participantes deben ser de la clase), `ClassExpensePaidMark`/`UnpaidMark`, `SubjectCreate` y `ClassRename`, con el nuevo `UserClassRepresentativeEnsure`.
- [x] B2b Tutor del alumno y autor: `EnrollmentCreate` y `EnrollmentConfirm` (solo el tutor del alumno, con `StudentGuardianEnsure`, que sustituye a la consulta de alumno); `LostItemRecoveredMark`/`NotRecoveredMark` (solo su autor, error nuevo `LostItem.MarkForbidden`).
- [ ] B2c `SchoolYearArchive`: HECHO (solo un delegado de alguna clase del curso). PENDIENTE: avisos y eventos de ámbito colegio (ver "Decisiones abiertas"). Crear cursos y clases se queda abierto (alta de colegio, doc §7.1).
- [x] S El ámbito colegio guarda su colegio (autorizado por el usuario, incluida la migración): `SchoolId` en `Announcement`, `CalendarEvent` y `LostItem`.
  - [x] S1 Caso de uso `SchoolIdFinderBySchoolYear` (curso -> colegio) con tests.
  - [x] S2 Avisos: dominio, especificación, mapeo EF, `AnnouncementCreate` (ámbito colegio: `ClassId` es la clase desde la que se publica; debe ser delegado de ella y el curso estar activo), `AnnouncementFinderByClass` (solo los del colegio de la clase) y tests.
  - [x] S3 Eventos: lo mismo en `CalendarEvent`.
  - [x] S4 Objetos perdidos: lo mismo en `LostItem` (ámbito colegio: `ClassId` es la clase desde la que se publica; basta con pertenecer a ella).
  - [x] S5 Una migración EF (columna `SchoolId` nula en las tres tablas; los avisos, eventos y objetos de colegio que ya existan se quedan sin colegio y dejan de mostrarse) y documentación.
- [x] C Frontend y documentación: pantallas revisadas (ver evidencia), sección "Autorización" del `AGENTS.md` del backend reescrita y avisos del frontend actualizados. El aviso de "no exponer la API" no se quita: se reformula con las condiciones concretas para exponerla.

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
- B2b hecho (sin commitear): matrículas y marcas de objetos perdidos con su test de "no autorizado" y de "sin sesión"; 14 mutaciones: todas fallan; 758 tests en verde con dos semillas. Seguimientos de la revisión de B2a: la comprobación de sesión de las marcas de pago va después de cargar el gasto y la clase (un anónimo recibiría NotFound antes que Unauthenticated), y los participantes de un gasto se comprueban con una consulta cada uno.
- B2c (archivar) hecho (sin commitear): `SchoolYearArchive` exige ser delegado de alguna clase del curso (6 tests nuevos o reescritos, 6 mutaciones: todas fallan; 761 tests en verde).
- S hecho (sin commitear): `SchoolIdFinderBySchoolYear` (2 tests); `Announcement`, `CalendarEvent` y `LostItem` guardan `SchoolId` en ámbito colegio y los tres `FinderByClass` solo devuelven los de el colegio de la clase; al crear en ámbito colegio `ClassId` es obligatorio (clase desde la que se publica; avisos y eventos: delegado; objetos: pertenecer a ella) y el curso debe estar activo; errores nuevos `ClassRequiredForSchoolScope`/`SchoolRequiredForSchoolScope`. 25 mutaciones nuevas: todas fallan (una de compilación descartada). Migración `SchoolIdInSchoolScope` (3 columnas `SchoolId` nulas; generada, NO aplicada a ninguna base de datos; `has-pending-model-changes` limpio). Los avisos, eventos y objetos de colegio que ya existan en una base de datos se quedan sin colegio y dejan de mostrarse. Dominio: las ramas `SchoolRequiredForSchoolScope` y descartar `SchoolId` en ámbito clase solo se alcanzan con el valor provisional del caso de uso, así que ningún test de caso de uso las puede ver. El frontend no crea nada de ámbito colegio, por lo que no cambia. 779 tests en verde con dos semillas (con los tests de ámbito colegio que pidió la revisión: no delegado y curso archivado en avisos, eventos y objetos).
- Commits de esta tanda: `5848d63` (archivar curso), `1e1345d` (SchoolIdFinderBySchoolYear), `38c412b` (seis mensajes de error), `393cebd` (migración), `7cf0771` (avisos), `b5a41c6` (eventos), `fe2fdda` (objetos), `e8aeb46` (tests de ámbito colegio). Las revisiones nativas de avisos, eventos y objetos quedaron en "corrección requerida" por un único hallazgo crítico, asumido por decisión del usuario: los avisos, eventos y objetos de colegio ya guardados no tienen colegio y dejan de mostrarse; el resto de hallazgos eran falsos positivos (la migración ya estaba en el commit anterior y la clase vacía ya la rechaza el dominio) o seguimientos menores.

- C hecho: revisión de pantallas contra las reglas nuevas. Encajaban Avisos, Eventos, Deberes, Asignaturas, cumpleaños y matrículas (el frontend ya ocultaba los botones de crear al que no es delegado, nunca crea ámbito colegio y lista solo hijos con matrícula confirmada) y los errores de lectura ya muestran el texto del backend. Dos desajustes corregidos: pago de gastos (`07edfb7`: cada familia marca su propia parte; la de otra persona, el delegado; era una decisión por defecto mía que rompía la interfaz) y marca de recuperado de un objeto (`960168f`: solo el autor ve el botón). `pnpm typecheck` y `pnpm lint` limpios; sin tests de frontend, verificado leyendo el código. 783 tests de backend.

## Siguiente paso
Nada abierto en esta tarea. Pendiente fuera de ella: aplicar la migración y decidir qué hacer con los datos de colegio antiguos, acotar el alta abierta de cursos y clases, tests de endpoints y de PostgreSQL real.
