# Tipos explícitos en lugar de `var` en el código de producción del backend

## Objetivo
Sustituir cada `var` del código de producción del backend por el tipo que le corresponde, sin cambiar el comportamiento.

## Alcance
- Entra: `apps/backend/src/SumiClas` (336 usos en 107 ficheros) y `apps/backend/api` (54 usos en 26 ficheros). Incluye `foreach (var ...)`, `out var ...`, `using var ...` y las 4 deconstrucciones `var (a, b) = ...`.
- No entra: `apps/test` (ya no tiene ningún `var`; la regla "sin `var`" estaba documentada solo para los tests), `apps/backend/tools/SumiClas.Tools.SchoolCatalogImporter` (63 usos en un fichero; el importador está pendiente de borrar) ni las migraciones de EF generadas.
- No hay tipos anónimos, así que todo `var` tiene un tipo nombrable.

## Decisiones
- El tipo lo decide el compilador, no a mano: se aplica el arreglo de código IDE0008 ("usar tipo explícito") con `dotnet format style`, para que cada tipo sea exacto (nulabilidad incluida) y no una suposición.
- La configuración que activa IDE0008 es un `.editorconfig` temporal que no se commitea; si se quiere una regla permanente es una decisión aparte.
- Comportamiento sin cambios: el diff solo puede tocar la palabra `var` (y los `using` que hagan falta).

## Tareas
- [x] T1 Prueba sobre un módulo pequeño para ver la calidad de lo que genera `dotnet format` (nombres completos, `using`, nulabilidad).
- [x] T2 Aplicar a `apps/backend/src/SumiClas`: build limpio, tests en verde con tres semillas, ningún `var` restante.
- [x] T3 Aplicar a `apps/backend/api`: lo mismo.
- [ ] T4 Documentar en `apps/backend/AGENTS.md` la regla "sin `var`" como una regla obligatoria de todo el backend (producción y tests), que hay que cumplir en todo código nuevo (petición del usuario).

## Criterios de aceptación
- `dotnet build` limpio y `dotnet test` en verde (856 tests) con las semillas por defecto, 21 y 808.
- `grep` de `\bvar\b` en `src` y `api`, sin migraciones, vacío.
- Un script comprueba que, quitando los tipos escritos, el diff solo cambia en esas posiciones (ninguna otra línea tocada).
- Revisión nativa por pieza y un commit por pieza.

## Progreso y evidencia
- T1: `dotnet format style --diagnostics IDE0008 --severity info` con un `.editorconfig` temporal (que se borra al terminar) escribe tipos con nombre simple, añade el `using` que falta y compila. Con `--severity warn` se salta los `var` cuyo tipo es evidente (p. ej. `var x = Foo.Create(...)`); con `info` los coge todos. Hay que repetir la pasada hasta que no cambie nada (3 pasadas en `src`).
- T2 (commiteada en tres partes por módulos, `e63a021`, `d6bfe2b` y `7ffe8db`, porque una sola revisión nativa de 108 ficheros no cabía en su presupuesto de contexto): `src` pasa de 336 `var` a 0 en 107 ficheros (+347/-336 líneas, 11 `using` añadidos). Un script (`git diff` línea a línea) comprueba que cada línea cambiada solo sustituye `var` por un tipo: 331 sustituciones exactas y 5 casos que no encajan en la regla simple y se revisaron a mano (las 4 deconstrucciones `var (a, b) = ...`, que pasan a `(IReadOnlyList<T> a, string? b) = ...`, y `UserPasswordVerify`, con un `out var` dentro). Dos correcciones de nulabilidad que el formateador escribió de más: `IReadOnlyList<T>?` en esas cuatro deconstrucciones y `TResponse?` en `UnitOfWorkBehavior`. Resto de tipos con `?` revisados uno a uno (`GetByIdAsync`, `FirstOrDefault`, `GetValueOrDefault`, `TryGetValue`...): correctos. Build de la solución sin errores y sin advertencias nuevas; 856 tests en verde.
- T3 (sin commitear): `api` pasa de 54 `var` a 0 en 26 ficheros (+56/-54 líneas, 2 `using` añadidos). El script de comprobación da 54 sustituciones exactas y 0 anomalías. Ocho `foreach` salieron con un `?` de más (`Type?`, `ApiVersionDescription?`) y se quitaron; los `string?` de las cookies y de los claims son correctos (el indexador de cookies y `FindFirst` devuelven `string?`). Build sin errores y 856 tests en verde con las tres semillas.

## Siguiente paso
T4: documentar la regla como obligatoria en `apps/backend/AGENTS.md` y apuntarla desde el `AGENTS.md` de la raíz.
