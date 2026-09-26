# Glosario de dominio (español → inglés)

Desde que se decidió que todo el código (identificadores, archivos, carpetas) va en inglés, este glosario es la referencia única para traducir de forma consistente los términos de negocio del dominio de SumiClas — descrito originalmente en español en `docs/arquitectura-app-coordinacion-escolar.md` — al nombrarlos en el código de `apps/backend` y `apps/frontend`.

Backend y frontend deben usar siempre la misma palabra en inglés para el mismo concepto (se integran vía el cliente NSwag generado a partir del OpenAPI del backend), así que cualquier término de dominio nuevo se añade aquí antes de usarse en el código, no se traduce por separado en cada lado.

| Español | Inglés |
|---|---|
| Colegio | School |
| CursoEscolar | SchoolYear |
| Clase | Class |
| Matrícula | Enrollment |
| Alumno | Student |
| Hijo (desde el punto de vista del tutor) | Child |
| Aviso | Announcement |
| EventoCalendario / Evento | CalendarEvent / Event |
| Cumpleaños | Birthday |
| Deberes (módulo) | Homework |
| Tarea (item de deberes) | Assignment |
| Asignatura | Subject |
| Gastos (módulo) | Expenses |
| Gasto (item) | Expense |
| Objetos perdidos (módulo) | LostAndFound |
| Objeto (item) | Item |
| Delegado (de padres) | ClassRepresentative |
| Participante | Participant |
| Tutor (padre/madre/tutor) | Guardian |
| Alcance (colegio/clase) | Scope (school/class) |
| Prioridad (baja/normal/alta) | Priority (low/normal/high) |
| Recuperado | Recovered |
| Resumen (IA) | Summary |

Si aparece un término de dominio nuevo que no está en esta tabla, tradúcelo de forma consistente con estos criterios y añade la fila correspondiente aquí.

**Nota**: esto no afecta al texto que ve el usuario final de la app (botones, títulos, mensajes) ni al contenido de los datos (nombres de personas, colegios, descripciones), que siguen en español porque los usuarios reales son hispanohablantes. Este glosario es solo para nombrar cosas en el código: clases, tipos, variables, rutas, carpetas.
