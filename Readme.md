# SumiClas

App de coordinación escolar (sustituto del grupo de WhatsApp de clase). Ver `docs/` para el contexto de producto y `AGENTS.md` para las convenciones del repositorio.

## Cliente HTTP del frontend (NSwag)

El frontend consume la API con un cliente TypeScript generado por NSwag a partir del documento OpenAPI del backend (`/openapi/v1.json`). Los ficheros están en `apps/frontend/sumiclas-web/src/modules/Shared/Infrastructure/`:

- `GeneratedApiClient.nswag`: configuración del generador.
- `GeneratedApiClient.ts`: cliente generado. No se edita a mano; se regenera.
- `ClientBase.ts`: clase base del cliente (se incluye arriba del fichero generado).

El CLI de NSwag está instalado como dependencia de desarrollo del frontend (`nswag`, versión 14.7.1), así que se instala con `pnpm install` y no hace falta NSwag Studio. Comprobación: `pnpm exec nswag version` (desde `apps/frontend/sumiclas-web`).

Para regenerar el cliente (desde `apps/frontend/sumiclas-web`):

```bash
pnpm generate:client
```

El comando compila y arranca una API temporal con el código actual del backend en un puerto libre, genera el cliente a partir de su `/openapi/v1.json` y la para al terminar. No toca ni depende de la API que tengas abierta en el IDE, así que el cliente siempre refleja el código actual. No modifica `GeneratedApiClient.nswag`: usa una copia temporal con el documento incrustado (`json`) vaciado y la `url` de la API temporal.

Requisitos:

- .NET 10 SDK (compilar la API y el runtime Net100 de NSwag; el CLI de npm solo envuelve los binarios de .NET).
- PostgreSQL levantado (`docker compose up -d` en la raíz), porque la API aplica las migraciones al arrancar.

Para generar contra una API que ya está en marcha, en lugar de levantar una temporal: `pnpm generate:client -- --url https://localhost:7064/openapi/v1.json`.

Se usa `pnpm`, no `npm`: un `npm add` actualiza `package-lock.json` y deja `pnpm-lock.yaml` sin la dependencia.
