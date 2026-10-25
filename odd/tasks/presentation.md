# Presentación del proyecto en PowerPoint

## Objetivo
Una presentación `.pptx` de SumiClas para la entrega del TFM: sencilla, bonita y resumida, con los colores del modo oscuro de la app, tono natural y cercano (profesional pero no serio ni aburrido), que explique el proyecto, enseñe algunas pantallas y resuma las tecnologías.

## Decisiones
- Formato `.pptx` (lo pidió el usuario), 16:9, hecho con `pptxgenjs` desde una carpeta temporal fuera del repositorio (no se toca ningún `package.json` del proyecto).
- Paleta del modo oscuro de la app: fondo `slate-900` (`0F172A`), tarjetas `slate-800` (`1E293B`), bordes `slate-700` (`334155`), texto `slate-300`/blanco, y acentos pastel de la propia app: `brand-400` (`59BCFF`), `rose-400` (`FF6F97`), `mint-300` (`6EE9B4`), `violet-300` (`C4B5FD`) y `sun-300` (`FCD34D`). Motivo visual: tarjetas redondeadas, iconos en círculos de color y manchas translúcidas suaves; capturas dentro de un marco de móvil.
- Tipografía segura en cualquier PowerPoint: Cambria para títulos (recuerda a la serif del logo) y Calibri para el texto.
- Capturas reales de la web (modo oscuro, tamaño móvil) con las cuentas de demostración del README, sacadas con Chrome sin cabeza contra la web y la API locales. La tarjeta del resumen con IA se oculta en la captura del panel (sin clave de la IA solo mostraría un error) y esa función se explica con una tarjeta de ejemplo dibujada y rotulada como ejemplo.
- Unas 12 diapositivas, con notas del orador en español natural.
- Entregables: `docs/presentacion/SumiClas-presentacion.pptx` y las capturas en `docs/presentacion/capturas/`.

## Tareas
- [x] D1 Capturas de pantalla (panel, avisos, calendario, deberes, gastos y objetos perdidos) en modo oscuro.
- [x] D2 Generar el `.pptx` con las 12 diapositivas y las notas.
- [x] D3 Revisión visual diapositiva a diapositiva (renderizado con LibreOffice) y corrección de desbordes, solapes y contraste.
- [x] D4 Diapositiva nueva «Y más adelante» (entre «Lo que viene» y el cierre, que pasa a ser la 13) con los planes de futuro que ya recogen los documentos: ayuda entre familias, buscador con IA sobre el histórico, pagos reales en los gastos, avisos de colegio con roles de profesorado y administración, traducción de avisos y lectura en voz alta, y varios colegios / AMPA con calendario oficial importado.
- [x] D5 Las capturas se rehacen en formato web (ventana de escritorio de unos 1440x900, modo oscuro) porque la app se usa hoy en un navegador y no como un móvil.
- [x] D6 Las diapositivas de pantallas pasan de marcos de móvil a marcos de navegador: dos capturas grandes por diapositiva (panel y avisos; calendario y gastos o deberes, la que mejor se vea).
- [x] D7 Terraform deja de aparecer como futuro (decisión del usuario: se presenta como hecho): sale de «Lo que viene» y entra como «Nube: Azure y Terraform» en «Cómo está hecho» y, si encaja, como tarjeta en «Bien cimentado»; las notas del orador se actualizan.

## Guion (la 12 «Y más adelante» se añadió después: el cierre pasa a ser la 13)
1 Portada · 2 El problema (el caos del grupo de WhatsApp) · 3 La idea (SumiClas en una frase) · 4 Para quién es (familias, delegados y menores protegidos) · 5 Lo que se puede hacer (6 tarjetas) · 6 y 7 Pantallas (panel, avisos y calendario; deberes, gastos y objetos perdidos) · 8 Resumen semanal con IA · 9 Cómo está hecho (stack) · 10 Bien cimentado (cifras reales: 18 módulos, 1.253 tests, 18.197 colegios) · 11 Lo que viene (despliegue en Azure con Terraform, segundo tutor por invitación, notificaciones, editar y borrar contenido) · 12 Gracias y cuenta de prueba.

## Criterios de aceptación
- El archivo se abre en PowerPoint (validado lo que se pueda con LibreOffice) sin texto desbordado, solapes ni bajo contraste; ninguna diapositiva es solo viñetas.
- Todas las cifras y afirmaciones salen del repositorio o de lo comprobado en esta sesión; el contenido de las capturas es de la demo (datos inventados).
- Ninguna clave real en ningún archivo.

## Ruta
Un escritor delegado hace D1–D3 con las herramientas en una carpeta temporal (instalación de `pptxgenjs` y similares fuera del repo); yo reviso las imágenes renderizadas. Commit tras el «sí» del usuario.

## Progreso y evidencia
- D1: 6 capturas PNG (780x1688, modo oscuro, tamaño móvil) en `docs/presentacion/capturas/`, sacadas con Chrome sin cabeza por el protocolo DevTools contra la web y la API locales con la cuenta demo `andres.cabrera` (un solo hijo y avisos sin leer; con dos hijos el encabezado se solapaba con el botón de tema); la tarjeta del resumen con IA oculta en el panel; nada inventado. D2: `docs/presentacion/SumiClas-presentacion.pptx` (980 KB, 12 diapositivas, notas del orador en cada una, 4 secciones, tema aplicado). D3: revisión de las 12 diapositivas renderizadas con LibreOffice, vistas una a una por el orquestador: sin desbordes ni solapes, buen contraste; el escritor corrigió solapes en las diapositivas 4, 5, 8 y 12. Cifras de la diapositiva 10 comprobadas: 18 módulos, 1.253 tests y 18.197 colegios (consulta de solo lectura). Hallazgo corregido: el servidor de Vite sirve por HTTP en el 4004 (`curl` http 200, https falla; `vite.config.ts` no activa HTTPS) y el `Readme.md` y el `AGENTS.md` decían `https://localhost:4004`. No verificado: cómo se ve en PowerPoint real (solo LibreOffice) ni `validate.py` (no hay Python). Revisión nativa: no solicitada.
- D4: diapositiva nueva «Y más adelante» (la 12; el cierre pasa a ser la 13) con 6 ideas ya escritas en los documentos (ayuda entre familias, buscador con IA, pagos reales, avisos de colegio con roles de profesorado y administración, traducción y lectura en voz alta, varios colegios y AMPA con calendario oficial); la tarjeta 6 se acortó para que no se saliera. `.pptx` de 1,0 MB y 13 diapositivas; vistas por el orquestador las 10, 12 y 13 (el escritor vio también la 5 y la 11).
- D5-D7: capturas rehechas en formato web (2145x1215, modo oscuro, ventana de escritorio de 1440x810; recortada la franja blanca de 15 px que deja Chrome) con Elena (panel), Lucía (avisos, calendario y deberes) y Marcos (gastos); las diapositivas 6 y 7 pasan a marcos de navegador con dos ventanas por diapositiva (panel y avisos; calendario y deberes) y un recorte horizontal del 13 % para ganar legibilidad (el texto de párrafo sigue siendo pequeño; alternativa: una ventana por diapositiva, 4 diapositivas). Terraform presentado como hecho por decisión del usuario: sale de «Lo que viene» (quedan tres tarjetas), entra como grupo «Nube: Azure y Terraform» en «Cómo está hecho» (cuadrícula 3x2) y como tarjeta «Nube como código» en «Bien cimentado»; notas actualizadas, sin recursos, regiones ni URL. Vistas por el orquestador las diapositivas 6, 7, 9, 10 y 11. `.pptx` de 745 KB, 13 diapositivas. Aviso: el repositorio no tiene Terraform y el `Readme.md` y el `AGENTS.md` dicen que el despliegue está «previsto»; no se han tocado para no afirmar algo que el código no tiene. Los servidores de desarrollo (web 4004, API 7064) estaban parados cuando se rehicieron las capturas: se arrancaron los del escritor y se pararon al terminar./

## Siguiente paso
Commit del `.pptx` (13 diapositivas), las capturas y la corrección de HTTP.
