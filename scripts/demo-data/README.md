# Datos de demostración

Cargador de datos de prueba para presentar la web completa. Usa la **API real** (registro, cursos, clases, hijos, matrículas, asignaturas, eventos, avisos, objetos perdidos, deberes y gastos), así que se aplican las mismas reglas que en la aplicación. No lleva dependencias: Node 20 o superior.

**Solo para desarrollo local.** Se niega a hablar con una API que no sea `localhost`, y las contraseñas de las cuentas son falsas y públicas a propósito. No lo uses nunca contra un entorno real.

## Uso

Requisitos: el contenedor de PostgreSQL de `docker-compose.yml` levantado y la API en marcha (`https://localhost:7064` por defecto).

```bash
node scripts/demo-data/seed.mjs load                  # crea la demo (se puede repetir: lo que ya existe se salta)
node scripts/demo-data/seed.mjs load --day=2026-11-03 # fechas relativas a ese día (por defecto, hoy en UTC)
node scripts/demo-data/seed.mjs verify                # entra con las cuentas destacadas y muestra lo que ve cada una
node scripts/demo-data/seed.mjs reset --dry-run       # cuenta lo que borraría
node scripts/demo-data/seed.mjs reset                 # borra solo lo creado por la demo
node scripts/demo-data/seed.mjs --help
```

Opciones: `--api=URL`, `--db-container=NOMBRE`, `--db-user=USUARIO`, `--db-name=BASE` (solo las usa `reset`, que borra con `docker exec ... psql`).

## Cómo funciona

- `dataset.mjs` tiene todo el contenido (cuentas, familias, clases y su contenido), en español e inventado. Los colegios son del catálogo oficial ya cargado en la base de datos.
- Todos los ids se derivan con un UUID v5 de una clave fija: por eso cargar dos veces no duplica nada y `reset` sabe qué borrar sin tocar lo que ya existía (cuenta real, clases de pruebas, etc.).
- Las fechas de eventos, deberes y cumpleaños salen de `--day`. Los avisos y los objetos perdidos se crean con la fecha del momento de la carga (la API no deja fijarla).
- **Para cambiar de día de referencia hay que hacer antes `reset`**: lo que ya existe se salta, con sus fechas antiguas.
- Cada ejecución inicia sesión de nuevo con las cuentas de la demo, así que `Sessions` y `RefreshTokens` crecen; `reset` las borra.
- Todo el contenido es de ámbito clase: el ámbito colegio no se puede publicar por la API (da 403 por diseño), y un hijo tiene un solo tutor.
- El resumen semanal con IA no se genera aquí: aparece cuando alguien abre el panel con la API arrancada con la clave de Anthropic.

## Cuentas

La contraseña de todas es `Demo1234`. Los correos son `nombre.apellido@demo.sumiclas.test`; las destacadas y lo que muestra cada una están en el `Readme.md` de la raíz y en la salida de `load`.
