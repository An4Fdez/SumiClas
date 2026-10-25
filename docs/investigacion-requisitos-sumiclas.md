# Investigación y definición de requisitos — SumiClas (app de coordinación escolar)

### 1. Contexto del proyecto

SumiClas es un proyecto de TFM que busca sustituir el grupo de WhatsApp de clase como canal de coordinación entre familias de un mismo colegio. El planteamiento cubre desde el inicio un **colegio completo** (no una sola clase), estructurado como Colegio → CursoEscolar → Clase → Alumno/Usuario, precisamente para que la Beta pueda evolucionar hacia un producto real sin rehacer el modelo de datos. El objetivo declarado no es solo entregar una demo académica, sino sentar una base que "en el futuro pueda llegar a ser funcional". El desarrollo tiene tiempo limitado, por lo que se ha definido un alcance de Beta acotado (ver RF más abajo) con una segunda ola de funcionalidades pospuesta a después de la entrega.

Decisiones ya tomadas que enmarcan esta investigación: acceso de alta abierto y autoservicio (sin administrador central, verificando el colegio contra un listado público oficial), stack Next.js (frontend, PWA) + .NET 10 Minimal API (backend) + PostgreSQL, y una identidad visual moderna, cercana y con modo oscuro.

### 2. Problema / Necesidad a resolver

El canal actual (grupo de WhatsApp de clase) tiene tres fallos estructurales, no coyunturales: (1) es un flujo lineal sin categorías, por lo que un aviso prioritario ("mañana no hay clase") se mezcla con conversación social y se pierde; (2) no hay memoria útil — la información se duplica porque nadie relee el histórico antes de preguntar; (3) no existe ningún resumen ni forma de consultar solo lo que interesa a cada padre en un momento dado (por ejemplo, "¿cuándo hay que entregar la autorización de la excursión?"), obligando a bucear manualmente en el chat. El impacto es tiempo perdido de las familias, información crítica (fechas de entrega, importes, horarios) que se pasa por alto, y fricción social cuando alguien pregunta algo "que ya se dijo".

### 3. Investigación competitiva y alternativas

* Competidor / Solución: **WhatsApp (statu quo actual)**
  * Funcionalidades clave: mensajería instantánea, ya instalada y adoptada por el 100% de las familias.
  * Ventajas: coste cero de adopción, no requiere convencer a nadie de instalar nada nuevo.
  * Limitaciones: sin estructura por tema, sin roles, sin resumen, sin calendario ni gestión de gastos; exactamente el problema descrito en la sección 2.

* Competidor / Solución: **TokApp School** ([tokapp.com/es/school](https://tokapp.com/es/school/), [tokappschool.com](https://tokappschool.com/))
  * Funcionalidades clave: mensajería con validez legal (equivalente a burofax), pagos online de excursiones/actividades, control de asistencia, avisos y tutorías, encuestas/votaciones, chatbot con IA, traducción automática, lectura en voz alta. Más de 5.500 centros clientes y 4M+ de descargas.
  * Ventajas: muy completo, pensado para relación oficial centro↔familia, con peso legal en las comunicaciones.
  * Limitaciones: **el centro educativo es el cliente y quien controla la comunicación** — no es una herramienta que un grupo de padres pueda adoptar por su cuenta sin que el colegio la contrate. No cubre coordinación *entre familias* (gastos compartidos para un regalo, objetos perdidos, ayuda mutua, dudas de deberes entre padres) porque su diseño es top-down (centro→familia), no peer-to-peer.

* Competidor / Solución: **Bloomz** ([bloomz.com](https://www.bloomz.com/parent-teacher-communication))
  * Funcionalidades clave: mensajería multicanal (app/email/SMS/voz) con traducción a 250+ idiomas, formularios y permisos digitales, alerta temprana de bienestar del alumno (Bloomz Thrive), portafolios de estudiante, integración con el sistema de gestión académica del centro (SIS).
  * Ventajas: plataforma muy robusta, pensada para distritos escolares enteros, con foco en bienestar y accesibilidad.
  * Limitaciones: mismo patrón que TokApp — el cliente es la institución (superintendentes, directores, centros), no un grupo de padres autoorganizado; sobredimensionada para el caso de uso de "una clase quiere coordinarse mejor".

* Competidor / Solución: **ClassDojo / Remind** (referencia rápida, mismo patrón)
  * Funcionalidades clave: comunicación profesor↔familia, gestión de comportamiento/aula (ClassDojo), mensajería masiva (Remind).
  * Ventajas: adopción muy extendida en EE. UU., interfaz simple.
  * Limitaciones: de nuevo, canal profesor→padres, no coordinación horizontal entre familias; sin gastos compartidos, objetos perdidos ni ayuda mutua.

**Conclusión de la investigación competitiva:** todas las alternativas relevantes son plataformas *institucionales* que requiere que el colegio las contrate y las imponga de arriba a abajo. Ninguna resuelve el caso de uso de un grupo de padres que quiere organizarse *entre sí*, sin depender de que el centro adopte nada. Ese es el hueco real que ocupa SumiClas: puede nacer y crecer desde una sola clase, sin necesidad de convencer a la dirección del colegio, y cubre necesidades que estas plataformas no contemplan (gastos compartidos tipo regalo de profesora, objetos perdidos, ayuda entre familias, resumen semanal orientado al padre y no al centro).

### 4. Insights de entrevistas e investigación usuario/negocio

No se han realizado aún entrevistas formales — lo que sigue son las necesidades ya declaradas por el promotor del proyecto en su rol de padre/usuario del grupo de WhatsApp actual, más una propuesta de a quién entrevistar antes de cerrar el alcance definitivo de la Beta. Es importante no tratar esto como investigación validada: son hipótesis de partida que conviene contrastar.

* Perfil entrevistado: **Promotor del proyecto (padre/tutor, usuario actual del grupo de WhatsApp de clase)**
  * Principales insights: la saturación de mensajes y la duplicidad de información son el dolor más citado; no existe hoy ninguna forma de filtrar por tema ni de obtener un resumen.
  * Necesidades explícitas: separación por temas (avisos, calendario, deberes, gastos...), resumen semanal, un asistente que responda preguntas puntuales (fecha/importe de una excursión) buscando en lo ya publicado.
  * Necesidades implícitas: confianza en que los avisos "oficiales" sean fiables (de ahí la necesidad de roles con permisos de publicación), y que gestionar dinero entre padres (regalos, gastos de clase) no genere conflictos por falta de trazabilidad de quién ha pagado.

**Perfiles pendientes de entrevistar antes de cerrar el diseño final**, con la justificación de por qué cada uno aporta algo que el promotor solo no puede validar:

* **Delegados de padres de otras clases/colegios** — para confirmar si el rol de "primer registrado = delegado provisional" (definido en el modelo de acceso) es realmente asumible sin fricción, o si genera rechazo ("¿por qué tengo que ser yo el responsable?").
* **Profesores/tutores** — para saber si estarían dispuestos a publicar avisos oficiales en una herramienta externa al centro, y si hay algo que legalmente no podrían/deberían compartir fuera de los canales oficiales del colegio.
* **AMPA de al menos un centro** — para validar el supuesto de que necesitan publicar avisos a nivel de todo el colegio (no solo una clase), y entender si ya usan alguna herramienta institucional (tipo las analizadas en el punto 3) con la que SumiClas tendría que convivir, no competir.
* **Padres con hijos en distintas clases/cursos (hermanos)** — para validar que el modelo "un usuario pertenece a varias clases" cubre bien su caso real de uso.
* **Un pequeño grupo de padres "no early adopters"** (los que menos usan apps nuevas) — para entender la barrera real de adopción: si no consiguen que se registre toda la clase, el producto no sustituye a WhatsApp, convive con él, y pierde su razón de ser.

### 5. Requisitos funcionales

*Núcleo de la Beta (imprescindible):*

* RF1: Un usuario puede crear un colegio seleccionándolo de un listado público oficial de centros educativos (no texto libre), evitando altas de colegios inexistentes.
* RF2: Un usuario puede crear una clase dentro de un colegio verificado; el primer usuario que la crea queda como delegado provisional de esa clase.
* RF3: Un delegado puede ceder su rol a otro usuario de la misma clase en cualquier momento.
* RF4: Un padre/tutor puede matricular a su hijo (alumno) en una clase concreta de un curso escolar activo.
* RF5: El sistema distingue roles por clase (profesor/tutor, delegado, participante) y un rol de admin de colegio (AMPA/dirección) con alcance sobre todo el centro.
* RF6: Solo los roles con permiso de publicación oficial (profesor, delegado, admin colegio) pueden publicar en Avisos y en eventos de Calendario con alcance oficial.
* RF7: Los Avisos y eventos de Calendario pueden publicarse con alcance "colegio" (visible en todas las clases) o "clase" (visible solo en una).
* RF8: Cualquier padre puede crear voluntariamente un evento de cumpleaños de su hijo en el calendario (solo día y mes, sin año, sin vincularlo al registro del alumno).
* RF9: Los Deberes se publican en formato "pizarra por día/asignatura" con un hilo corto de comentarios para dudas puntuales, no como chat libre.
* RF10: El sistema genera un resumen semanal automático (vía LLM) a partir de lo publicado esa semana en Avisos, Calendario y Deberes.
* RF11: Un usuario puede consultar cursos escolares anteriores en modo solo lectura (sin poder publicar, comentar ni editar).
* RF12: Al iniciar un nuevo curso escolar, cada padre debe confirmar la matrícula de su hijo (con opción de continuidad sugerida si sigue en el mismo colegio).
* RF13: La landing pública (no autenticada) presenta el producto, funcionalidades y reseñas, con el login como opción secundaria y no como pantalla de entrada.
* RF14: La app funciona como PWA instalable, con notificaciones push y modo oscuro/claro seleccionable por el usuario.

*Segunda ola (si el tiempo de desarrollo lo permite):*

* RF15: Gastos de clase en modo *tracking*: registro de quién participa en un gasto compartido y quién ha marcado que ha pagado, sin mover dinero real dentro de la app.
* RF16: Objetos perdidos: publicación de objetos extraviados con descripción y estado (buscado/encontrado).

*Fuera de alcance de la Beta (backlog explícito):*

* RF17: Ayuda entre familiares (peticiones puntuales tipo "¿alguien puede recoger a mi hijo?").
* RF18: Buscador conversacional con IA sobre todo el histórico (más allá del resumen semanal).
* RF19: Pagos reales integrados en Gastos de clase.

### 6. Requisitos no funcionales

* RNF1 (Privacidad/legal): cumplimiento RGPD/LOPDGDD reforzado por tratarse de datos de menores; minimización de datos de alumnos (nombre + inicial de apellido visible, sin fecha de nacimiento, sin fotos por defecto); consentimiento del tutor legal explícito en el alta.
* RNF2 (Seguridad): autenticación con JWT, control de acceso por rol y por clase/colegio a nivel de API (no solo de interfaz), borrado de datos de alumno accesible por el propio tutor (derecho al olvido).
* RNF3 (Rendimiento/UX): landing con carga rápida (idealmente estática/SSG), diseño mobile-first ya que el uso mayoritario será desde el móvil de los padres.
* RNF4 (Accesibilidad): contraste mínimo AA en ambos modos de color, tamaño de fuente legible sin zoom, navegación completa por teclado.
* RNF5 (Escalabilidad): modelo de datos preparado para múltiples colegios y cursos escolares desde el diseño inicial, aunque la Beta se despliegue en uno solo.
* RNF6 (Disponibilidad): hosting en PaaS con soporte de contenedores (Render/Fly.io; actualización 2026-10-24: se desplegará en Azure), sin necesidad de alta disponibilidad crítica para una Beta.
* RNF7 (Mantenibilidad): migraciones versionadas con EF Core desde el primer commit, para trazabilidad del modelo de datos.
* RNF8 (Coste): uso de LLM acotado a un resumen semanal por clase (contexto pequeño, sin RAG complejo) para mantener el coste de IA controlado durante la Beta.

### 7. Restricciones, dependencias y riesgos

Restricción principal: tiempo de desarrollo limitado por ser un TFM, lo que obliga al alcance acotado ya reflejado en la sección 5. Dependencia técnica no verificada aún: la existencia y accesibilidad real (API o dataset descargable, actualización, cobertura autonómica) de un listado público de centros educativos para RF1 — esto debería confirmarse técnicamente antes de comprometerse a esa solución de verificación. Riesgo de adopción: SumiClas compite por la atención de los padres con WhatsApp, que ya está instalado y no requiere ninguna acción; si no se consigue una masa crítica de la clase registrada, el producto no sustituye al grupo, convive con él y pierde su propósito. Riesgo legal/de confianza en Gastos de clase: en cuanto hay dinero de por medio, aunque sea solo tracking, hay expectativa de fiabilidad — de ahí la decisión de no mover dinero real en la Beta. Dependencia de negocio: se necesita al menos un colegio/clase piloto real dispuesto a probarlo para que la validación tenga valor más allá de una demo. Riesgo de alcance: el rol de AMPA (a nivel de colegio completo) puede requerir validación adicional con un caso real antes de darlo por cerrado (ver sección 4).

### 8. Hipótesis a validar

* Los padres reducirán de forma medible su dependencia del grupo de WhatsApp si tienen una alternativa estructurada por temas (no solo "la probarán una vez").
* El resumen semanal automático es la funcionalidad que más valor percibido aporta frente a navegar manualmente por secciones.
* El modelo de alta autoservicio (colegio verificado por listado público + delegado provisional cedible) genera suficiente confianza sin necesidad de un administrador humano que apruebe altas.
* Un padre está dispuesto a asumir voluntariamente el rol de delegado de clase al ser el primero en registrarse, sin que eso se perciba como una carga no pedida.
* El formato "pizarra + comentarios cortos" en Deberes es suficiente y no empuja a los padres a volver a un chat libre por sentirlo más rápido.

### 9. Valor esperado y métricas de impacto

Valor para las familias: menos tiempo buscando información dispersa, menor probabilidad de perderse una fecha o autorización importante, y una forma más ordenada de coordinar gastos y ayuda mutua. Como métricas de validación de la Beta (más relevantes para un TFM que métricas de negocio clásicas): porcentaje de familias de la clase/colegio piloto que completan el registro sobre el total de familias existentes; porcentaje de avisos/eventos que se publican en SumiClas frente a los que se siguen enviando por WhatsApp durante el periodo de prueba (proxy de sustitución real); tasa de uso semanal recurrente (retención), no solo de alta; número de preguntas resueltas mediante el resumen/búsqueda de IA sin que el padre tenga que preguntar manualmente; y una medida cualitativa de satisfacción (encuesta corta a las familias participantes al cierre de la Beta). Como posible vía de sostenibilidad a futuro (no relevante para la Beta en sí, pero sí para la sección de conclusiones/futuro del TFM): modelo freemium por colegio completo, con funcionalidades avanzadas (multi-centro, pagos integrados, panel para AMPA) de pago para centros que quieran ir más allá de lo que cubre la versión gratuita entre familias.

### 10. Oportunidades futuras e innovaciones

Ayuda entre familiares y buscador conversacional con IA (ya identificados como fase 2), integración de pagos reales para Gastos de clase una vez validada la confianza en el tracking, soporte multi-colegio real para que una misma AMPA administre varios centros, traducción automática de avisos para familias no hispanohablantes (patrón que tanto TokApp como Bloomz ya resuelven y que sería relevante en colegios con alta diversidad), opción de lectura en voz alta para accesibilidad (mismo patrón detectado en TokApp), y una eventual interoperabilidad con las plataformas institucionales que ya usa el centro (por ejemplo, importar el calendario oficial desde Educamos/Alexia/Séneca si el colegio ya las usa) en vez de competir con ellas — reforzando el posicionamiento de SumiClas como la capa *entre familias* que hoy no cubre ninguna plataforma institucional.
