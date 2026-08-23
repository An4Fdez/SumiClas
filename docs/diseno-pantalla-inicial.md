# Diseño de la pantalla inicial (landing) — Beta TFM

## 1. Nombre y marca (propuesta de trabajo)

Nombre del producto: **SumiClas**. Cambiar el nombre no afecta a la estructura descrita aquí, solo al copy.

## 2. Paleta de color y modo oscuro

Buscas algo moderno, intuitivo, cercano pero no llamativo — esto descarta azules corporativos fríos y colores saturados tipo "app de gestión". Propongo una base neutra cálida (grises con un toque cálido, no gris frío/azulado) con un color de acento cálido y suave como color de marca, y un segundo acento solo para prioridad/urgencia (Avisos importantes), para no diluir su significado usándolo en todas partes.

**Modo claro:**
- Fondo: `#FAF8F5` (blanco roto cálido, no blanco puro — descansa más la vista)
- Superficie/tarjetas: `#FFFFFF`
- Texto principal: `#2B2724` (casi negro, cálido, no negro puro)
- Texto secundario: `#6B655F`
- Color de marca / acento principal: `#C97B4A` (terracota suave — cercano, cálido, sin ser chillón)
- Acento secundario (éxito/confirmación): `#5B8A6B` (verde salvia)
- Alerta / prioridad en Avisos: `#C25450` (rojo teja, con moderación, solo para lo verdaderamente urgente)
- Bordes/separadores: `#E8E2DB`

**Modo oscuro:**
- Fondo: `#1C1A18`
- Superficie/tarjetas: `#262320`
- Texto principal: `#F2EEE8`
- Texto secundario: `#B0A99F`
- Color de marca / acento principal: `#D98F5F` (mismo terracota, aclarado para contraste sobre fondo oscuro)
- Acento secundario: `#74A886`
- Alerta / prioridad: `#D97873`
- Bordes/separadores: `#38332E`

Regla general: el acento de marca (terracota) se usa para botones primarios, enlaces y elementos interactivos; el rojo teja se reserva exclusivamente para prioridad alta en Avisos, para que cuando el usuario lo vea sepa que es importante de verdad (si se usa el mismo tono para todo, pierde fuerza). El toggle de modo oscuro/claro/automático (según el sistema) va en la cabecera, siempre visible, no escondido en un menú de ajustes.

## 3. Estructura de la landing (de arriba a abajo)

La landing es la ruta raíz (`/`) de la web app. El login **no** es la home: es una ruta aparte (`/acceso`) enlazada desde la landing, nunca lo primero que ve el usuario.

### 3.1 Cabecera (header, fija/sticky)

- Logo + nombre a la izquierda.
- A la derecha: toggle de modo oscuro/claro, y dos acciones: "Iniciar sesión" (texto, discreto) y "Buscar mi colegio" (botón relleno, con el color de acento — este es el CTA principal, no "Registrarse" a secas, porque conecta directamente con el flujo de verificación de colegio que ya definimos).
- En móvil: el toggle y "Iniciar sesión" colapsan en un menú hamburguesa; el botón "Buscar mi colegio" se mantiene siempre visible porque es la acción que quieres maximizar.

### 3.2 Hero (primera pantalla visible, sin scroll)

- Titular corto y concreto, no genérico tipo "la mejor app para tu familia". Ejemplo: **"Todo lo del cole de tus hijos, en un solo sitio. Sin perderte nada entre 200 mensajes de WhatsApp."** El segundo tramo de la frase es importante: nombra el dolor real del usuario, no solo el beneficio abstracto.
- Subtítulo de una línea explicando el mecanismo, no solo el beneficio: "Avisos, calendario, deberes y gastos de clase organizados por tema — con un resumen semanal automático para no tener que leerlo todo."
- Un único CTA primario grande: **"Buscar mi colegio"** (lleva al flujo de verificación contra el listado oficial). Nada de dos CTAs compitiendo en el hero (eso diluye la decisión).
- Debajo del CTA, una línea de micro-confianza, en texto pequeño: algo tipo "Gratis durante la Beta · Verificamos tu colegio antes de darte acceso" — esto responde a la objeción implícita de "¿esto es de fiar/no es spam?" sin que el usuario tenga que buscarla.
- A la derecha (o debajo en móvil), en vez de una foto de stock genérica de una familia sonriente (evítalo, se ve falso), una captura simplificada/mockup del propio producto mostrando la pantalla de Avisos o Calendario con datos de ejemplo — esto genera más confianza porque el usuario ve exactamente lo que va a usar.

### 3.3 Bloque de dolor → solución (justo debajo del hero)

Una franja corta, dos columnas: a la izquierda "Así es ahora" con 3 líneas breves representando el problema (mensajes duplicados, información perdida, avisos importantes enterrados en el ruido); a la derecha "Así es con SumiClas" con las mismas 3 situaciones resueltas. Este contraste directo funciona mejor que una lista abstracta de features, porque el padre se reconoce en el problema antes de que le vendas la solución.

### 3.4 Funcionalidades (grid de tarjetas)

Una tarjeta por sección de la app (Avisos, Calendario, Deberes, Gastos de clase, Objetos perdidos, Resumen semanal con IA), cada una con: icono simple (no ilustración recargada), nombre de la sección, y una frase de una línea de beneficio concreto — no descripción técnica. Ejemplo para Gastos de clase: no "Gestiona los gastos compartidos de la clase", sino "Sabrás exactamente cuánto te toca pagar del regalo de la profe, sin preguntarlo en el grupo." El resumen semanal con IA merece una tarjeta algo más grande o destacada dentro del grid, porque es el diferencial frente a un simple tablón de anuncios — puedes incluir ahí un mini-ejemplo de texto: *"Esta semana: excursión al museo el jueves (traer autorización firmada), examen de mates el viernes, cumpleaños de Martina el sábado."*

### 3.5 Prueba social (reseñas)

Sección con 3-4 tarjetas de reseña: nombre de pila + inicial (coherente con tu propia política de minimización de datos, ni siquiera en marketing conviene exponer apellidos completos), un texto corto (2-3 líneas) y una valoración en estrellas. Para los datos dummy, evita reseñas genéricas tipo "¡Genial, la recomiendo!" — hazlas específicas y creíbles, por ejemplo: *"Antes se me pasaban las autorizaciones de las excursiones entre tanto mensaje. Ahora las veo directas en Avisos." — Laura M., madre de 2º de Primaria.* Encima de las tarjetas, un dato agregado tipo "4.8/5 · basado en X familias de la Beta" (con un aviso discreto en el código/comentario de que hay que sustituir esto por datos reales en cuanto existan, para no dejarlo como dato falso en producción).

### 3.6 Cómo funciona (pasos)

3 pasos numerados, simples: 1) Busca tu colegio (verificado) 2) Únete a la clase de tu hijo con el código de invitación de la clase 3) Empieza a ver avisos, calendario y deberes al momento. Esto reduce la incertidumbre de "¿qué tengo que hacer exactamente para usar esto?" antes de que el usuario pulse el CTA.

### 3.7 CTA final + FAQ corta

Repetición del CTA principal ("Buscar mi colegio") antes del footer, para capturar a quien ha llegado hasta abajo leyendo. Debajo, un acordeón de 3-4 preguntas frecuentes cortas que resuelven las objeciones típicas antes de que abandonen: "¿Esto sustituye al grupo de WhatsApp del cole?", "¿Es gratis?", "¿Qué pasa con los datos de mi hijo?" (enlaza directamente a la política de privacidad — refuerza la confianza en el tema sensible de menores), "¿Puede unirse el profesor o el AMPA?".

### 3.8 Footer

Enlaces a Política de privacidad, Términos y condiciones, contacto, y de nuevo el enlace discreto a "Iniciar sesión" para quien ya tiene cuenta y ha llegado hasta el final sin verlo arriba.

## 4. Comportamiento responsive / mobile-first

Los padres van a usar esto casi siempre desde el móvil, así que diseña primero la versión móvil y expande a escritorio, no al revés. En móvil: hero a una columna, CTA a ancho completo y fácil de pulsar con el pulgar, grid de funcionalidades a una columna (o carrusel horizontal deslizable si prefieres compactarlo), reseñas en carrusel deslizable en vez de grid. Como es una PWA, después de la primera visita o tras una acción de compromiso (por ejemplo, al completar el registro) puedes mostrar el banner nativo de "Añadir a pantalla de inicio" — pero no en el primer segundo de la primera visita, porque en la landing todavía no ha visto valor y es más probable que lo descarte.

## 5. Dónde vive el login

`/acceso` como ruta separada, con un formulario simple (email/magic link o email+contraseña, según lo que decidáis) y, debajo, un enlace de vuelta "¿Aún no tienes cuenta? Busca tu colegio" — que reconduce al flujo de la landing en vez de duplicar el formulario de registro ahí mismo. Esto mantiene una única entrada al flujo de alta (coherente con el modelo de verificación de colegio ya definido) en lugar de tener dos caminos distintos de registro.

## 6. Notas de implementación (Next.js)

La landing es una página estática/SSG (no necesita datos del backend salvo, quizás, el contador de reseñas si en el futuro es real) — esto la hace rápida de cargar, importante porque es la primera impresión. El resto de la app (una vez logueado) puede ser CSR/con datos dinámicos sin problema. El toggle de modo oscuro conviene implementarlo con la preferencia `prefers-color-scheme` como valor por defecto y permitir que el usuario la override, guardando su elección (en cookie o en el propio perfil de usuario una vez logueado, no en localStorage tal y como se comentó).

## 7. Accesibilidad

Contraste AA mínimo entre texto y fondo en ambos modos (los tonos propuestos arriba cumplen, pero conviene verificarlo con una herramienta al implementar), tamaño de fuente base no inferior a 16px para que sea legible sin zoom en móvil, y foco de teclado visible en todos los elementos interactivos — relevante también para el argumento de calidad en la memoria del TFM.
