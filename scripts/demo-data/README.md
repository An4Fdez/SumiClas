# Datos de demostración

Cargador de datos de prueba para presentar la web completa. Usa la **API real** (registro, cursos, clases, hijos, matrículas, asignaturas, eventos, avisos, objetos perdidos, deberes y gastos), así que se aplican las mismas reglas que en la aplicación. No lleva dependencias: Node 20 o superior.

**Pensado para desarrollo local.** Por defecto se niega a hablar con una API que no sea `localhost`. Las cuentas son falsas y su contraseña (`Demo1234`) es pública a propósito.

**Contra una API remota (por ejemplo el App Service de pruebas):** hay que pedirlo expresamente con `--allow-remote`, solo vale con `https`, mantiene la contraseña de la demo y pide escribir el nombre del servidor para confirmar. Ojo: en un entorno remoto cualquiera que conozca el `Readme` podrá entrar con esas cuentas. `reset` no funciona en remoto, porque borra con `docker exec` en la base de datos local; para limpiar un entorno remoto hay que borrar a mano.

```bash
node scripts/demo-data/seed.mjs load --api=https://<tu-app>.azurewebsites.net --allow-remote
node scripts/demo-data/seed.mjs verify --api=https://<tu-app>.azurewebsites.net --allow-remote
```

Antes hay que tener cargado el catálogo de colegios en esa base de datos (los colegios de la demo son del catálogo oficial). La API de producción limita los intentos de acceso, así que la carga espera cuando se lo pide y tarda unos minutos.

## Uso

Requisitos: el contenedor de PostgreSQL de `docker-compose.yml` levantado y la API en marcha (`https://localhost:7064` por defecto).

```bash
node scripts/demo-data/seed.mjs load                  # crea la demo (se puede repetir: lo que ya existe se salta)
node scripts/demo-data/seed.mjs load --day=2026-11-03 # fechas relativas a ese día (por defecto, hoy en UTC)
node scripts/demo-data/seed.mjs verify                # entra con las cuentas destacadas y muestra lo que ve cada una
node scripts/demo-data/seed.mjs reset --dry-run       # cuenta lo que borraría
node scripts/demo-data/seed.mjs reset                 # borra solo lo creado por la demo
node scripts/demo-data/coverage.mjs                   # comprueba, sin tocar nada, que cada semana tiene material para el resumen
node scripts/demo-data/seed.mjs --help
```

Opciones: `--api=URL`, `--db-container=NOMBRE`, `--db-user=USUARIO`, `--db-name=BASE` (solo las usa `reset`, que borra con `docker exec ... psql`).

## Cómo funciona

- `dataset.mjs` tiene todo el contenido (cuentas, familias, clases y su contenido), en español e inventado. Los colegios son del catálogo oficial ya cargado en la base de datos.
- Todos los ids se derivan con un UUID v5 de una clave fija: por eso cargar dos veces no duplica nada y `reset` sabe qué borrar sin tocar lo que ya existía (cuenta real, clases de pruebas, etc.).
- **Un único día ancla (D).** Toda la demo se coloca respecto a `--day` (por defecto, hoy en UTC). El contenido cubre de D-6 a D+14: así la semana de lunes a domingo que contiene a D (sea D el día que sea) y la siguiente tienen avisos, eventos y deberes para el resumen semanal, y hay algo en la tercera. Cada clase tiene 9-11 avisos (urgentes y normales, los más antiguos leídos por casi todas las familias y los nuevos sin leer), 5-8 eventos de tipos distintos, 7-10 deberes (con propuestas de familias y comentarios), 3-4 gastos (con alguno pendiente), 4-7 objetos perdidos (abiertos, encontrados y recuperados) y cumpleaños dentro de la ventana. Los textos evitan días de la semana y fechas concretas para seguir valiendo al mover el ancla.
- Las fechas de eventos, deberes y cumpleaños salen de `--day` y las pone la API. Los avisos, los objetos perdidos y los gastos los sella la API con el momento de la carga y no deja cambiarlo, así que `load` fija su fecha de publicación planificada (`offset` y `at` en `dataset.mjs`) con un `UPDATE` local por `docker exec ... psql`, igual que `reset` borra. Contra una API remota ese paso no se hace. Repetir `load` vuelve a escribir los mismos valores.
- **Aviso centinela.** El aviso `agenda` de 3ºA (`anchorSentinelId()` en `dataset.mjs`) tiene como fecha de publicación (en UTC) exactamente el día ancla. Un script SQL de refresco lee de ahí el ancla vigente y mueve todas las fechas de la demo la diferencia hasta el día nuevo. No lo borres ni lo muevas por separado.
- `coverage.mjs` calcula, solo con los offsets del dataset, cuántos avisos, eventos y deberes caen en la semana actual y la siguiente para cada uno de los siete días de la semana posibles del ancla (mínimo 4 elementos y 2 secciones con contenido). Pasarlo tras tocar el contenido.
- **Para cambiar de día de referencia con `load` hay que hacer antes `reset`**: lo que ya existe se salta, con sus fechas antiguas (salvo la publicación de avisos, objetos y gastos, que se recoloca). Para mover una demo ya cargada sin vaciarla, hay que desplazar todas las fechas de sus filas la misma cantidad de días.
- Los avisos futuros (hasta 14 días) aparecen en la web con fecha de publicación posterior a hoy: hasta 6 días como «mañana» o «dentro de N días» y a partir de ahí con la fecha corta, sin marca de futuro.
- Cada ejecución inicia sesión de nuevo con las cuentas de la demo, así que `Sessions` y `RefreshTokens` crecen; `reset` las borra.
- Todo el contenido es de ámbito clase: el ámbito colegio no se puede publicar por la API (da 403 por diseño), y un hijo tiene un solo tutor.
- El resumen semanal con IA no se genera aquí: aparece cuando alguien abre el panel con la API arrancada con la clave de Anthropic.

## Ficheros SQL para desplegar (`export-sql.mjs`)

Para cargar la demo en una base de datos sin pasar por la API (por ejemplo, la de Azure), `export-sql.mjs` genera tres ficheros SQL a partir de la demo cargada en la base **local** (solo lee con `docker exec ... psql`; no toca ninguna fila). Las filas se eligen únicamente por los ids de la demo, nunca volcando tablas.

```bash
node scripts/demo-data/export-sql.mjs --out=<carpeta>   # por defecto, la carpeta actual
```

- `Datos-demo-SumiClas.sql`: catálogo territorial (provincias, poblaciones, códigos postales y colegios) y **toda** la demo. Requiere las migraciones aplicadas y esas tablas vacías. INSERT estándar en lotes de 500 filas dentro de una transacción; vale para pgAdmin, DBeaver, Azure Data Studio o psql. No lleva sesiones, tokens de refresco, resúmenes de IA ni reclamaciones de objetos perdidos.
- `Datos-demo-ampliados-SumiClas.sql`: **solo** el contenido añadido por la ampliación del curso (`EXTENSION_KEYS` en `dataset.mjs`), aditivo, para una base que ya cargó la versión anterior (demo base). No debe ejecutarse sobre una base vacía ni dos veces.
- `Actualizar-fechas-demo-SumiClas.sql`: script de refresco (ver abajo).
- Opción oculta `--emit-base-only`: escribe además un fichero con el catálogo y solo la demo base, para comprobar que base + ampliación = completo.

Hay que regenerarlos cuando cambie el dataset o el esquema.

## Colocar las fechas de la demo en otro día (`Actualizar-fechas-demo-SumiClas.sql`)

Un único bloque `DO` de PostgreSQL, sin comandos de cliente. **Conviene ejecutarlo cada dos semanas** (o antes de una presentación) para que el contenido vuelva a caer alrededor de hoy. Los parámetros son las dos primeras líneas del bloque:

```sql
new_anchor date := CURRENT_DATE;   -- el día en que quieres colocar la demo (por defecto, hoy)
dry_run boolean := false;          -- true: informa de lo que haría y lo deshace (acaba con el error «simulacion»)
```

Lee el ancla actual del aviso centinela, calcula la diferencia en días hasta `new_anchor` y mueve esa cantidad todas las fechas de las filas de la demo (por sus ids): publicaciones y ediciones, fechas de entrega, fecha de eventos y cumpleaños (con aritmética real de fechas), lecturas, entregas, comentarios, altas y confirmaciones de matrícula; los valores vacíos siguen vacíos. No mueve las fechas del curso escolar (2026-09-01 a 2027-06-30): avisa si el nuevo ancla cae fuera. Borra los resúmenes semanales de las clases de la demo para que se regeneren (hace falta `Ai__ApiKey` en el App Service). Es atómico, se puede repetir (con la misma fecha no hace nada) y no toca filas que no sean de la demo. No mueve sesiones ni tokens de refresco. Avisa si el esquema tiene una columna de fecha que no conoce.

**Orden en producción:** migraciones, luego `Datos-demo-SumiClas.sql` (o la demo base antigua más `Datos-demo-ampliados-SumiClas.sql`) y, por último, el refresco con el `new_anchor` deseado. Si la base ya tenía la demo base del fichero antiguo, esas filas quedan un día desplazadas hasta que se ejecute el refresco.

Verificado solo contra bases de prueba locales (contenedor de desarrollo); no se ha ejecutado contra Azure.

## Cuentas

La contraseña de todas es `Demo1234`. Los correos son `nombre.apellido@demo.sumiclas.test`; las destacadas y lo que muestra cada una están en el `Readme.md` de la raíz y en la salida de `load`.
