# Arreglos de arquitectura tras la auditoría

## Objetivo
La auditoría de convenciones (2026-10-25) encontró cuatro infracciones reales de arquitectura en `apps/backend/src`. Se corrigen, cada una con sus tests y su skill si cambia lo que describe.

## Alcance autorizado
Solo estas cuatro (el usuario pidió «lo de arquitectura (reales)»). Los números mágicos, los Mothers de los tests, los comentarios y los puntos dudosos de la auditoría quedan fuera y esperan su propia orden.

## Tareas
- [x] A1 `SchoolYearArchive` importa el Domain de `Users` (`using SumiClas.Users.Domain` y `throw new UserExceptions.ClassRepresentativeForbidden()`). Pasa a pedirlo por el bus con una query del módulo `Users` que lanza esa excepción dentro de `Users`, con el mismo código y el mismo estado HTTP que hoy.
- [x] A2 El puerto `IWeeklySummaryWriter` y lo que cuelga de él (`WeeklySummaryInput` y sus tres registros, `WeeklySummaryPrompt`, `WeeklySummaryPromptBuilder`, `WeeklySummaryWriterException`) vive en `Application` y `Infrastructure` lo importa. Pasa a `WeeklySummaries/Domain`; `Application` e `Infrastructure` dependen de él. `WeeklySummaryFingerprint` y `WeeklySummaryGenerate` se quedan en `Application`.
- [ ] A3 `Locality`, `PostalCode`, `Province` y `AccessToken` validan con `if` sueltos en `Create`; pasan a un `private static void Validate(...)` como manda `backend-domain`, sin cambiar códigos ni mensajes de error.
- [ ] A4 Los finders de dominio de `SchoolYears`, `Schools`, `Students` y `Users` usan `FindByIdAsync`; la convención (y los otros tres) es `FindAsync`. Se renombra en la interfaz, la implementación, los usos y los tests.

## Criterios de aceptación
- Ningún `Application` ni `Infrastructure` de un módulo importa el `Domain` de otro, y `Infrastructure` no importa `Application`, en los casos corregidos (comprobado con grep).
- Mismo comportamiento observable: mismos códigos de error y mismos estados HTTP.
- `dotnet build` sin errores y `dotnet test apps/test/backend/SumiClas.Tests` en verde (con Docker), `grep \bvar\b` vacío.
- Las skills `backend-architecture`, `backend-weekly-summary` y `backend-cross-module` dicen lo mismo que el código.

## Ruta
Cada tarea la escribe un escritor delegado (cambia 2 o más archivos no triviales), una detrás de otra para no pisar el build. Tests: RED primero cuando hay comportamiento nuevo (A1); A2, A3 y A4 son refactors con los tests existentes como red de seguridad. Commit por tarea solo tras el «sí» del usuario.

## Progreso y evidencia
- A1: nuevo caso de uso `UserAnyClassRepresentativeEnsure` en `Users` (lanza `ClassRepresentativeForbidden` dentro de `Users`, mismo código y HTTP) y `UserSpecification` con `classIds`; `SchoolYearArchive` ya no importa `Users.Domain` y manda una sola query con todas las clases. Tests: 6 nuevos (RED por no compilar, luego verde), suite 1221 correctos, 0 fallos; `var` vacío; sin `Users.Domain` en `SchoolYears`. Skill `backend-authorization` actualizada. Ruta: escritor delegado (3 archivos de código y 5 de tests). Revisión nativa: no solicitada.
- A2: movidos con `git mv` a `WeeklySummaries/Domain` el puerto `IWeeklySummaryWriter`, `WeeklySummaryInput` (+3 registros), `WeeklySummaryPrompt`, `WeeklySummaryPromptBuilder` y `WeeklySummaryWriterException`; `Infrastructure` y `Domain` ya no importan `Application` (grep). Tests de lo movido pasan a `Tests/WeeklySummaries/Domain`. Refactor puro: suite 1221 correctos, 0 fallos; `var` vacío; skill `backend-weekly-summary` actualizada. Ruta: escritor delegado (8 archivos movidos y 6 de usings/tests). Revisión nativa: no solicitada.

## Siguiente paso
Seguir con A3.
