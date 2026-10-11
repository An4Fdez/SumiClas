# Especificación SDD: JWT + refresh token en cookie + CORS

> **Estado: especificación aprobada, sin implementar.** Este documento es la fuente de verdad de la funcionalidad. El seguimiento de las tareas está en `odd/tasks/jwt.md`. No se implementa nada que no esté aquí: si cambia un requisito, se edita primero este documento.

Formato **Spec-Driven Development**: primero la especificación completa (qué debe hacer el sistema, con escenarios comprobables), luego el diseño, luego las tareas. Cada tarea se da por buena cuando sus escenarios pasan.

---

## 0. Contexto y alcance

**Problema.** Ya existe login con email y contraseña (módulo `Authentication`), pero la sesión es falsificable: el backend toma al usuario de la cabecera `X-User-Id` (`api/Http/HeaderUserProvider.cs`), que el cliente elige libremente, y ningún endpoint exige autenticación. La app maneja datos de menores, así que esto debe cerrarse antes de cualquier uso fuera de desarrollo.

**Objetivo.** Identidad real y verificable en cada petición (JWT firmado), sesión larga y revocable (refresh token rotatorio en cookie httpOnly), cierre de sesión efectivo y CORS restrictivo.

**Decisiones del usuario.**
- Access token de vida corta **en memoria** + refresh token **rotatorio en cookie httpOnly**, guardado **hasheado** en BD.
- Producción en **un solo dominio con proxy `/api`** (mismo origen, como el proxy de Vite hoy); CORS restrictivo y configurable, no se usa en producción.
- Método de acceso: email + contraseña (ya implementado). Magic link fuera.

**Dentro de alcance.** Emisión y validación de JWT; refresh rotatorio con detección de reutilización; logout; autenticación obligatoria por defecto; `JwtUserProvider` en lugar de `HeaderUserProvider`; retirada de `X-User-Id`; CORS; ProblemDetails en 401/403; límite de peticiones para los endpoints nuevos; capa de sesión del frontend (restaurar sesión al recargar, refresco transparente, logout); documentación.

**Fuera de alcance (planes posteriores).** Autorización por rol y pertenencia a la clase en las lecturas de datos de menores (hoy cualquiera con sesión lee cualquier clase), recuperación/cambio de contraseña, bloqueo de cuenta por intentos, claim de rol en el token, limpieza periódica de refresh tokens caducados, tests de integración HTTP automáticos (no hay infraestructura; se verifica con E2E real como en fases anteriores), tests de frontend (no hay infraestructura).

---

## 0.1 Cómo funciona, explicado paso a paso

### El problema de hoy
Cuando haces login, la app guarda tu id de usuario y lo manda en cada petición en una cabecera (`X-User-Id`). El servidor se lo cree sin comprobar nada. Es como entrar a un edificio diciendo en portería «soy el vecino del 3ºB» y que te dejen pasar sin pedirte ninguna llave: cualquiera que conozca (o adivine) el id de otra persona puede hacerse pasar por ella y leer sus datos o los de sus hijos.

### La idea: dos llaves con funciones distintas
Para resolverlo se usan dos «llaves», porque ninguna sola es a la vez cómoda y segura:

1. **Access token (la llave de uso diario).** Es un texto, un **JWT**, que el servidor fabrica y **firma** con una clave secreta que solo él conoce. Dentro lleva quién eres (tu id) y cuándo caduca. El navegador lo manda en cada petición (`Authorization: Bearer ...`). El servidor comprueba la firma: si alguien cambia un solo carácter (por ejemplo, otro id), la firma deja de cuadrar y la petición se rechaza. Así nadie puede inventarse una identidad sin la clave. Como el servidor lo valida solo con la firma, **no puede anularlo antes de tiempo**; por eso dura **poco (15 minutos)**: si alguien lo robara, serviría muy poco rato. Se guarda **solo en la memoria de la pestaña** (no en el almacenamiento del navegador), de modo que si algún día se cuela un script malicioso (XSS), no puede llevarse una sesión duradera.
2. **Refresh token (la llave maestra, guardada bajo llave).** Es un valor aleatorio largo, no un JWT, que sirve únicamente para pedir un access token nuevo cuando el actual caduca. Dura **14 días**. Se guarda en una **cookie `httpOnly`**: el navegador la manda solo, pero ningún código JavaScript de la página puede leerla, ni siquiera el nuestro. Además, el servidor **no guarda el valor en claro, sino su huella (hash SHA-256)**, igual que se hace con las contraseñas: si alguien copiara la base de datos, no podría usar esos tokens.

### El recorrido completo de una sesión
1. **Login.** Envías correo y contraseña. El servidor las comprueba y te devuelve el access token en la respuesta y pone el refresh token en la cookie.
2. **Uso normal.** Cada petición lleva el access token. El servidor lo valida y sabe quién eres, sin consultar la base de datos.
3. **A los 15 minutos el access caduca.** La siguiente petición recibe un 401 («no autenticado»). El navegador, sin que lo notes, llama a `POST /auth/refresh`; la cookie viaja sola. El servidor comprueba el refresh, te da un access nuevo y **cambia el refresh por uno nuevo** (esto se llama *rotación*). El refresh antiguo queda invalidado. Luego repite la petición que había fallado y todo continúa.
4. **Recargar la página.** Al recargar, el access token (que solo vivía en memoria) desaparece. La app llama a `/auth/refresh` al arrancar, la cookie sigue ahí y la sesión se recupera sin pedirte la contraseña otra vez.
5. **Cerrar sesión.** Se llama a `POST /auth/logout`: el servidor anula el refresh token y borra la cookie. A partir de ese momento nadie puede renovar la sesión, ni siquiera con una copia del token. Con solo un access token en localStorage esto no sería posible hasta que caducara.

### Por qué rotar el refresh token y qué es una «familia»
Cada login crea una *familia* de refresh tokens: cada rotación genera uno nuevo de la misma familia y deja el anterior inutilizable. Si alguien roba un refresh token y lo usa, el verdadero dueño acabará presentando el suyo, que ya está usado: el servidor ve que se **reutiliza un token que ya había sido reemplazado**, deduce que hay un robo y **revoca toda la familia**; los dos (ladrón y dueño) tendrán que volver a iniciar sesión, pero el atacante pierde el acceso.

Hay una excepción razonable: si tienes la app abierta en dos pestañas y las dos intentan refrescar a la vez, una usará el token primero y la otra llegará con el ya reemplazado. Eso no es un robo. Por eso hay una **ventana de gracia de 10 segundos**: dentro de ella una reutilización solo devuelve «inténtalo otra vez» (la otra pestaña ya dejó la cookie nueva) sin cerrar la sesión. Además, el frontend coordina las pestañas con un mecanismo del navegador (`navigator.locks`) para que normalmente ni se produzca.

### Qué protege cada decisión (y de qué amenaza)
| Decisión | Amenaza que evita |
|---|---|
| Access de 15 min en memoria | Robo del token por XSS o por copiarlo de un equipo: caduca enseguida y no persiste |
| Refresh en cookie `httpOnly` | Un script malicioso no puede leer la sesión larga |
| `SameSite=Strict` + `Secure` | Otras webs no pueden provocar que el navegador envíe la cookie (CSRF) y solo viaja por HTTPS |
| Cookie limitada a la ruta `/api/auth` | La cookie no se envía en el resto de peticiones: menos exposición |
| Solo el hash del refresh en la BD | Una filtración de la base de datos no regala sesiones |
| Rotación + revocación de familia | Un refresh robado deja de servir y se detecta el robo |
| Limitador de peticiones en `/auth/*` | Fuerza bruta de contraseñas y abuso del refresh |
| El token no lleva email, nombre ni datos de menores | Si se filtra, no revela información personal |
| Todo protegido por defecto | Olvidar proteger un endpoint nuevo deja de ser un fallo de seguridad: es el comportamiento por defecto exigir sesión |

### Qué es CORS y por qué lo incluimos
Los navegadores impiden que una web de un origen (por ejemplo `https://app.midominio.com`) llame a una API de **otro** origen (`https://api.otrositio.com`), salvo que esa API lo autorice expresamente con unas cabeceras: eso es CORS. Es una protección del navegador, no del servidor.

En este proyecto, en producción, la web y la API se sirven **desde el mismo dominio** (la API cuelga de `/api`, igual que hace hoy el proxy de desarrollo de Vite). Para el navegador es el mismo origen, así que **en producción CORS no entra en juego**. Aun así lo configuramos de forma estricta y desactivada por defecto: una **lista de orígenes permitidos** que sale de la configuración (vacía = ninguno), nunca `*`. Sirve para dos cosas: poder probar el frontend sin el proxy durante el desarrollo, y no tener que rehacer nada si algún día la web y la API acaban en dominios distintos. Si en el futuro hiciera falta, bastaría con rellenar la lista y ajustar la cookie desde la configuración, sin tocar código.

### Qué cambia para cada parte
- **Para la persona usuaria**: prácticamente nada. Inicia sesión una vez, la sesión dura hasta 14 días sin volver a pedir la contraseña (mientras use la app) y «Cerrar sesión» cierra de verdad.
- **Backend**: un nuevo agregado `RefreshToken` con su tabla, tres casos de uso (`AccountLogin` ampliado, `AccountTokenRefresh`, `AccountLogout`), validación del JWT en cada petición y un `JwtUserProvider` que lee el id del token. Los casos de uso que hoy piden «quién es el usuario actual» **no cambian**: solo cambia de dónde sale ese dato. Desaparecen `HeaderUserProvider` y la cabecera `X-User-Id`.
- **Frontend**: el id deja de guardarse en `localStorage`; un almacén en memoria guarda el access token, un envoltorio de las peticiones lo añade y se encarga del refresco transparente, y un contexto de sesión decide si mostrar la app o llevar a `/login`.

### Qué NO resuelve este plan (importante)
Saber **quién eres** no es lo mismo que saber **qué puedes ver**. Después de este plan, cualquier persona con sesión sigue pudiendo pedir los datos de **cualquier clase** si conoce su id, incluidos los de menores. Eso se resuelve en un plan posterior de **autorización** (comprobar que perteneces a la clase y qué rol tienes). Tampoco incluye recuperar la contraseña, bloquear cuentas por intentos fallidos ni el acceso por enlace mágico.

---

## 1. Glosario

| Término | Significado |
|---|---|
| Access token | JWT firmado (HS256) que identifica al usuario en cada petición. Vida: 15 min. Solo en memoria del navegador. |
| Refresh token | Valor aleatorio opaco de 256 bits (base64url). Permite pedir un access nuevo. Cookie httpOnly. En BD solo su hash SHA-256 (hex). |
| Familia | Cadena de refresh tokens nacida de un mismo login (cada rotación hereda `FamilyId`). Revocar la familia cierra esa sesión. |
| Rotación | Cada uso de un refresh lo invalida y crea el siguiente. |
| Reutilización | Presentar un refresh que ya fue reemplazado: señal de robo si ocurre fuera de la ventana de gracia. |
| Ventana de gracia | 10 s tras una rotación en los que una reutilización se trata como carrera benigna (dos pestañas), no como robo. |

---

## 2. Requisitos funcionales (RF)

Palabras clave: DEBE (obligatorio), DEBERÍA (recomendado).

**Emisión de sesión**
- **RF-01** `POST /auth/login` correcto DEBE devolver 200 con `{ accessToken, expiresAt, userId, fullName }` y DEBE establecer la cookie de refresh. Credenciales incorrectas DEBEN seguir dando el mismo 401 idéntico (`Authentication.InvalidCredentials`), sin cookie.
- **RF-02** El access token DEBE ser un JWT HS256 con cabecera `typ: at+jwt` y claims `sub` (id de usuario, Guid), `jti` (Guid), `iat`, `nbf`, `exp`, `iss`, `aud`. NO DEBE contener email, nombre ni datos de menores. Caduca a los `AccessTokenMinutes` (15).
- **RF-03** La cookie de refresh DEBE ser `HttpOnly; Secure; SameSite=Strict`, con `Path` configurable (por defecto `/api/auth`, la ruta que ve el navegador tras el proxy), `Max-Age` = `RefreshTokenDays` días (14), nombre `sumiclas_refresh` (configurable) y sin atributo `Domain`. Su valor NUNCA DEBE aparecer en cuerpos de respuesta ni en logs.
- **RF-04** Cada login DEBE crear una familia nueva y un `RefreshToken` activo; en BD solo se guarda `TokenHash` (SHA-256 hex del valor).

**Refresco y rotación**
- **RF-05** `POST /auth/refresh` (anónimo, lee la cookie) con un refresh activo DEBE: marcarlo reemplazado, crear el siguiente en la misma familia (caduca a +14 días desde ese momento), devolver 200 con `{ accessToken, expiresAt, userId, fullName }` y fijar la cookie nueva.
- **RF-06** Sin cookie, valor desconocido, caducado o revocado → 401 `Authentication.RefreshTokenInvalid`, cookie borrada (`Max-Age=0`).
- **RF-07** Un refresh ya reemplazado presentado **más de 10 s** después de su rotación DEBE revocar **toda la familia** y devolver 401 (la revocación DEBE persistir aunque la respuesta sea 401). Presentado **dentro de 10 s** DEBE devolver 401 sin revocar la familia (carrera benigna entre pestañas).
- **RF-08** Un refresh de un usuario que ya no existe DEBE tratarse como inválido (401).

**Cierre de sesión**
- **RF-09** `POST /auth/logout` (anónimo, lee la cookie) DEBE revocar la familia del refresh presentado, borrar la cookie y devolver 204. DEBE ser idempotente: sin cookie o con valor desconocido también 204.

**Protección de la API**
- **RF-10** Toda petición a un endpoint no incluido en la lista de anónimos DEBE llevar `Authorization: Bearer <access>` válido (firma, `iss`, `aud`, `exp` con tolerancia de 30 s); si no, 401 con `ProblemDetails` (`status` 401, `code` `Authentication.Unauthenticated`). Un token válido pero con recurso prohibido → 403 con `ProblemDetails`.
- **RF-11** Endpoints anónimos (únicos): `POST /auth/register`, `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout`, `GET /healthy`, y en Development `/openapi/*` y Swagger UI. Quedan por tanto protegidos los `GET` hoy abiertos (colegios, provincias, poblaciones).
- **RF-12** `IUserProvider` DEBE resolverse desde el claim `sub` del token validado (`JwtUserProvider`). `HeaderUserProvider` y la cabecera `X-User-Id` DEBEN eliminarse: enviar `X-User-Id` NO DEBE autenticar nunca. Los casos de uso no cambian.
- **RF-13** Un access token válido cuyo `sub` ya no corresponde a un usuario existente NO DEBE conceder acceso a datos ajenos; los casos de uso que ya comprueban existencia siguen devolviendo `User.NotFound`.

**CORS y límites**
- **RF-14** CORS DEBE permitir únicamente orígenes de `Cors:AllowedOrigins` (vacía por defecto = ninguno), métodos `GET, POST, PUT, DELETE`, cabeceras `Authorization` y `Content-Type`, con credenciales. NUNCA `*`. Un origen no listado NO DEBE recibir cabeceras `Access-Control-*`.
- **RF-15** `/auth/refresh` y `/auth/logout` DEBEN tener su propio limitador por IP (30 peticiones / 60 s; configurable); `/auth/login` y `/auth/register` mantienen 10/60 s (60/60 en Development). Superado → 429 `ProblemDetails` con `Retry-After`.

**Configuración**
- **RF-16** Claves `Jwt` DEBEN validarse al arrancar: `SigningKey` ≥ 32 bytes, `Issuer`, `Audience`, `AccessTokenMinutes` 1–60, `RefreshTokenDays` 1–90. En ningún entorno la API DEBE arrancar sin `Jwt:SigningKey`, y el mensaje de error DEBE indicar cómo definirla. La clave NUNCA se escribe en un `appsettings*.json` del repositorio: en desarrollo se guarda con User Secrets (`dotnet user-secrets set "Jwt:SigningKey" "<clave>"`, con `UserSecretsId` en `SumiClas.Api.csproj`) y en los demás entornos con la variable de entorno `Jwt__SigningKey` o el gestor de secretos del hosting.

**Frontend**
- **RF-17** El access token DEBE guardarse solo en memoria; NO DEBE escribirse en `localStorage`/`sessionStorage`. El antiguo `sumiclas-user-id` DEBE borrarse si existe. Se conserva `sumiclas-guardian` (no sensible).
- **RF-18** Al cargar la app sin access en memoria, el frontend DEBE intentar `POST /auth/refresh`; mientras tanto las rutas privadas muestran estado de carga (no redirigen); si falla, redirige a `/login`.
- **RF-19** Ante un 401 en una petición a la API (excepto `/auth/*`), el frontend DEBE refrescar **una vez** (compartiendo una única promesa entre peticiones simultáneas y serializando entre pestañas con `navigator.locks` cuando exista) y repetir la petición una sola vez; si el refresh falla, DEBE limpiar la sesión y navegar a `/login`. Si el refresh falla con 401 DEBERÍA reintentarse una vez tras 300 ms (cookie rotada por otra pestaña) antes de cerrar sesión.
- **RF-20** El registro DEBE llamar a `register` y, tras el 201, a `login` con las mismas credenciales; la política de ids y el manejo del 409 no cambian.
- **RF-21** «Cerrar sesión» DEBE llamar a `POST /auth/logout`, vaciar la sesión en memoria y navegar a `/login`, aunque la llamada falle.

---

## 3. Requisitos no funcionales (RNF)

- **RNF-01 Seguridad de secretos**: ni access, ni refresh, ni hash, ni clave de firma en logs; el refresh en BD solo como hash; comparación por hash indexado (sin comparaciones de texto plano).
- **RNF-02 Transaccionalidad**: login, refresh y logout usan el patrón de casos de uso con `UnitOfWorkBehavior`; **un `Result` fallido hace rollback**, por lo que los casos donde hay que persistir y aun así responder 401 (reutilización, familia revocada) DEBEN devolver un resultado **exitoso** con un resultado de dominio (`Reused`, `Invalid`) que el endpoint traduce a 401.
- **RNF-03 Rendimiento**: refresh y login < 300 ms en local (una consulta por hash + una escritura); validación del access sin consulta a BD.
- **RNF-04 Compatibilidad**: .NET 10, EF Core 10/Npgsql, sin dependencias extra salvo `Microsoft.AspNetCore.Authentication.JwtBearer`.
- **RNF-05 Privacidad de menores**: el access token no incorpora datos de alumnos ni de menores.
- **RNF-06 Trazabilidad**: cada RF mapea a escenarios y estos a tests o a una comprobación E2E (matriz en §11).

---

## 4. Contratos de API

| Endpoint | Auth | Petición | Respuesta OK | Errores |
|---|---|---|---|---|
| `POST /auth/register` | anónimo | `{id, fullName, email, password}` (sin cambios) | 201 sin cuerpo | 400 `Authentication.PasswordTooWeak`/validación, 409 `User.EmailAlreadyRegistered`/`Shared.AlreadyExists`, 429 |
| `POST /auth/login` | anónimo | `{email, password}` | 200 `AccountSessionResponse {accessToken, expiresAt, userId, fullName}` + `Set-Cookie` refresh | 401 `Authentication.InvalidCredentials`, 400 validación, 429 |
| `POST /auth/refresh` | anónimo (cookie) | sin cuerpo | 200 `AccountSessionResponse` + `Set-Cookie` rotada | 401 `Authentication.RefreshTokenInvalid` + borrado de cookie, 429 |
| `POST /auth/logout` | anónimo (cookie) | sin cuerpo | 204 + cookie borrada | 429 |
| Resto de endpoints | Bearer | como hasta ahora (sin `X-User-Id`) | como hasta ahora | 401 `Authentication.Unauthenticated`, 403 |

- `expiresAt`: fecha-hora UTC ISO-8601 de caducidad del access (`ApiClient` ya revive `createdAt`; añadir `expiresAt` a `DATE_TIME_KEYS`).
- Cookie: `sumiclas_refresh=<valor>; Path=/api/auth; Max-Age=1209600; HttpOnly; Secure; SameSite=Strict`.
- 401/403 de middleware: `ProblemDetails` con `type`, `title`, `status`, `code` (misma forma que `ToProblem`).
- Endpoints nuevos: `WithName` = `AccountTokenRefresh` / `AccountLogout`, `WithTags("Authentication")`, `Produces*` y `ProducesProblem` completos; esquema de seguridad Bearer declarado en el OpenAPI.

## 5. Modelo de datos

Tabla `RefreshTokens` (migración `RefreshTokens`):

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | uuid PK | generado fuera del dominio (convención del repo) |
| `UserId` | uuid not null | sin FK (referencias por id) |
| `FamilyId` | uuid not null | índice `IX_RefreshTokens_FamilyId` |
| `TokenHash` | varchar(64) not null | SHA-256 hex; índice único `IX_RefreshTokens_TokenHash` |
| `CreatedAt` | timestamptz not null | `IClock` |
| `ExpiresAt` | timestamptz not null | `CreatedAt + RefreshTokenDays` |
| `RevokedAt` | timestamptz null | revocado por logout o reutilización |
| `ReplacedAt` | timestamptz null | cuándo fue reemplazado (ventana de gracia) |
| `ReplacedById` | uuid null | siguiente de la cadena |

## 6. Configuración

```
Jwt:Issuer              "sumiclas-api"
Jwt:Audience            "sumiclas-web"
Jwt:SigningKey          (≥32 bytes; NO va en ningún appsettings del repo. Development: User Secrets; otros entornos: Jwt__SigningKey. Obligatoria en todos)
Jwt:AccessTokenMinutes  15
Jwt:RefreshTokenDays    14
Jwt:RefreshGraceSeconds 10
Jwt:RefreshCookie:Name     "sumiclas_refresh"
Jwt:RefreshCookie:Path     "/api/auth"
Jwt:RefreshCookie:Secure   true
Jwt:RefreshCookie:SameSite "Strict"
Cors:AllowedOrigins     []   (Development: https://localhost:4004 para probar sin proxy)
RateLimiting:Auth:PermitLimit / WindowSeconds                 10 / 60   (Development 60 / 60)
RateLimiting:AuthRefresh:PermitLimit / WindowSeconds          30 / 60
```

## 7. Catálogo de errores

| Code | HTTP | Mensaje (es) |
|---|---|---|
| `Authentication.Unauthenticated` | 401 | No hemos podido identificar tu sesión. Inicia sesión de nuevo. |
| `Authentication.InvalidCredentials` | 401 | (existente) Correo o contraseña incorrectos. |
| `Authentication.RefreshTokenInvalid` | 401 | Tu sesión ha caducado. Inicia sesión de nuevo. |
| `Authentication.Forbidden` (403 de middleware) | 403 | No tienes permiso para hacer esto. |

`ToProblem` ya mapea los sufijos `Unauthenticated`, `InvalidCredentials` y `Forbidden`; añadir `RefreshTokenInvalid`.

---

## 8. Escenarios de aceptación (Dado / Cuando / Entonces)

| ID | Escenario | RF |
|---|---|---|
| S-01 | Dado un usuario registrado, cuando hace login correcto, entonces 200 con access, `expiresAt` a +15 min, y cookie `HttpOnly; Secure; SameSite=Strict; Path=/api/auth`; en BD hay 1 fila `RefreshTokens` activa con hash de 64 hex y sin el valor en claro | RF-01, 03, 04 |
| S-02 | Cuando decodifico el access, entonces tiene `sub`=id, `jti`, `iat`, `nbf`, `exp`, `iss`, `aud`, cabecera `typ: at+jwt`, y no tiene email ni nombre | RF-02 |
| S-03 | Contraseña errónea y email inexistente dan 401 con cuerpo idéntico, sin `Set-Cookie`, sin fila en `RefreshTokens` | RF-01 |
| S-04 | Dado un refresh activo, cuando llamo a `/auth/refresh`, entonces 200 con access nuevo, cookie nueva distinta, el anterior queda con `ReplacedAt`/`ReplacedById` y el nuevo hereda `FamilyId` | RF-05 |
| S-05 | Sin cookie, o con un valor inventado, `/auth/refresh` → 401 `RefreshTokenInvalid` y `Set-Cookie` con `Max-Age=0` | RF-06 |
| S-06 | Un refresh caducado (reloj +15 días) → 401 | RF-06 |
| S-07 | Dado un refresh ya reemplazado hace 60 s, cuando se presenta de nuevo, entonces 401 **y** toda la familia queda revocada (incluido el token sucesor activo); un refresh posterior con el sucesor → 401 | RF-07 |
| S-08 | Dado un refresh reemplazado hace 3 s, cuando se presenta de nuevo, entonces 401 pero la familia **no** se revoca y el sucesor sigue sirviendo | RF-07 |
| S-09 | `/auth/logout` con cookie válida → 204, cookie borrada, familia revocada; `/auth/refresh` después → 401 | RF-09 |
| S-10 | `/auth/logout` sin cookie → 204 | RF-09 |
| S-11 | Cualquier endpoint protegido (p. ej. `GET /users/me`) sin `Authorization` → 401 `ProblemDetails` con `code` `Authentication.Unauthenticated` | RF-10 |
| S-12 | Con access válido, `GET /users/me` → 200 con el perfil de `sub` | RF-10, 12 |
| S-13 | Access caducado (reloj +20 min o `AccessTokenMinutes` bajo) → 401 | RF-10 |
| S-14 | Access con firma alterada, `iss` o `aud` distintos → 401 | RF-10 |
| S-15 | Petición con `X-User-Id` válido y sin Bearer → 401 | RF-12 |
| S-16 | Con Bearer de A y `X-User-Id` de B, el usuario actual es A | RF-12 |
| S-17 | `GET /healthy` y `POST /auth/register|login|refresh|logout` responden sin Bearer; `GET /schools`, `/provinces` sin Bearer → 401 | RF-11 |
| S-18 | Origen no listado: respuesta sin `Access-Control-Allow-Origin`; origen listado: cabeceras con `Allow-Credentials: true`; preflight permite `Authorization` | RF-14 |
| S-19 | 31 llamadas a `/auth/refresh` en 60 s desde una IP → la 31 da 429 con `Retry-After`; `/healthy` no se ve afectado | RF-15 |
| S-20 | En cualquier entorno, sin `Jwt:SigningKey` (ni en User Secrets ni en `Jwt__SigningKey`) la API no arranca y el mensaje indica cómo definirla; con clave de 16 bytes tampoco; ningún `appsettings*.json` del repo contiene la clave | RF-16 |
| S-21 | Tras login, `localStorage` y `sessionStorage` no contienen ningún token; `sumiclas-user-id` antiguo se borra | RF-17 |
| S-22 | Recargar una ruta privada con sesión: muestra carga, refresca por cookie y entra sin pasar por `/login` | RF-18 |
| S-23 | Recargar sin cookie válida: tras el intento de refresh, redirige a `/login` | RF-18 |
| S-24 | Con el access caducado, dos peticiones simultáneas generan **una sola** llamada a `/auth/refresh` y ambas se repiten con éxito | RF-19 |
| S-25 | Si el refresh falla definitivamente, la sesión se limpia y se navega a `/login`; un 401 de `/auth/login` (credenciales malas) NO dispara refresh | RF-19 |
| S-26 | Dos pestañas abiertas que refrescan a la vez no cierran la sesión (locks / ventana de gracia) | RF-19, RF-07 |
| S-27 | Registrarse en la UI deja la sesión iniciada y navega a `/onboarding`; un 409 descarta el id sin bucle | RF-20 |
| S-28 | «Cerrar sesión» llama a logout, vacía la sesión y navega a `/login`; el botón Atrás no entra en rutas privadas; un fallo de red en logout no impide cerrar la sesión local | RF-21 |

---

## 9. Diseño

### 9.1 Backend (convenciones de `apps/backend/AGENTS.md`)

**Dominio** (`Authentication/Domain`): agregado `RefreshToken : AggregateRoot`
- `Create(id, userId, familyId, tokenHash, IClock clock, TimeSpan lifetime)` → `Result<RefreshToken>` (valida ids y hash no vacío; errores `RefreshToken.IdRequired`, `RefreshToken.UserRequired`, `RefreshToken.HashRequired`).
- `IsActive(clock)` = no revocado, no reemplazado y no caducado.
- `MarkReplaced(successorId, clock)`, `Revoke(clock)`, `WasReplacedWithin(clock, grace)`.
- `IRefreshTokenRepository` (`GetByIdAsync`, `SearchAsync(ISpecification)`, `AddAsync`) + `RefreshTokenSpecification(tokenHash?, familyId?)` base con un parámetro por campo.

**Puertos** (`Authentication/Application`): `IAccessTokenIssuer.Issue(userId)` → `AccessToken(Value, ExpiresAt)`; `IRefreshTokenGenerator.Generate()` → `(Raw, Hash)` y `Hash(raw)`.
**Infraestructura** (`Authentication/Infrastructure`): `JwtAccessTokenIssuer` (`JsonWebTokenHandler`, HS256, `IClock`), `RefreshTokenGenerator` (`RandomNumberGenerator` 32 bytes, base64url, SHA-256 hex), `RefreshTokenRepository`. `JwtOptions` (con validación `IValidateOptions`) y `RefreshCookieOptions` en `Authentication/Application` o `api/Bootstrap` según dependan de ASP.NET.

**Casos de uso** (clase estática con `Command`/`UseCase`/`Handler`):
- `AccountLogin` (modificado): tras verificar contraseña → genera refresh, crea `RefreshToken` (familia nueva), emite access; devuelve `Result<AccountSession>` con `AccessToken`, `ExpiresAt`, `UserId`, `FullName` y `RefreshTokenRaw` (el endpoint pone la cookie y **no** lo serializa).
- `AccountTokenRefresh.Command(RawRefreshToken?)` → `Result<RefreshOutcome>` **siempre exitoso** salvo error técnico: `Issued(AccountSession+raw)`, `Invalid` o `Reused`. Flujo: hash → buscar; no encontrado/caducado/revocado → `Invalid`; reemplazado: dentro de gracia → `Invalid` sin tocar, fuera → revocar familia y `Reused`; activo → comprobar que el usuario existe (`IQueryBus` + `UserFinderById`, si no → `Invalid`), `MarkReplaced`, crear sucesor, emitir access. El endpoint traduce `Invalid`/`Reused` a 401 + borrado de cookie (así el `UnitOfWorkBehavior` confirma la revocación).
- `AccountLogout.Command(RawRefreshToken?)` → `Result` siempre éxito: si encuentra el token revoca la familia entera.

**API** (`api/`): `AddJwtBearer` (`MapInboundClaims=false`, `ValidateIssuer/Audience/Lifetime/IssuerSigningKey`, `ClockSkew` 30 s, `RequireSignedTokens`), `AddAuthorizationBuilder().SetFallbackPolicy(RequireAuthenticatedUser)`, `.AllowAnonymous()` en la lista de §4/RF-11, `AddProblemDetails`, eventos `OnChallenge`/`OnForbidden` que escriben `ProblemDetails` con `code`; `JwtUserProvider` (lee `sub`; ausente o no-Guid → `Authentication.Unauthenticated`) sustituye a `HeaderUserProvider` (se borra); `CorsServiceCollectionExtensions` (política única desde `Cors:AllowedOrigins`; si la lista está vacía no se registra `UseCors`); `RateLimitingServiceCollectionExtensions` añade la política `auth-refresh`. Orden del pipeline: `UseRateLimiter` → `UseCors` → `UseAuthentication` → `UseAuthorization` → `MapEndpoints`. Helper `RefreshCookie.Write/Clear` en `api/Http` (único sitio que toca la cookie).

**Secuencias.**
- *Login*: cliente → `POST /auth/login` → verificar contraseña → crear familia + `RefreshToken` → `Set-Cookie` + cuerpo `{accessToken,…}`.
- *Refresco*: cliente → `POST /auth/refresh` (cookie) → `AccountTokenRefresh` → ramas `Issued` (200 + cookie rotada) / `Invalid` / `Reused` (401 + borrar cookie; en `Reused` la revocación ya está confirmada).
- *401 con reintento*: petición con access caducado → 401 → single-flight refresh → repetir una vez → OK; o fallo → limpiar sesión → `/login`.

### 9.2 Frontend (`apps/frontend/sumiclas-web/src`)

- `lib/tokenStore.ts`: `getAccessToken`, `setSession({accessToken, expiresAt, userId})`, `clearSession`; solo variables de módulo (memoria). `lib/auth.ts` conserva `guardian` y borra el id antiguo (`sumiclas-user-id`, `sumiclas-auth`) al arrancar; `getUserId()` lee de `tokenStore`.
- `modules/Shared/Infrastructure/authHttp.ts` (sustituye a `identityHttp`): añade `Authorization: Bearer`; si la respuesta es 401 y la URL no empieza por `/auth/`, llama a `refreshSession()` (single-flight: una `Promise` compartida; envuelta en `navigator.locks.request('sumiclas-refresh', …)` si existe) y repite la petición una vez; si el refresh devuelve 401 reintenta una vez a los 300 ms; si sigue fallando, `onSessionLost()` (callback registrado por `AuthProvider`: `clearSession` + `navigate('/login')`).
- `refreshSession()` usa una instancia del cliente generado **sin** `authHttp` (evita recursión) apuntando a `/api`; la cookie viaja sola (mismo origen).
- `modules/authentication`: `IAuthenticationRepository` + `refresh(): Promise<Session>` y `logout(): Promise<void>`; `login` devuelve `Session {accessToken, expiresAt, userId, fullName}`; tipo `Session` en su fichero.
- `context/AuthContext.ts`, `AuthProvider.tsx`, `hooks/useAuth.ts`: estado `'loading' | 'authenticated' | 'anonymous'`, usuario `{userId, fullName}`, acciones `login(credentials)`, `register(...)`, `logout()`; al montar llama a `refresh()` una vez; mantiene `setSession`/`clearSession` de `tokenStore`.
- `RequireSession`: `loading` → pantalla de carga; `anonymous` → `Navigate /login`; `authenticated` → `Outlet`. `Login`, `Register`, `AppLayout` ("Cerrar sesión"), `More` y los usos de `isAuthenticated()` pasan a `useAuth`.
- `Register`: `register` → `login` (mismas credenciales) → `/onboarding`; id con `useWizardIds` como hoy.
- `ApiClient.ts`: `createApiClient()` usa `authHttp`; `expiresAt` en `DATE_TIME_KEYS`.
- `lib/apiError.ts`: mensajes de `Authentication.RefreshTokenInvalid` y `Authentication.Unauthenticated`.

---

## 10. Tareas (cada una con sus escenarios; un commit revisado por pieza, ≤ ~600 líneas)

| ID | Tarea | Depende | Escenarios | Salida |
|---|---|---|---|---|
| **J0** | Guardar este documento en `docs/specs/jwt-authentication.md`, crear `odd/tasks/jwt.md` + espejo Engram, actualizar los puntos de «REVISIÓN COMPLETA PENDIENTE» que dependan de esto (solo marcar «en curso»). | — | — | docs |
| **J1a** | Dominio `RefreshToken`, `RefreshTokenSpecification`, `IRefreshTokenRepository`, errores, `RefreshTokenInvalid` en `ResultExtensions` + tests unitarios (RED→GREEN). | J0 | S-04/07/08 (dominio) | ~250 líneas |
| **J1b** | `JwtOptions` + validación, `IAccessTokenIssuer` + `JwtAccessTokenIssuer`, `IRefreshTokenGenerator` + implementación, paquete JwtBearer y `UserSecretsId` en `SumiClas.Api.csproj` (requiere permiso del usuario para tocar el `.csproj`), claves no secretas en `appsettings*.json` y la clave de firma solo en User Secrets/entorno + tests (claims, `typ`, caducidad, hash, validación de opciones). | J1a | S-01 (parcial), S-02, S-20 | ~350 líneas |
| **J1c** | Mapeo EF + `DbSet` + `RefreshTokenRepository` + migración `RefreshTokens` (pieza propia por el `Designer`). | J1a | S-01 (BD) | ~800 líneas generadas |
| **J2a** | Casos de uso `AccountLogin` (sesión+refresh), `AccountTokenRefresh`, `AccountLogout` + tests con Moq/`IClock` falso (rotación, reutilización con y sin gracia, caducado, usuario inexistente, idempotencia). | J1b, J1c | S-01, S-03..S-10 | ~500 líneas |
| **J2b** | API: JwtBearer, política de reserva y lista anónima, ProblemDetails 401/403, `JwtUserProvider` (se borra `HeaderUserProvider`), endpoints `/auth/refresh` y `/auth/logout` con `RefreshCookie`, CORS, limitador `auth-refresh`, OpenAPI Bearer, tests de `JwtUserProvider`. | J2a | S-11..S-19 | ~550 líneas |
| **J3a** | Regenerar cliente NSwag; `tokenStore`, `authHttp` (single-flight, locks, reintento), `IAuthenticationRepository.refresh/logout`, `Session`; retirar `X-User-Id`. | J2b | S-21, S-24..S-26 | ~450 líneas |
| **J3b** | `AuthContext`/`AuthProvider`/`useAuth`, `RequireSession`, `Login`, `Register` (register→login), logout en `AppLayout`/`More`, mensajes. | J3a | S-22, S-23, S-27, S-28 | ~450 líneas |
| **J4** | Verificación E2E real (API temporal + Postgres: S-01..S-20 con `curl`/script; navegador CDP: S-21..S-28, con `AccessTokenMinutes` bajo en la prueba para forzar el refresco), limpieza de datos, docs finales (ambos `AGENTS.md`: quitar «identidad provisional» y `X-User-Id`; `odd/tasks/jwt.md` con evidencia; espejo Engram). | J3b | todos | docs + evidencia |

**Definición de hecho por tarea**: build y `dotnet test` / `pnpm typecheck` + `pnpm lint` limpios; los escenarios de la tarea comprobados y anotados con su evidencia en `odd/tasks/jwt.md`; revisión nativa aprobada por pieza; sin atribución de IA en commits; cada commit compila por sí solo (se comprueba en copia aislada).

## 11. Matriz de trazabilidad

| RF | Escenarios | Tarea(s) | Verificación |
|---|---|---|---|
| RF-01..04 | S-01, S-02, S-03 | J1b, J1c, J2a | tests unitarios + E2E |
| RF-05..08 | S-04..S-08 | J1a, J2a | tests unitarios + E2E |
| RF-09 | S-09, S-10 | J2a | tests + E2E |
| RF-10..13 | S-11..S-17 | J2b | E2E |
| RF-14, 15 | S-18, S-19 | J2b | E2E |
| RF-16 | S-20 | J1b | test de opciones + E2E de arranque |
| RF-17..21 | S-21..S-28 | J3a, J3b | E2E navegador |

## 12. Riesgos y decisiones registradas

- **Rollback en fallo**: `UnitOfWorkBehavior` revierte con `Result` fallido → reutilización/logout devuelven éxito con resultado de dominio (RNF-02). Cubierto por tests de J2a.
- **Dos pestañas refrescando**: `navigator.locks` + ventana de gracia de 10 s; sin Web Locks el reintento a 300 ms es la red de seguridad.
- **Tokens caducados acumulados en BD**: no se limpian en este plan (índice por hash mantiene la búsqueda barata); tarea de mantenimiento futura.
- **Autorización**: con identidad real, el hueco principal pasa a ser que cualquier usuario autenticado lee datos de cualquier clase; plan siguiente (pertenencia a la clase en lecturas, rol en escrituras).
- **Sin tests de integración HTTP ni de frontend** (no hay infraestructura): la verificación HTTP/UI se hace con E2E real y queda documentada la deuda.
- **Detrás de proxy inverso** el limitador por IP agrupa clientes salvo que se configuren cabeceras reenviadas (ya anotado).
- **Clave de firma**: rotarla invalida todos los access (≤15 min de efecto); los refresh no dependen de ella.

## 13. Proceso SDD

1. Aprobada esta especificación, se implementa por tareas (J1 en adelante) siguiendo `odd/tasks/jwt.md`.
2. Por tarea: escribir antes los tests derivados de sus escenarios (RED) cuando haya runner aplicable → implementar (GREEN) → comprobar los escenarios → anotar evidencia.
3. Cambios de requisito: se editan primero spec y escenarios, luego código; nada se implementa fuera de lo especificado.
4. Cierre: matriz §11 completa (todos los escenarios con evidencia).
