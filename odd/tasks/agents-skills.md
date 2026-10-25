# Dividir los AGENTS.md en skills

## Objetivo
Los `AGENTS.md` han crecido hasta 139 KB (backend), 47 KB (frontend) y 11 KB (raíz): cualquier agente que los carga paga todo ese texto aunque solo vaya a tocar una cosa. Se reparte el contenido en **skills** (un `SKILL.md` por tema, con su descripción de cuándo cargarlo) y los `AGENTS.md` quedan cortos y **referencian** esas skills para que se carguen a la hora de desarrollar.

## Decisiones
- Las skills viven en `.claude/skills/<nombre>/SKILL.md` (dentro del repo y versionadas): Claude Code las descubre solas, y los `AGENTS.md` las citan por ruta, así que cualquier otro agente puede leerlas también.
- Se mueve el texto **tal cual** (sin reescribirlo ni resumirlo) para no perder información; solo se retocan los encabezados, las referencias cruzadas («ver sección "X"» pasa a decir en qué skill está X) y lo que queda desfasado.
- El **estado y el historial** (casos de uso hechos, huecos conocidos por caso de uso, verificaciones en navegador por pantalla) no son convenciones: van en skills de notas (`backend-status-notes`, `frontend-screens`) para que no estorben al leer las reglas.
- Los `AGENTS.md` conservan lo que se debe leer **siempre**: qué es cada parte, estructura mínima, reglas obligatorias, tabla de skills y «Pendiente de decidir».
- Los nombres de las skills van en inglés y minúsculas con guiones; el contenido, en español como el resto de la documentación.

## Tareas
- [x] S1 Skills del backend (reparto de `apps/backend/AGENTS.md`).
- [x] S2 Skills del frontend (reparto de `apps/frontend/AGENTS.md`).
- [x] S3 `AGENTS.md` nuevos (raíz, backend y frontend) con su tabla de skills y las referencias cruzadas arregladas.
- [x] S4 Comprobación: ninguna línea de contenido se pierde, enlaces y descripciones válidos. La revisión nativa no fue posible (ver el progreso).

## Criterios de aceptación
- Cada línea de los dos `AGENTS.md` grandes está en algún sitio (skill o `AGENTS.md` nuevo), salvo las que se retiran de forma explícita y se listan aquí.
- Cada `AGENTS.md` dice qué skill cargar para cada tipo de tarea, con ruta.
- Cada `SKILL.md` tiene `name` (igual al nombre de su carpeta) y `description` que dice cuándo usarla.
- Ningún `AGENTS.md` ni skill apunta a una sección que ya no existe.

## Progreso y evidencia
- Commit: `2fbea05`.
- Lectura completa de los tres `AGENTS.md` y reparto por rangos de líneas con scripts que mueven el texto tal cual (no se reescribió ni resumió nada); cobertura comprobada línea a línea contra la versión de HEAD.
- **Backend (139 KB → 8,4 KB de `AGENTS.md`)**, en 15 skills: `backend-architecture` (módulos, capas, mapa de módulos, nomenclatura), `backend-domain` (agregados, excepciones de dominio, validaciones, `IClock`, ids), `backend-use-cases` (CQRS, transacción, registro por convención), `backend-cross-module` (`IQueryBus`, eventos, `IUserProvider`), `backend-endpoints` (capa `api`, OpenAPI, versionado, validación de longitud), `backend-errors` (excepciones y estado HTTP, `ApiExceptionHandler`, logging), `backend-read-queries` (GET y paginación por cursor), `backend-specifications` (especificaciones y repositorios), `backend-persistence` (EF Core, migraciones, mapeos), `backend-auth`, `backend-authorization`, `backend-territorial-catalog`, `backend-weekly-summary`, `backend-testing` y `backend-status-notes` (endpoints hechos, huecos conocidos por caso de uso, cobertura y pendientes: referencia, no reglas).
- **Frontend (47 KB → 4,4 KB de `AGENTS.md`)**, en 5 skills: `frontend-architecture`, `frontend-api-integration`, `frontend-auth-session`, `frontend-ui-patterns` y `frontend-screens` (notas por pantalla).
- **Raíz (11 → 12,5 KB)**: se añade la sección «Skills del proyecto» y las dos referencias a «Autenticación de la API» y «Resumen semanal con IA» pasan a decir la skill.
- Cada `AGENTS.md` de backend y frontend lleva una tabla «Si vas a… → Lee la skill», una lista corta de las reglas que más importan y lo que se debe leer siempre: la regla de `var` (backend), el estilo y la verificación, y «Pendiente de decidir» (con sus referencias ya apuntando a las skills).
- Las descripciones de las skills van entre comillas dobles (llevan «: » y en YAML sin comillas no serían válidas); `name` coincide con la carpeta en las 20. Las skills suman unos 190 KB, que se cargan solo cuando una tarea las necesita; lo que sí se carga siempre son las 20 descripciones (unos 5,7 KB).
- Referencias cruzadas: `ver "Base de datos"` y similares pasan a `ver "Base de datos" (skill `backend-persistence`)` (18 etiquetas); las que decían «más arriba», «abajo» o «más abajo» sobre algo que ahora está en otra skill, o apuntaban a otro `AGENTS.md`, se reescribieron a mano (8 sitios).
- **Retirado de forma explícita** (5 trozos que ya no eran ciertos o estaban repetidos): backend, «Estado actual (importante)» (líneas 5-7: decía que la API aún no tenía endpoints ni `Application`, y hoy hay decenas); frontend, el párrafo de la línea 9 («el resto de pantallas siguen con mocks», ya no es cierto), el encabezado «Datos y backend real» (su contenido está repartido en tres skills) y la viñeta «Pendiente de decidir» de la línea 75 (repetía las dos decisiones que ya lista el final del fichero); y el `>` suelto que separaba dos citas.
- Comprobación (`verify_split`): 414 trozos de línea de los dos ficheros grandes; todos están en alguna skill o `AGENTS.md` salvo 13: los 5 retirados de arriba y los 8 reescritos a mano.
- **Revisión nativa: no se pudo hacer.** Con todo el cambio y con solo los tres `AGENTS.md` y la ficha, el arranque de la revisión se rechaza con `lens_context_budget_exceeded` (la evidencia completa de un candidato no se recorta, y aquí pesa sobre todo lo que se borra: unos 186 KB de los dos `AGENTS.md` grandes, que ya están íntegros en las skills). Partir el cambio en candidatos más pequeños no es posible sin partir las propias skills. Lo que sustituye a la revisión: la comprobación de cobertura línea a línea (414 trozos, 13 diferencias todas explicadas), la comprobación de que ninguna referencia apunta a una skill que no existe (`check_refs`: 0; solo quedan dos citas a algo que no es una sección, un texto de una ficha y una frase «asignación de delegados» que remite a una viñeta de la misma skill) y la lectura final de los dos `AGENTS.md` nuevos.
- Falta (fuera de esta tarea): el `AGENTS.md` raíz dice que `apps/test/backend` está vacía y que `apps/test/AGENTS.md` está pendiente, cuando ya hay más de mil tests en `apps/test/backend`; es un resto de antes que no se ha tocado.

## Siguiente paso
Que el usuario dé el visto bueno y commitear (un solo commit: skills y `AGENTS.md`).
