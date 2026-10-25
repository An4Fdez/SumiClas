---
name: backend-endpoints
description: "Capa api del backend: IEndpoints y descubrimiento por reflexión, metadatos OpenAPI y etiquetas obligatorios en cada endpoint, versionado, health check, validación de longitud de los DTO y registro de servicios en Configuration. Úsala al crear o cambiar un endpoint o su DTO de request, o al tocar Program.cs o Configuration."
---

# Capa HTTP del backend: endpoints, OpenAPI y validación de requests

Aplica a `apps/backend/**`. Las reglas obligatorias, la tabla de skills y lo pendiente de decidir están en `apps/backend/AGENTS.md`; las convenciones comunes de idioma, commits y ramas, en el `AGENTS.md` de la raíz.

## `api/` — capa HTTP (Minimal API)

Organizar en tres carpetas, no todo en `Program.cs`:

- **`Endpoints/`** — un archivo (o subcarpeta) por módulo con los endpoints de ese módulo que implementa `IEndpoints` (`Endpoints/IEndpoints.cs`). **No hay que registrar nada en `Program.cs`**: `app.MapEndpoints()` (`Endpoints/EndpointsExtensions.cs`) descubre por reflexión toda clase concreta que implemente `IEndpoints` y llama a su `Map`. Aquí también viven los DTOs de request/response de esa capa HTTP (no reutilices las entidades de dominio como DTOs); los DTO de request se llaman `<CasoDeUso>Request` (`SchoolRegisterRequest`).
  ```csharp
  // Endpoints/AnnouncementsEndpoints.cs
  public sealed class AnnouncementsEndpoints : IEndpoints
  {
      public void Map(IEndpointRouteBuilder app)
      {
          app.MapGet("/announcements", ...);
          app.MapPost("/announcements", ...);
      }
  }
  ```
- **`Middleware/`** — middleware transversal (manejo de errores, logging de requests, etc.).
- **`Configuration/`** — inyección de dependencias y arranque: métodos de extensión que registran servicios de cada módulo (`AddSumiClasApplication()`, `AddSumiClasInfrastructure()` —que incluye `AddSumiClasJwtOptions()`—, `AddSumiClasApiSecurity()`, `AddSumiClasRateLimiting()`), invocados desde `Program.cs` para que este no acumule configuración de DI directamente. **Criterio para decidir dónde va un registro (decisión del usuario):** ¿seguiría haciendo falta en un proyecto de consola que reutilice el dominio? Sí → infraestructura (`AddSumiClasInfrastructure`: implementa un puerto del Domain o es fontanería técnica/persistencia); solo sirve para recibir peticiones HTTP → capa API (`AddSumiClasApiSecurity`, rate limiting). Hoy el registro de infraestructura vive en el proyecto `api`, así que un proyecto de consola no podría reutilizarlo salvo que se mueva a la librería `SumiClas` (posible paso futuro, no hecho). El orden en `Program.cs` importa: `AddSumiClasInfrastructure` antes de `AddSumiClasApiSecurity`.

`Program.cs` debe quedar como orquestador fino: llama a los métodos de `Configuration/` para registrar servicios y a `app.MapEndpoints()` (descubre todos los `IEndpoints` de `Endpoints/`) para mapear rutas, sin lógica propia.

## Metadatos OpenAPI y etiquetas de cada endpoint

- **Metadatos OpenAPI en cada endpoint (obligatorio)**: el frontend genera su cliente con NSwag a partir de `/openapi/v1.json`, y sin metadatos NSwag no sabe qué devuelve cada endpoint (todo saldría como `SwaggerResponse<void>`) ni cómo llamar a los métodos. Cada `Map*` de `api/Endpoints/` declara: (1) `.WithName("<CasoDeUso>")`: el `operationId` es exactamente el nombre del caso de uso que envía (`AnnouncementCreate`, `SchoolFinderById`), único en toda la API y **sin guion bajo** (NSwag usa lo que va antes del `_` como nombre de cliente, así que un guion bajo partiría `GeneratedApiClient` en varios); el método generado queda `announcementCreate(...)`. (2) El éxito, según lo que el endpoint devuelve de verdad: `.Produces<T>(StatusCodes.Status200OK)` en los `GET` con el DTO de `Application` (`PagedResponse<AnnouncementResponse>`, `IReadOnlyCollection<SchoolResponse>`, `UserProfileResponse`), `.Produces(StatusCodes.Status201Created)` en las creaciones (`Results.StatusCode(201)` no devuelve cuerpo) y `.Produces(StatusCodes.Status204NoContent)` en los comandos sin cuerpo (`Results.NoContent()`). (3) `.ProducesValidationProblem()` si el request lleva atributos de validación (`[StringLength]`, `[Range]`). Los demás errores (400, 401, 403, 404 y 409) no se declaran: los documenta `ProblemResponsesTransformer`. El 429 de un endpoint con límite de peticiones sí, con `.ProducesProblem(StatusCodes.Status429TooManyRequests)`, porque no sale de una excepción de dominio. Un endpoint nuevo no se da por terminado sin esto; una excepción nueva no exige tocar ningún endpoint si su nombre sigue la convención. Tras cambiar los endpoints hay que regenerar el cliente en el frontend.
- **Etiqueta por módulo (`WithTags`)**: cada clase de endpoints etiqueta su grupo con el nombre de su módulo (`app.MapGroup("/announcements").WithTags("Announcements")`). Sin etiqueta, ASP.NET usa el nombre del ensamblado (`SumiClas.Api`) y Swagger UI muestra los endpoints en un solo bloque. La etiqueta solo agrupa en Swagger: no afecta al cliente NSwag, que se nombra por `operationId`.

## Validación de longitud de los datos

**Validación de longitud de los datos** (decisión explícita del usuario): la longitud máxima **no se valida en el Domain**; se comprueba en el **frontend** y en el **request del endpoint**. En `api` cada DTO de request lleva `[StringLength(n)]` (DataAnnotations) en sus campos de texto acotados, y `builder.Services.AddSumiClasValidation()` (en `Program.cs`; envuelve `AddValidation()`, la validación nativa de Minimal API de .NET 10, y vive en `api/Configuration/` porque el generador de código que descubre qué validar solo ve los tipos del proyecto que hace la llamada: llamado desde otro proyecto, como el de tests, no valida nada) responde **400** con un `ValidationProblem` antes de llegar al caso de uso. Ejemplo: `SchoolYearCreateRequest`. El límite del atributo debe coincidir con el `HasMaxLength(n)` del mapeo EF de esa columna: **al cambiar uno hay que cambiar el otro** (y el del frontend). Si se añade un campo de texto acotado a un request, lleva su `[StringLength]`. Se valida la longitud del texto sin recortar (el Domain luego hace `Trim`). Los campos no acotados (`text`) no llevan límite. La respuesta de validación usa el formato estándar de `ValidationProblem` (sin el campo `code` de los errores de dominio).

## Versionado, OpenAPI y salud (ya implementado, no lo dupliques)

- Versionado por header `X-Api-Version` (paquete `Asp.Versioning`), versión por defecto `1.0`. Nuevos endpoints deben respetar este esquema de versionado, no introducir uno distinto (versionado por URL, por ejemplo) sin acordarlo antes.
- Swagger/OpenAPI ya configurado con un documento por versión — los nuevos endpoints aparecen automáticamente si siguen las convenciones de `Asp.Versioning`/`AddApiExplorer` ya presentes en `Program.cs`.
- Health check ya expuesto en `/healthy` vía `AddHealthChecks()`/`MapHealthChecks`.
