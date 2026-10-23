# Tests de los endpoints en lo más delicado

## Objetivo
Cubrir con tests, sobre los endpoints reales, solo lo más delicado de la capa HTTP, que hoy no tiene ninguno: el perímetro de autenticación, los límites de entrada, el límite de intentos del login y que los errores de seguridad llegan al cliente con su estado. No se prueba todo endpoint por endpoint.

## Qué ya está probado (y no se repite)
- La capa de autenticación en sí (`AuthenticationEndpointsShould`: cookie, rotación, reutilización), la validación del bearer (`JwtBearerValidationShould`) y el manejador de excepciones con la convención de estados.
- La autorización por pertenencia y rol está probada en los casos de uso, no aquí.

## Qué se añade
1. **Perímetro**: todo endpoint real salvo los cuatro de `/auth` exige sesión. Se recorren los endpoints registrados (no una lista a mano), así que un endpoint nuevo olvidado sin protección rompe el test. Con un caso de control: una petición con un token válido llega al caso de uso, para que el test no pase solo porque todo responda 401.
2. **Límites de entrada**: lo que el binding HTTP rechaza antes de llegar al caso de uso (importe de un gasto, longitud de títulos, de la URL de la foto y del nombre de una clase), con su 400 y sin llamar al caso de uso.
3. **Límite de intentos**: el login y el registro se limitan a un número de peticiones por IP y el refresco y el cierre de sesión tienen su propio contador (429 con `Retry-After`).
4. **Errores de seguridad por la interfaz real**: un 403 al leer una clase ajena, un 403 al publicar a nivel de colegio y un 409 al guardar un duplicado, con el `code` en el cuerpo.

## Patrón seguido (el de los otros tests de `Api/`)
- Servidor real en un puerto efímero (`WebApplication.CreateSlimBuilder`, `UseUrls("http://127.0.0.1:0")`), casos de uso simulados con `Mock<ISender>`, `new XEndpoints().Map(app)` o `MapEndpoints()`, sin paquetes nuevos ni base de datos.
- `<Algo>Should`, métodos en `snake_case`, sin `var`, `this._campo`, como mucho 120 columnas, Object Mothers para los datos, configuración de los mocks en métodos `Should...` (el `SetUp` solo hace `Reset`) y verificación mutando el código de producción (todo test debe fallar si se rompe lo que prueba).

## Tareas
- [x] T1 Ayudante común (`EndpointsHost`) que monta la API real con los casos de uso simulados + tests del perímetro.
- [x] T2 Tests de los límites de entrada y del límite de intentos.
- [ ] T3 Tests de los errores de seguridad por la interfaz real y documentación (`AGENTS.md`).

## Criterios de aceptación
- Build limpio, todos los tests en verde con las semillas por defecto, 21 y 808, y ningún `var`.
- Barrido de mutaciones sobre producción: hacer anónimo un endpoint, quitar un límite del request, quitar el límite de intentos o la política de autorización por defecto hace fallar algún test.
- Revisión nativa por pieza y un commit por pieza.

## Progreso y evidencia
- T1 (commiteada `aac3334`): `EndpointsHost` (`Api/`) monta la API real como `Program.cs` (seguridad, manejador de excepciones, límite de intentos, validación y todos los endpoints con `MapEndpoints`) sobre un puerto efímero, con `ISender` simulado y sin base de datos; reutiliza los mothers de JWT, configuración y cookie. `ProtectedEndpointsShould` (6 tests): recorre los endpoints que la API realmente registra (los que no son de `/auth`, más de 35) y exige 401 con el `code` `Authentication.Unauthenticated` sin token y ninguna llamada a un caso de uso; un caso de control (`GET /users/me` con un token válido llega al caso de uso, para que el test no pase solo porque todo responda 401); y los cuatro endpoints de `/auth` no piden sesión. `AddValidation` se pudo llamar directamente (no hizo falta reflexión). Barrido: 3 mutaciones detectadas (quitar la política de autorización por defecto, dejar un endpoint anónimo y quitar el anonimato del grupo `/auth`). 875 tests en el proyecto.
- T2 (sin commitear): `RequestLimitsShould` (12 tests, con `RequestBodyMother` que construye los cuerpos desde los `record` reales de las peticiones): el importe de un gasto fuera del rango 0,01 a 99.999.999,99 y justo en cada borde, concepto de más de 200 caracteres, gasto sin participantes, contraseña de más de 128 (registro y login) y justo de 128, correo de más de 320, URL de foto de más de 2.048 y nombre de alumno de más de 100; cada rechazo comprueba el 400, que el campo aparece en `errors` y que no se llama a ningún caso de uso. `RateLimitingShould` (6 tests, con el límite bajado a 2): pasan hasta el límite y luego 429 con `Retry-After` y `code` `Authentication.TooManyRequests`; el registro comparte contador con el login y el cierre de sesión con el refresco; el refresco no cuenta con el login; las demás rutas no tienen límite; y cada dirección tiene su propio contador (un cliente que sale desde `127.0.0.2`), para que nadie pueda bloquear el login de los demás. Hallazgo: `AddValidation()` se apoya en un generador de código que descubre los tipos desde el proyecto que hace la llamada; llamado desde el proyecto de tests no validaba nada (devolvía 201 y 500 donde se esperaba 400). Por eso la llamada pasa a `AddSumiClasValidation()` (`api/Configuration/ValidationServiceCollectionExtensions.cs`), que usan `Program.cs` y el servidor de pruebas; en producción nada cambia, porque la llamada ya estaba en el proyecto de la API. Barrido: 21 mutaciones detectadas y 1 equivalente (`RejectionStatusCode`, que `OnRejected` ya fija a 429 con el `ProblemDetails`); la partición por IP solo murió al añadir el test de direcciones.

## Siguiente paso
T3: errores de seguridad por la interfaz real y documentación.
