---
name: backend-territorial-catalog
description: "Catálogo territorial del backend: provincias, poblaciones, códigos postales y colegios oficiales, su normalización de texto, sus buscadores y el importador temporal. Úsala al tocar colegios, provincias, poblaciones o el importador."
---

# Catálogo territorial y colegios oficiales

Aplica a `apps/backend/**`. Las reglas obligatorias, la tabla de skills y lo pendiente de decidir están en `apps/backend/AGENTS.md`; las convenciones comunes de idioma, commits y ramas, en el `AGENTS.md` de la raíz.

## Catálogo territorial (colegios oficiales)

Los colegios **no se registran a mano**: salen del listado oficial de centros y viven solo en PostgreSQL (el Excel de origen nunca se commitea). Modelo normalizado, sin repetidos (todo con índice único):

- `Provinces` (`Name`, `NormalizedName`): único por `NormalizedName`.
- `Localities` (`ProvinceId`, `Name`, `NormalizedName`): única por `(ProvinceId, NormalizedName)`.
- `PostalCodes` (`LocalityId`, `Code` de 5 dígitos): único por `(LocalityId, Code)`, **no** globalmente, porque el mismo código postal aparece en varias poblaciones.
- `Schools`: `LocalityId` **obligatorio**, `PostalCodeId` **opcional** (el origen trae `00000` como "desconocido": se deja `null`, no se inventa), `OfficialCode` (código de centro) único, y `NormalizedName` con índice `(LocalityId, NormalizedName)` para buscar. `Phone` es opcional (máx. 20, se recorta y un valor vacío queda `null`); se guarda y se devuelve en `SchoolResponse`, pero **no** es buscable ni se valida su formato. El importador debe rellenarlo desde la columna `phone` del Excel (~79 % de las filas la traen).

`Provinces`, `Localities` y `PostalCodes` son módulos de referencia: `Domain` (con `Create`, errores, specification y puerto), `Infrastructure` (repositorio) y una `Application` solo de lectura (sin casos de uso de escritura): `ProvinceFinderAll`, `ProvinceFinderByIds`, `LocalityFinderByProvince`, `LocalityFinderByIds` y `PostalCodeFinderByIds`, con sus `*Response` pequeños (`ProvinceResponse(Id, Name)`, `LocalityResponse(Id, ProvinceId, Name)`, `PostalCodeResponse(Id, LocalityId, Code)`). Las referencias entre ellos y desde `School` son por id, sin FK en EF (igual que el resto de módulos).

`shared/domain/TextNormalizer.Normalize` genera el texto de comparación y búsqueda (recorta, colapsa espacios, minúsculas y quita diacríticos; la `ñ` pierde también la tilde, así `pena` encuentra `peña`). Se aplica igual al texto guardado y al que escribe el usuario.

`GET /provinces` devuelve las 52 provincias ordenadas por nombre y `GET /provinces/{provinceId}/localities` las poblaciones de esa provincia ordenadas por nombre, con `name` opcional (contiene, sin distinguir mayúsculas ni acentos, vía `NormalizedName`). Ninguno de los dos pagina: la mayor provincia tiene menos de ~1.000 poblaciones. Una provincia desconocida devuelve 200 con lista vacía, igual que el buscador de colegios.

`SchoolResponse` lleva ahora `ProvinceId`, `ProvinceName`, `LocalityId`, `LocalityName`, `PostalCodeId`, `PostalCode` (el código de 5 dígitos, `null` si no hay), `Address` y `Phone`. Los nombres son de otros módulos, así que `Schools` **no** toca su dominio ni su infraestructura: `SchoolResponseFactory` (interno de `Schools/Application`) los pide por `IQueryBus` una sola vez por lista (`LocalityFinderByIds` → `ProvinceFinderByIds` → `PostalCodeFinderByIds`, esta última solo si algún colegio tiene código postal). Un listado de hasta 50 colegios cuesta siempre 4 consultas SQL (colegios + las 3 de nombres), no una por colegio. Lo usan igual `SchoolFinderById`, `SchoolFinderByIds` (otros módulos, como `Students`, siguen funcionando) y `SchoolFinderByLocality`. Si el catálogo está inconsistente (un colegio apunta a una población que no existe) se lanza `InvalidOperationException` (500) en vez de devolver nombres vacíos.

El tope de 50 de `SchoolFinderByLocality` ya se aplica en la consulta (`Specification.AddTake`, junto con el orden por nombre normalizado), en lugar de cargar todos los colegios de la población y recortar en memoria.

`GET /schools` exige `localityId` (el caso de uso `SchoolFinderByLocality` devuelve 400 `School.LocalityFilterRequired` si falta o es vacío; el endpoint solo lo traduce) y admite `name` como filtro opcional; una población desconocida devuelve 200 con lista vacía. No hay alta libre de colegios (`POST /schools` y `SchoolRegister` se eliminaron). La migración `TerritorialCatalog` añade las columnas obligatorias a `Schools` con un valor por defecto vacío: solo es segura con la tabla `Schools` vacía (es el caso de la base de desarrollo); con colegios ya guardados habría que rellenarlos antes.

**Importador temporal** (`apps/backend/tools/SumiClas.Tools.SchoolCatalogImporter`, en `SumiClas.slnx`): proyecto de consola de un solo uso que carga el catálogo desde los tres Excel (`listado_centros_*.xls`, que nunca se commitean) usando las fábricas de dominio. Es idempotente (casa por clave natural) y se **eliminará** cuando la base esté cargada: no añadas lógica de la aplicación que dependa de él. Ejecución: `dotnet run --project apps/backend/tools/SumiClas.Tools.SchoolCatalogImporter -- [carpetaConLosXls] [cadenaDeConexion]` (por defecto `C:\Users\anusk\Desktop`, y la conexión sale de `SUMICLAS_DB` o de `appsettings.Development.json`).
