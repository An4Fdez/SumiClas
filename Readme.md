# 🏫 SumiClas

> La app de coordinación de la clase: avisos, calendario, deberes, gastos y objetos perdidos en un solo sitio, en lugar de perderse en el grupo de WhatsApp.

## 📋 Descripción general

**SumiClas** es una aplicación web de coordinación escolar pensada para las familias de un colegio de infantil y primaria. Sustituye al grupo de WhatsApp de la clase: lo que antes se perdía entre cientos de mensajes (la excursión de la semana, el dinero del regalo a la maestra, los deberes del viernes, la chaqueta olvidada) se reparte en pantallas claras, con lo pendiente destacado en un panel de inicio.

- Los **tutores** (padres, madres y tutores legales) consultan lo que pasa en la clase de cada uno de sus hijos y cambian de hijo con un selector.
- Los **delegados** de cada clase publican avisos, eventos, asignaturas y gastos, y gestionan quién más es delegado.
- Se cuida la **privacidad de los menores**: solo se muestra la inicial del segundo apellido y los cumpleaños no llevan año.
- Un **resumen semanal con inteligencia artificial** cuenta en pocas frases lo más importante de la semana de la clase.

Es un proyecto de TFM en fase beta. El contexto de producto, los roles y el modelo de datos están en [`docs/`](#-documentación).

## ✨ Funcionalidades principales

| Pantalla | Qué permite |
| --- | --- |
| 🔐 **Registro e inicio de sesión** | Alta de la familia con correo y contraseña, sesión segura y cierre de sesión. |
| 🧭 **Alta guiada (onboarding)** | Añadir a un hijo y matricularlo buscando su colegio por provincia y población (catálogo oficial de centros), curso y clase. |
| 📊 **Panel de inicio** | Resumen de lo pendiente de la clase del hijo elegido (avisos sin leer, deberes, gastos, objetos perdidos y próximos eventos) y tarjeta con el **resumen semanal con IA**. |
| 📢 **Avisos** | Avisos de la clase con prioridad (urgentes), marca de leído por familia y enlace a un evento. |
| 📅 **Calendario** | Excursiones, reuniones, actividades y **cumpleaños** de la clase, en vista de día, semana, mes o lista. |
| 📚 **Deberes** | Deberes por asignatura con fecha de entrega, comentarios y marca de «hecho»; los de una familia se publican como **propuesta** y los del delegado como oficiales. |
| 💶 **Gastos** | Gastos de la clase repartidos entre familias, con el pago de cada una (hecho o pendiente). |
| 🔎 **Objetos perdidos** | Publicar objetos perdidos o encontrados y marcarlos como recuperados. |
| 👥 **Rol en la clase** | Ver los delegados y las familias de la clase; el delegado puede nombrar a otro (hasta tres), ceder el rol o renunciar. |
| 🌙 **Modo claro y oscuro** | Toda la interfaz se adapta al tema elegido. |

La interfaz está en español; el código, en inglés.

## 🛠️ Stack tecnológico

**Backend** (`apps/backend`)
- [.NET 10](https://dotnet.microsoft.com/) · ASP.NET Core Web API (endpoints mínimos), arquitectura de *vertical slices* por módulos
- [Entity Framework Core 10](https://learn.microsoft.com/ef/core/) con [Npgsql](https://www.npgsql.org/) · [PostgreSQL 17](https://www.postgresql.org/)
- [MediatR](https://github.com/jbogard/MediatR) (casos de uso, CQRS) · autenticación con **JWT** y *refresh token* rotatorio en cookie
- OpenAPI y versionado de API (`Asp.Versioning`) · log4net
- [API de mensajes de Anthropic](https://docs.anthropic.com/) para el resumen semanal (opcional, con clave propia)

**Frontend** (`apps/frontend/sumiclas-web`)
- [React 18](https://react.dev/) · [TypeScript 5](https://www.typescriptlang.org/) · [Vite 5](https://vite.dev/)
- [Tailwind CSS 3.4](https://tailwindcss.com/) · [React Router 7](https://reactrouter.com/) · [lucide-react](https://lucide.dev/)
- Cliente HTTP generado con [NSwag](https://github.com/RicoSuter/NSwag) a partir del OpenAPI del backend · gestor de paquetes [pnpm](https://pnpm.io/)
- ESLint (`typescript-eslint`)

**Pruebas y herramientas**
- [NUnit 4](https://nunit.org/) · [Moq](https://github.com/devlooped/moq) · [AwesomeAssertions](https://awesomeassertions.org/) · [Testcontainers](https://dotnet.testcontainers.org/) (PostgreSQL real en Docker)
- [Docker Compose](https://docs.docker.com/compose/) para la base de datos local

## 📁 Estructura del proyecto

```
SumiClas/
├── apps/
│   ├── backend/
│   │   ├── api/                 → SumiClas.Api: capa HTTP (endpoints, autenticación, errores)
│   │   ├── src/SumiClas/        → dominio, casos de uso e infraestructura, un módulo por agregado
│   │   └── tools/               → importador temporal del catálogo de colegios
│   ├── frontend/
│   │   └── sumiclas-web/        → Vite + React + TypeScript (pages, components, hooks, modules)
│   └── test/
│       └── backend/             → tests del backend (SumiClas.Tests)
├── docs/                        → documentación de producto, arquitectura y glosario
├── scripts/demo-data/           → cargador de datos de demostración
├── odd/                         → fichas de trabajo (tareas) del desarrollo
├── docker-compose.yml           → PostgreSQL local
└── SumiClas.slnx                → solución .NET
```

Los módulos del backend (`Announcements`, `CalendarEvents`, `Assignments`, `ClassExpenses`, `LostItems`, `Classes`, `Users`, `Students`, `Enrollments`, `Schools`, `WeeklySummaries`…) siguen la misma forma: `Domain`, `Application` e `Infrastructure`. Las convenciones del repositorio están en [`AGENTS.md`](AGENTS.md).

## 🚀 Instalación y ejecución

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) y [pnpm](https://pnpm.io/installation)
- [Docker](https://www.docker.com/) (Docker Desktop o equivalente con `docker compose`)

### Pasos

```bash
# 1. Base de datos (PostgreSQL, desde la raíz del repositorio)
docker compose up -d

# 2. Clave de firma del JWT (obligatoria, una sola vez; no se guarda en el repositorio)
dotnet user-secrets set "Jwt:SigningKey" "una-clave-de-al-menos-32-caracteres" --project apps/backend/api/SumiClas.Api.csproj

# 3. Backend: https://localhost:7064 (aplica las migraciones al arrancar)
dotnet run --project apps/backend/api/SumiClas.Api.csproj

# 4. Frontend, en otra terminal: https://localhost:4004
cd apps/frontend/sumiclas-web
pnpm install
pnpm dev
```

También se puede abrir `SumiClas.slnx` en Visual Studio o Rider y ejecutar el proyecto `SumiClas.Api`: arranca el frontend por su cuenta.

- El frontend reenvía `/api/` al backend, así que la web y la API comparten origen y no hay CORS.
- **Resumen semanal con IA (opcional):** sin clave la tarjeta del panel muestra el botón «Generar resumen» y el resto de la app funciona igual. Para activarlo: `dotnet user-secrets set "Ai:ApiKey" "<tu clave de Anthropic>" --project apps/backend/api/SumiClas.Api.csproj`.
- **Catálogo de colegios:** los colegios salen del listado oficial de centros, que se carga en PostgreSQL con el importador de `apps/backend/tools` (los Excel de origen no se incluyen en el repositorio). Sin el catálogo cargado, la búsqueda de colegios del alta aparece vacía.

### 🧪 Ejecutar las pruebas

```bash
# Backend (más de mil tests; los de base de datos arrancan PostgreSQL en Docker)
dotnet test apps/test/backend/SumiClas.Tests

# Frontend (comprobación de tipos y estilo; aún no hay tests automáticos)
cd apps/frontend/sumiclas-web
pnpm typecheck
pnpm lint
```

## 🗄️ Base de datos y migraciones

El esquema de PostgreSQL lo gestionan las **migraciones de Entity Framework Core**, que viven en `apps/backend/src/SumiClas/shared/infrastructure/persistence/migrations/` (hoy hay 12, desde `InitialCreate` hasta `WeeklySummaries`).

- **Se aplican solas:** al arrancar, la API ejecuta las migraciones pendientes (`ApplySumiClasMigrations()`, en todos los entornos). No hay que lanzar nada a mano con la base de datos del `docker-compose.yml`: con `docker compose up -d` y `dotnet run` las tablas quedan creadas.
- **Conexión en desarrollo:** `ConnectionStrings:SumiClasDb` está en `apps/backend/api/appsettings.Development.json` y apunta al PostgreSQL local del `docker-compose.yml` (credenciales de desarrollo, no son secretos). En otro entorno hay que dar esa cadena de conexión como ajuste de la aplicación (`ConnectionStrings__SumiClasDb`), porque no hay ninguna por defecto y la API la necesita para migrar al arrancar.
- **Crear una migración nueva** al cambiar el modelo (hace falta la herramienta `dotnet-ef`: `dotnet tool install --global dotnet-ef`). Desde `apps/backend/src/SumiClas`:

  ```bash
  dotnet ef migrations add <Nombre> --context SumiClasDbContext --output-dir shared/infrastructure/persistence/migrations --namespace SumiClas.Shared.Infrastructure.Persistence.Migrations
  ```

  Después se compila y se arranca la API para que se aplique, y la migración se commitea junto al cambio que la motiva.
- **Los datos no vienen en las migraciones:** crean las tablas vacías. El catálogo de colegios se carga aparte con el importador (ver arriba) y los datos de prueba, con el script de la siguiente sección.
- **Empezar de cero:** `docker compose down -v` borra el volumen de PostgreSQL, y con él los datos, **catálogo de colegios incluido**. Para quitar solo los datos de la demo usa `node scripts/demo-data/seed.mjs reset`.

## 🔑 Usuario y contraseña de prueba

La base de datos de desarrollo lleva datos de prueba inventados para recorrer la web completa: dos colegios (CEIP Aravaca, en Madrid, y CEIP Aníbal González, en Sevilla), tres clases (3ºA y 4ºB en Aravaca, 2ºA en Sevilla), 17 familias con 19 hijos, y avisos, eventos del calendario, cumpleaños, objetos perdidos, deberes y gastos con pagos hechos y pendientes.

Se entra en `https://localhost:4004` con cualquiera de estas cuentas. **La contraseña de todas es `Demo1234`**.

| Correo | Qué caso muestra |
| --- | --- |
| `lucia.navarro@demo.sumiclas.test` | Madre y **delegada** de 3ºA (Aravaca): publica avisos, eventos, deberes y gastos, y gestiona su clase. |
| `marcos.gil@demo.sumiclas.test` | Padre con **dos hijos** (mellizos) en la misma clase, con un aviso sin leer y un gasto pendiente. |
| `elena.ruiz@demo.sumiclas.test` | Madre con **dos hijos en clases distintas** (4ºB y 3ºA): se cambia de hijo y de clase con el selector de hijo. |
| `tomas.herrero@demo.sumiclas.test` | Padre con la hija **al día**: todo leído, deberes hechos y gastos pagados (nada pendiente). |
| `carmen.ruiz@demo.sumiclas.test` | Madre y delegada de 2ºA en **otro colegio y otra ciudad** (Sevilla). |
| `irene.castillo@demo.sumiclas.test` | **Recién registrada**, sin hijos ni clase: recorre el alta (provincia, población, colegio, curso y clase). |

El resto de familias (otras doce cuentas del mismo dominio) completan las listas de la clase. Todos los datos son ficticios; solo los nombres de los colegios son oficiales (del catálogo ya cargado).

Las fechas de los eventos, los deberes y los cumpleaños son **relativas al día en que se carga la demo**, así que conviene recargarla el día de la presentación o el anterior (se necesitan Docker con PostgreSQL levantado y la API en marcha):

```bash
node scripts/demo-data/seed.mjs reset            # borra solo lo creado por la demo
node scripts/demo-data/seed.mjs load --day=2026-11-03   # día de referencia (por defecto, hoy)
node scripts/demo-data/seed.mjs verify           # lo que ve cada cuenta destacada
```

El resumen semanal con IA del panel solo aparece si la API se arranca con la clave de Anthropic (`Ai:ApiKey`, ver `AGENTS.md`); sin ella la tarjeta muestra el botón «Generar resumen». Más detalles en `scripts/demo-data/README.md`.

## 📦 Despliegue

El despliegue de producción está previsto en **Microsoft Azure**, definido como infraestructura como código con **Terraform**. Todavía no está montado en el repositorio; el diseño pendiente incluye:

- el servicio que aloja la API (App Service o Container Apps) y el alojamiento del frontend;
- PostgreSQL gestionado y la estrategia de migraciones de EF Core en producción;
- los secretos (`Jwt__SigningKey`, `Ai__ApiKey` y la cadena de conexión) como ajustes de la aplicación o referencias a Key Vault con identidad administrada, de modo que se puedan rotar sin tocar la app.

El código solo lee `IConfiguration`, así que ningún secreto vive en el repositorio. El `docker-compose.yml` es únicamente para desarrollo local.

## 🔌 Cliente HTTP del frontend (NSwag)

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

## 📖 Documentación

- [Arquitectura y producto](docs/arquitectura-app-coordinacion-escolar.md): roles, modelo de datos y reglas de privacidad de menores.
- [Diseño de la pantalla inicial](docs/diseno-pantalla-inicial.md)
- [Investigación de requisitos](docs/investigacion-requisitos-sumiclas.md)
- [Glosario de dominio](docs/glosario-dominio.md): términos del negocio en español y su nombre en el código.
- [`AGENTS.md`](AGENTS.md): convenciones del repositorio (idioma, commits, estilo y verificación).

## ✒️ Autores

- **[Ana Fernández Muñoz](https://github.com/An4Fdez)**: diseño y desarrollo.

## 📄 Licencia

© 2026 Ana Fernández Muñoz. **Todos los derechos reservados.** Este código no es de uso libre: no se puede usar, copiar, modificar ni distribuir, en todo ni en parte, sin el permiso previo y por escrito del autor. La única excepción es la evaluación académica del proyecto, que permite leerlo y ejecutarlo localmente a quienes lo evalúan. Los detalles están en el archivo [`LICENSE`](LICENSE).

---
⌨️ con ❤️ para que las familias dejen de buscar en el grupo de WhatsApp 😊
