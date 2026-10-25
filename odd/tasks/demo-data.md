# Datos de demostración para presentar la web

## Objetivo
Tener en la base de desarrollo un conjunto de datos de prueba, todo en español e inventado, con el que se pueda enseñar el funcionamiento completo de la web en una presentación: colegios, clases, padres, madres y tutores con uno o varios hijos, avisos, eventos del calendario, objetos perdidos, deberes y gastos. El README lleva el email y la contraseña de algunas familias ficticias con casos distintos, para que quien evalúe el proyecto pueda entrar.

## Decisiones
- Se crea todo por la **API real** (registro, login, colegios del catálogo, cursos, clases, hijos, matrículas, asignaturas, eventos, avisos, objetos perdidos, deberes, gastos y marcas), no por SQL, para que se apliquen las reglas del dominio. Las cuentas se crean con `POST /auth/register`.
- El cargador es un script reutilizable en `scripts/demo-data/` (fuera de `SumiClas.slnx`, sin tocar proyectos), con **ids deterministas** (para no duplicar), **fechas relativas al día** que se le pase (`--day`, por defecto hoy) y un `reset` que borra solo lo creado por la demo, identificado por esos ids. Los avisos y objetos perdidos no se pueden retrasar en el tiempo: se crean con la fecha del momento de la carga.
- Se conserva lo que ya hay en la base: la cuenta real de Ana Fernández Muñoz, su hija Martina y la clase 1ºA del CEIP Domingo Miras.
- Colegios de la demo, del catálogo oficial ya cargado: CEIP Aravaca (Madrid, id `0547848d-a673-4599-a4f3-ca583fdd0b49`) y CEIP Aníbal González (Sevilla, id `fe57f114-ea79-40c4-8666-88c29ca25633`). Año escolar activo 2026-2027 en cada uno.
- Límites del dominio que condicionan los datos: todo el contenido es de ámbito clase (el ámbito colegio da 403 por diseño); un hijo tiene un solo tutor (el segundo tutor está fuera del MVP); solo el delegado crea avisos, eventos, asignaturas y gastos; los deberes de un no delegado son propuestas.
- Contraseña de las cuentas demo: sencilla y única (`Demo1234`), emails con dominio inventado `@demo.sumiclas.test`. Las claves reales (JWT, Anthropic) no se tocan: el script arranca (o reutiliza) la API sin User Secrets; el resumen semanal con IA solo funcionará el día de la presentación si el usuario arranca la API con su clave.

## Tareas
- [x] S1 Script `scripts/demo-data/` con la carga (cuentas, colegios, clases, hijos, matrículas, asignaturas, eventos, avisos, objetos perdidos, deberes, gastos, marcas de leído, hecho y pagado), el `reset` y un README del script. Se prueba primero contra una base de pruebas desechable (nunca contra la de desarrollo), incluida la recarga.
- [x] S2 Carga en la base de desarrollo `sumiclas` (tras una copia de seguridad) y comprobación de que cada cuenta destacada ve lo previsto: por la API y mirando la web en el navegador.
- [x] S3 Sección en el `README.md` con el email y la contraseña de las cuentas destacadas y qué caso muestra cada una, y la mención del script en el `AGENTS.md`/skill que corresponda.

## Dataset previsto
- Clases: Aravaca 3ºA (delegada Lucía) y 4ºB (delegado Javier); Sevilla 2ºA (delegada Carmen). Asignaturas: Matemáticas, Lengua, Ciencias Naturales, Inglés y Educación Física.
- Cuentas destacadas: Lucía Navarro Ortega (madre, delegada de Aravaca 3ºA, un hijo); Marcos Gil Prieto (padre con dos hijos en la misma clase); Elena Ruiz Campos (madre con un hijo en Aravaca 4ºB y otro en Sevilla 2ºA); Tomás Herrero Vidal (padre sin nada pendiente); Irene Castillo Mora (recién registrada, sin hijos ni clase, para ver el alta).
- Más cuentas para que las listas y los gastos parezcan reales (entre 8 y 10 familias en Aravaca 3ºA).
- Contenido por clase, con fechas relativas al día elegido: avisos (urgente, normal y bajo; uno enlazado a un evento), eventos (excursión, reunión, actividad, otro y cumpleaños), objetos perdidos (perdido y encontrado), deberes en varias asignaturas con comentarios y marcas de hecho, y gastos con varios pagos hechos y otros pendientes; avisos sin leer para algunas familias y ninguno para Tomás.

## Criterios de aceptación
- El script carga y recarga sin duplicar, y el `reset` deja la base como estaba antes de la demo sin tocar los datos que ya existían.
- Todo el contenido está en español y es inventado; nada real salvo los nombres oficiales de los colegios del catálogo.
- Las cuentas destacadas entran con el email y la contraseña del README y ven el caso previsto.
- No se escribe ninguna clave real en el repositorio.

## Ruta
Un escritor delegado hace S1 y S2 en orden (S2 con copia de seguridad previa en la carpeta temporal del trabajo) y S3 después; yo verifico en el navegador. Commit tras el «sí» del usuario, sin incluir el estado de la base.

## Progreso y evidencia
- S1: `scripts/demo-data/` (`seed.mjs` con `load`, `reset` y `verify`; `dataset.mjs` con el contenido; README propio), Node sin dependencias, ids deterministas, `--day`, pacing del límite de login. Probado en una base desechable: carga sin errores (18 cuentas, 19 hijos), recarga sin duplicar, `reset` devuelve las tablas al estado previo, datos ajenos intactos, se niega a usar una API que no sea local. S2: cargado en la base de desarrollo a través de la API del IDE (día de referencia 2026-10-25): Users 19, Students 20, Classes 4, Enrollments 20, Subjects 14, Announcements 8, CalendarEvents 15, LostItems 6, Assignments 12, ClassExpenses 4; la huella md5 de las tablas de Ana, Martina, el curso y la clase 1ºA es idéntica antes y después; copia de seguridad previa en la carpeta temporal del trabajo. Verificación independiente por la API con `verify`: Lucía (delegada, 0 avisos sin leer), Marcos (1 aviso sin leer, gasto pendiente, 5 deberes), Elena (4ºB con 2 sin leer y 3ºA con 3), Tomás (nada pendiente), Carmen (Sevilla 2ºA) e Irene (sin hijos, 52 provincias). S3: sección «Cuentas de demostración» en `Readme.md` y una línea en el `AGENTS.md` raíz. Desviación respecto a la ficha: Elena tiene sus dos hijos en dos clases de Aravaca (4ºB y 3ºA), y el caso de otro colegio y ciudad lo muestra Carmen (Sevilla). No comprobado: la web en el navegador (la extensión de Chrome no estaba conectada). Revisión nativa: no solicitada.

## Siguiente paso
Commit del script y de la documentación (sin el estado de la base). Pendiente: mirar la web con las cuentas destacadas cuando el navegador esté disponible.
