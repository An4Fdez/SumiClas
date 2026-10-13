# Tests de los casos de uso que faltan

## Objetivo
Cubrir con tests unitarios los 42 casos de uso del backend que hoy no tienen ninguno, con exactamente las mismas convenciones que ya se aplicaron a `Sessions`, `Users`, `Schools`, el territorio, `Announcements` y `Enrollments` (ver "Tests" en `apps/backend/AGENTS.md`).

## Decisiones (usuario)
- Solo se prueban casos de uso (más la infraestructura con lógica propia); el dominio se prueba a través de los casos de uso que lo alcanzan.
- Clase `<CasoDeUso>Should`, métodos en `snake_case` describiendo el comportamiento, una carpeta por módulo (`<Modulo>/Application/`), un fixture por caso de uso. **No se testean los `Handler`** (solo delegan; decisión expresa del usuario).
- Siempre Object Mother para `Command`/`Query`/respuestas/entidades, excepto los que no tienen parámetros (se crean con `new`). Los Mothers de entidades llaman al constructor **completo y público** de la entidad, nunca a su `Create`. Un Mother solo expone lo que los tests usan.
- Datos de relleno aleatorios o de Mother; los literales solo cuando son el comportamiento que se prueba. Los datos se declaran como variables locales de cada test, no como campos del fixture.
- Los registros de `QueryBus`/`UserProvider` son helpers `Should...` llamados desde cada test (nunca desde `[SetUp]`); los de varios módulos van a `UnitTestCase`, los de un agregado a su módulo como extensión de `Mock<IQueryBus>`, los de un solo caso de uso quedan privados en su fixture.
- Cada test se comprueba rompiendo a propósito una expectativa (mutación) y se mantienen estables con orden aleatorio.
- Las pruebas describen el comportamiento **actual**; los huecos de autorización conocidos (ver "A revisar (autorización)" en `apps/backend/AGENTS.md`) no se corrigen aquí, solo se anotan si un test los deja a la vista.

## Restricciones
- No se toca código de producción salvo hacer **público el constructor con todo el estado** de una entidad cuando su Mother lo necesite (decisión del usuario), manteniendo el constructor privado vacío de EF; comprobar con `dotnet ef migrations has-pending-model-changes --project apps/backend/src/SumiClas/SumiClas.csproj`.
- No se tocan `.csproj` ni `SumiClas.slnx`; paquetes nuevos solo con permiso explícito.
- Un escritor a la vez; commits directos a `main` (regla del proyecto), uno por unidad de trabajo, cada uno compilando por sí solo y con revisión nativa si el usuario la consiente.
- Sin atribución de IA en commits.

## Tareas
Cada tarea cierra con un commit con evidencia (build, `dotnet test`, mutación, estabilidad con orden aleatorio, greps de estilo).
- [x] T1 `Classes` (ClassCreate, ClassFinderById, ClassFinderByIds, ClassFinderBySchoolYear, ClassRename) y `SchoolYears` (SchoolYearArchive, SchoolYearCreate, SchoolYearEnsureIsActive, SchoolYearFinderByIds, SchoolYearFinderBySchool). Ruta: delegada, un escritor. Hecha (commit `73a50f0`): 74 tests nuevos, cobertura 100 % de `Application` y `Domain` de ambos módulos; constructores públicos en `Class` y `SchoolYear`.
- [x] T2 `Enrollments` (EnrollmentCreate, EnrollmentConfirm, EnrollmentFinderByClass, EnrollmentFinderByStudents) y `Students` (StudentRegister, StudentFinderById, StudentFinderByCurrentGuardian, StudentGuardianAdd, StudentGuardianRemove). Ruta: delegada. Hecha (commit `ab7ba5f`).
- [x] T3 `Users` (UserClassRepresentativeAssign, UserClassRepresentativeResign) y `Subjects` (SubjectCreate, SubjectFinderByClass, SubjectFinderById). Ruta: delegada. Hecha (commit `2ff6c9e`).
- [ ] T4 `Announcements` (AnnouncementCreate, AnnouncementReadMark) y `CalendarEvents` (CalendarEventCreate, CalendarEventBirthdayCreate, CalendarEventFinderByClass, CalendarEventFinderById, CalendarEventFinderByIds). Ruta: delegada.
- [ ] T5 `Assignments` (AssignmentCreate, AssignmentCommentAdd, AssignmentFinderByClass) y `ClassExpenses` (ClassExpenseCreate, ClassExpenseFinderByClass, ClassExpensePaidMark, ClassExpenseUnpaidMark). Ruta: delegada.
- [ ] T6 `LostItems` (LostItemCreate, LostItemFinderByClass, LostItemRecoveredMark, LostItemNotRecoveredMark). Ruta: delegada.
- [ ] T7 Cierre: informe de cobertura, actualizar la sección de tests de `apps/backend/AGENTS.md` y esta ficha.

## Criterios de aceptación
- Los 42 casos de uso tienen su `<CasoDeUso>Should` (comprobable con el inventario de casos de uso frente a ficheros `*Should.cs`).
- `dotnet build` sin errores ni avisos y `dotnet test` en verde, tres veces seguidas y una con semilla de orden aleatorio.
- Greps sin `var`, sin `Assert.`, sin líneas de más de 120 columnas, sin métodos de Mother sin usar, sin `.Setup(` de `QueryBus`/`UserProvider` fuera de helpers `Should...`.
- Cobertura de líneas de las capas `Application` de los módulos tocados por encima del 85 %.

## Progreso y evidencia
- Línea base (2026-10-13): 283 tests en verde; 22 de 64 casos de uso con test; cobertura global de líneas ~29 % (sin código generado).
- Tras T1: 357 tests. Tras T2: 457 tests (Enrollments y Students: cobertura de Application 96,6 % y 96,9 %).
- Tras T3: 502. Tras quitar los tests de `Handler` y cubrir 4 casos de uso de `Users` más: ver el commit siguiente.

## Hallazgos (comportamiento actual, probado tal cual y sin corregir)
Se acumulan al terminar cada tarea; el resumen final se entrega al usuario al cerrar T7.
- **Autorización ausente** (hueco ya apuntado en `apps/backend/AGENTS.md`): cualquier usuario autenticado puede archivar un curso (`SchoolYearArchive`), crear cursos (`SchoolYearCreate`), crear y renombrar clases (`ClassCreate`, `ClassRename`), matricular y confirmar a cualquier alumno (`EnrollmentCreate`, `EnrollmentConfirm`, que ni siquiera usan `IUserProvider`) y crear o listar asignaturas de cualquier clase (`SubjectCreate`, `SubjectFinderByClass`).
- **Delegado de clase:** `UserClassRepresentativeAssign` no exige que el destinatario pertenezca a la clase; comprueba que quien actúa es delegado antes de comprobar que el destinatario existe (un no delegado recibe 403 aunque no exista); `UserClassRepresentativeResign` no tiene control de concurrencia (dos dimisiones simultáneas podrían dejar la clase sin delegado).
- **Estados y roles numéricos:** `Enum.TryParse` acepta `"0"`/`"1"` como valores válidos en `SchoolYearFinderBySchool` (estado) y `"1"` como rol en `StudentRegister` y `StudentGuardianAdd`; solo se rechazan los números sin valor definido.
- **Orden de validaciones** (documentado con tests): `ClassRename` comprueba el curso activo antes que el nombre; `SchoolYearCreate` comprueba el colegio antes que los datos; `SchoolYearFinderBySchool` valida el estado antes que el colegio.
- **Sin validación de duplicados:** `SubjectCreate` permite repetir la misma asignatura en una clase.
- **Ordenación sensible a la cultura del servidor:** `StudentFinderByCurrentGuardian` (por nombre) y `SubjectFinderByClass` (por etiqueta) usan el comparador de cadenas por defecto; además `StudentFinderByCurrentGuardian` expone `FirstSurname[..1] + "."` sin normalizar mayúsculas.
- **Filtros por ids:** una colección vacía devuelve vacío, pero `null` en `UserSpecification(ids: null)` devolvería todos los usuarios (`UserFinderByIds`, `UserClassMemberFinderByIds`; la query no admite `null`, así que no hay guarda); el orden del resultado es el que devuelve el repositorio, no el de los ids pedidos.
- **Código inalcanzable:** una rama de `Student.Register` (`GuardianLink.Create` no puede fallar tras `Validate`) y la rama `Guid.Empty` de `StudentGuardianAdd` (solo se alcanza simulando el buscador de usuarios).
- **Constructores públicos de las entidades** (decisión del usuario para los Mothers): permiten construir entidades sin pasar por la validación de sus factorías; las revisiones nativas lo señalaron como WARNING en `Class`, `SchoolYear`, `Student`, `Subject`... Alternativa propuesta por la revisión: `internal` + `InternalsVisibleTo`.
- **Lectura de EF sin probar:** `SchoolYear`, `Enrollment`, `Student`, `RefreshToken` y otras entidades tienen ahora un constructor público con todo el estado que EF podría usar para materializar; el modelo no cambia, pero ninguna prueba lee filas reales (falta TestContainers).
- **Cobertura retirada por decisión del usuario:** las factorías `School.Create`, `Province.Create`, `Locality.Create`, `PostalCode.Create` (solo las usa el importador), igualdad y `ToString` de `Password`, idempotencia de `TextNormalizer`, `Pagination.ByDate`.
- **De proceso:** se habían conservado 54 tests de `Handler` heredados y se habían replicado en tests nuevos, contra lo que el usuario había indicado; retirados, y 4 casos de uso que solo tenían ese test pasan a estar cubiertos de verdad.

## Siguiente paso
T4. Antes, en el commit en curso: quitar los 39 tests de `Handler` que quedaban y cubrir de verdad `UserFinderById`, `UserFinderByIds`, `UserClassMemberFinderByIds` y `UserIsClassRepresentative` (solo tenían el test del `Handler`; el inventario inicial los daba por cubiertos solo porque existía el fichero).
