# App de coordinación escolar — Documento de arquitectura (Beta / TFM)

## 1. Problema y objetivo

Sustituir el grupo de WhatsApp de clase/colegio como canal de coordinación entre familias, profundamente ineficiente por ser un flujo lineal sin estructura (información duplicada, avisos importantes perdidos entre ruido, sin resúmenes ni filtrado por tema).

La Beta se diseña para cubrir un **colegio completo** (no una única clase), de modo que la estructura de datos no tenga que rehacerse si el proyecto escala más allá de la validación inicial. El desarrollo tiene tiempo limitado, por lo que se prioriza un núcleo funcional sólido sobre la cobertura completa de todas las ideas iniciales.

## 2. Modelo de datos (jerarquía)

```
Colegio
 └── CursoEscolar (ej. "2026/2027", fecha_inicio, fecha_fin, estado: activo/archivado)
      └── Clase (ej. "3ºB" — pertenece a un CursoEscolar concreto, no es permanente)
           └── Matricula (alumno_id, clase_id) — vincula al Alumno con la Clase de ese curso
           └── Usuario (padre/madre/tutor) — con rol dentro de esa clase

Alumno (entidad permanente, independiente del curso)
 └── vinculado a 1+ Usuarios (tutores) y, por curso, a una Clase vía Matricula

Usuario
 ├── rol global: Admin colegio (AMPA / dirección)
 ├── rol de clase: Profesor/tutor de clase
 ├── rol de clase: Delegado de padres
 └── rol de clase: Participante (padre/madre estándar)
```

Puntos clave del diseño:

- Un **Aviso** o un evento de **Calendario** puede publicarse con alcance `colegio` (visible para todas las clases del curso activo) o `clase` (visible solo para una). Esto es lo que permite que el AMPA opere a nivel colegio sin mezclar sus avisos con los de cada aula.
- Un usuario puede pertenecer a varias clases (padres con hijos en cursos distintos) — el modelo de "Usuario pertenece a N clases" evita duplicar cuentas.
- Los roles se modelan **por clase**, no globalmente (salvo el rol de Admin colegio), porque un mismo padre puede ser delegado en la clase de un hijo y participante normal en la de otro.
- **El archivado de cursos anteriores no es una funcionalidad aparte**: al colgar Aviso/Calendario/Deberes/Gastos de una `Clase`, y esta de un `CursoEscolar`, archivar un curso es solo cambiar `CursoEscolar.estado` a `archivado`. El backend deniega cualquier escritura sobre clases de un curso archivado; la única pieza de interfaz nueva es una vista "Cursos anteriores" que reutiliza las pantallas existentes en modo solo lectura, filtradas por curso.

### Entidades principales

| Entidad | Campos clave | Notas |
|---|---|---|
| Colegio | nombre, configuración | Punto de entrada multi-tenant a futuro |
| CursoEscolar | colegio_id, año (ej. "2026/2027"), fecha_inicio, fecha_fin, estado (activo/archivado) | Archivar un curso es solo cambiar `estado` |
| Clase | curso_escolar_id, nombre/grado (ej. "3ºB") | No permanente: se recrea cada CursoEscolar |
| Usuario | nombre, email/teléfono | Auth simple (email+password o magic link); puede tener roles en varias clases |
| Alumno | nombre | Entidad permanente, independiente del curso; vinculado a tutores vía tabla intermedia |
| Matricula | alumno_id, clase_id | Vincula al alumno con su clase de un curso concreto; se confirma cada curso nuevo |
| Rol | usuario_id, clase_id (nullable), tipo (profesor/delegado/participante/admin_colegio) | |
| Aviso | alcance (colegio/clase), autor_id, título, cuerpo, prioridad, fecha | |
| EventoCalendario | alcance, tipo (excursión/reunión/cumpleaños/actividad), fecha, ubicación, coste, autor_id | |
| TareaDeberes | clase_id, asignatura, fecha, descripción, autor_id | Con comentarios cortos asociados |
| ComentarioDeberes | tarea_id, autor_id, texto | Hilo corto, no chat libre |
| GastoClase | clase_id, concepto, importe_total, participantes[], estado_pago por participante | Sin integración de pago real en la Beta |
| ObjetoPerdido | clase_id o colegio_id, descripción, foto, estado (buscado/encontrado) | |
| ResumenSemanal | clase_id, semana, contenido_generado | Generado por IA a partir de Avisos+Calendario+Deberes de la semana |

## 3. Roles y permisos (Beta)

| Rol | Puede publicar Avisos/Calendario oficial | Ámbito |
|---|---|---|
| Admin colegio (AMPA/dirección) | Sí | Todo el colegio |
| Profesor/tutor de clase | Sí | Su clase |
| Delegado de padres | Sí | Su clase |
| Participante (resto de padres) | No publica oficial; puede comentar, proponer en Deberes, publicar en Objetos perdidos | Su(s) clase(s) |

Esto resuelve directamente el problema original: los avisos "de verdad importantes" tienen una fuente fiable y no se mezclan con el ruido conversacional.

## 4. Alcance funcional de la Beta

**Núcleo (imprescindible):**
- Avisos (colegio/clase, con prioridad)
- Calendario (excursiones, reuniones, actividades; cumpleaños como vista filtrada del calendario)
- Deberes en formato "pizarra + comentarios cortos" (no chat libre)
- Roles y permisos básicos descritos arriba
- Notificaciones push (ver stack, sección 5) — **fuera del MVP, para una versión posterior** (ver la nota de alcance de debajo)

> **Nota de alcance (2026-10-23): en esta versión de la Beta los avisos, los eventos del calendario y los objetos perdidos se publican solo con alcance `clase`.** La publicación con alcance `colegio` queda para una versión posterior, cuando existan los roles de **admin del colegio** y de **profesor**. Motivo: el §3 la reserva al admin del colegio, y ese rol no se puede asignar con fiabilidad mientras cualquiera pueda crear una clase y quedar como su delegado (§7.1) sin que nadie verifique a la persona. El modelo de datos ya está preparado (campo de colegio en los tres), así que no hay que rehacer la estructura para abrirlo.
>
> **Nota de alcance (2026-10-23): las notificaciones push quedan fuera del MVP** y pasan a una versión posterior (decisión del usuario: no son un tema principal para el MVP; las familias abren la app para ver lo publicado). Al retomarlas hay que decidir el mecanismo: el §5 propone la Web Push API, que necesita un service worker y una PWA, y el stack del frontend (Vite + React) descarta la PWA y el service worker de forma definitiva (ver el `AGENTS.md` de la raíz), así que habrá que reabrir esa decisión o elegir otro canal. Hoy no hay nada construido.

**Segunda ola (si el tiempo lo permite):**
- Gastos de clase en modo *tracking* (registro de quién participa y quién ha pagado, sin mover dinero real dentro de la app)
- Objetos perdidos
- Resumen semanal automático generado por IA a partir de lo publicado en Avisos/Calendario/Deberes esa semana

**Después de la Beta (fuera de alcance inicial):**
- Ayuda entre familiares
- Buscador conversacional con IA sobre todo el histórico
- Pagos integrados reales
- Panel de administración avanzado para múltiples colegios (multi-tenant real)

## 5. Stack recomendado (decisión tomada)

- **Frontend:** Web app como **PWA** con **React + Next.js** (instalable, notificaciones vía Web Push, sin pasar por App Store/Google Play).
- **Backend:** **API REST con .NET 10 Minimal API**. Usar EF Core Migrations desde el primer commit para tener trazabilidad del modelo de datos (útil para la memoria del TFM). Aunque se use Minimal API, conviene organizar el código en capas (Application/Domain/Infrastructure) para que no se convierta en un único archivo de endpoints.
- **Base de datos:** **PostgreSQL**, vía Npgsql/EF Core. Encaja bien con la jerarquía Colegio→Clase→Matrícula→Usuario y con las relaciones N:M tutores-alumnos.
- **Autenticación:** email + contraseña o magic link; JWT para sesiones. Sin OAuth social necesario para una Beta cerrada por colegio.
- **Notificaciones push:** Web Push API (nativa de las PWA) — evita depender de servicios de terceros para la Beta. **Fuera del MVP**: ver la nota de alcance del §4.
- **IA (resumen semanal / búsqueda):** llamada a un LLM (API de Anthropic/OpenAI) con un prompt que recibe el contenido de la semana ya estructurado desde la base de datos (no hace falta RAG complejo: el volumen de datos por clase es pequeño y se puede pasar directo en el contexto).
- **Hosting:** proveedor con buen soporte para contenedores .NET vía Docker — Render o Fly.io son las opciones más directas; Railway también funciona pero tiene algo menos de documentación específica para .NET. **Actualización (2026-10-24): el usuario ha decidido desplegar en Azure**; estas opciones quedan descartadas.

## 6. Consideraciones para la memoria del TFM

- El diseño colegio→clase→usuario con alcance de publicación (colegio/clase) es el punto arquitectónico más defendible: demuestra que se pensó en escalabilidad desde el modelo de datos, aunque la Beta se despliegue en un único colegio.
- La decisión de no integrar pagos reales en la Beta es justificable por razones de responsabilidad legal/fiscal y complejidad de integración (pasarelas de pago, KYC), no solo por falta de tiempo — es un argumento de diseño válido para la memoria.
- El uso de PWA en vez de apps nativas es defendible como decisión de arquitectura (un único código base, notificaciones push sin tiendas de apps, instalación sin fricción) más que como limitación de tiempo.
- Conviene documentar explícitamente qué queda fuera del alcance de la Beta y por qué (sección 4), ya que en un TFM valorar el alcance controlado con criterio es tan importante como lo implementado.

## 7. Alta, ciclo de vida y privacidad (decisiones tomadas)

### 7.1 Alta de colegio y clase, sin administrador central

- **Verificación del colegio:** al crear un colegio en la app, el usuario debe **buscarlo y seleccionarlo** de un listado público oficial de centros educativos (Ministerio de Educación / consejerías autonómicas tienen datasets abiertos con código de centro, nombre y dirección), en vez de escribirlo libremente. Esto elimina la mayoría de altas de colegios inventados sin necesidad de revisión manual.
- **Rol de delegado de clase:** dentro de un colegio ya verificado, el **primer padre que crea una clase** queda automáticamente como delegado/administrador provisional de esa clase. Puede **ceder el rol** a otro padre en cualquier momento desde la app (por ejemplo, si se une después el delegado "oficial" del AMPA o la clase). No requiere aprobación de ningún admin central: el propio sistema de verificación de colegio + cesión de rol sustituye esa función.
- El profesor/tutor de la clase se da de alta con el mismo mecanismo de autoservicio, pero marcado con rol "profesor" (a decidir en una fase posterior si este rol requiere alguna verificación adicional, por ejemplo un email corporativo del colegio si el centro lo tiene).

### 7.2 Alumnos y ciclo por curso escolar

- Los **propios padres/tutores** dan de alta a su hijo, seleccionando colegio y clase.
- Modelo de datos: la relación alumno–clase **no es fija**, sino una **matrícula** (`Matricula`: alumno_id, clase_id) donde el curso escolar viene implícito a través de la `Clase` (que ya pertenece a un `CursoEscolar`). Esto separa la identidad del alumno (permanente) de su matrícula (una por curso), igual que en los sistemas de gestión académica reales.
- Al finalizar un curso escolar, basta con cambiar `CursoEscolar.estado` a `archivado`: como Avisos, Calendario, Deberes y Gastos cuelgan de una `Clase` de ese curso, quedan automáticamente en modo **solo lectura** sin tener que marcar cada registro individualmente. Se pueden seguir consultando como histórico, pero no se puede publicar, comentar ni modificar nada.
- Al iniciar un nuevo curso, se crean las nuevas `Clase` bajo el `CursoEscolar` nuevo y se pide a cada padre **confirmar la matrícula** de su hijo en la clase que corresponda (con opción de "sugerir continuidad" si sigue en el mismo colegio, para no repetir el proceso completo desde cero).

### 7.3 Privacidad y datos de menores

- Términos y condiciones y política de protección de datos obligatorias en el alta, dejando explícito que el consentimiento sobre los datos del menor lo presta su tutor legal.
- **Minimización de datos visibles:** aunque el padre registre el nombre completo del alumno (necesario para identificación real en caso de incidencia), en la interfaz solo se muestra a otros usuarios **nombre + inicial del primer apellido**.
- No se solicita fecha de nacimiento del alumno como dato de matrícula (con el curso/clase ya hay contexto suficiente). El **cumpleaños es opcional y vive fuera del registro del alumno**: cualquier padre puede crear voluntariamente un evento de calendario tipo "cumpleaños" para su hijo (alcance clase, marcado como recurrente anual), guardando solo **día y mes** — nunca el año, para no exponer la edad exacta del menor a través de un campo calculado. Si un padre quiere indicar la edad, lo hace como texto libre en el propio evento, no como dato derivado de una fecha de nacimiento real almacenada.
- No se permiten fotos de alumnos por defecto; si se habilitara en el futuro (ej. en Objetos perdidos), debe ser opt-in explícito y por foto, no un ajuste general.
- Los datos de un alumno se eliminan automáticamente al archivar un curso si el tutor no confirma continuidad, y el tutor puede solicitar el borrado completo desde la propia app (derecho al olvido) sin depender de gestión manual.
