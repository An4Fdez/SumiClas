# Login with email and password (before JWT)

## Objective
Real email + password sign-up and login, with credentials in a separate `Authentication` module. JWT is a separate, later plan: in this phase login returns the user id and name, and the frontend keeps sending `X-User-Id` (the session stays forgeable until JWT; already tracked in "REVISIÓN COMPLETA PENDIENTE" of apps/backend/AGENTS.md).

## Decisions (user)
- Method: email + password (magic link out of scope).
- Credential in its own module `Authentication`, aggregate `UserCredential` linked by `UserId`.
- Login returns user id and profile now; JWT next plan.

## Constraints
- Follow apps/backend/AGENTS.md and apps/frontend/AGENTS.md (Result/Error, one use case per action, domain events instead of a command bus, IQueryBus for cross-module reads, OpenAPI metadata, one file per type on the frontend).
- The password and its hash never travel in domain events and are never logged.
- Login failures return the same `Authentication.InvalidCredentials` (401) for unknown email and wrong password.
- Plan file: C:\Users\anusk\.claude\plans\robust-rolling-moonbeam.md

## Tasks
- [x] T1 (done, not committed yet) Domain + port + hasher + persistence + migration `Authentication` (UserCredential, AuthenticationErrors additions, IPasswordHasher, PasswordHasher<T> implementation with Microsoft.Extensions.Identity.Core, UserCredentialConfiguration, repository, tests).
- [x] T2 (done, not committed yet) `UserCredentialCreatedDomainEvent`, handler in Users, `AccountRegister` (`POST /auth/register`), `AccountLogin` (`POST /auth/login`), `UserFinderByEmail`, endpoints, rate limiter, remove `POST /users` and `UserRegister`, tests.
- [ ] T3 Regenerate NSwag client + frontend module `authentication`.
- [ ] T4 Pages Register and Login + apiError messages + real-browser verification.
- [ ] T5 Docs (both AGENTS.md) and Engram mirror.

## Acceptance criteria
- Register 201, repeated email 409, login 200 with id, wrong password and unknown email identical 401, rate limit 429, hash in DB never contains the password, `GET /users/me` still works with `X-User-Id`.
- `dotnet build`, `dotnet test`, `pnpm typecheck`, `pnpm lint` clean; browser flow register -> logout -> login works and test data is cleaned up.

## Progress
T1 and T2 done (backend), not committed. Route: delegated direct (writer fork).
- Decisions: `Authentication.InvalidCredentials` keeps its name and `ResultExtensions.ToProblem` now maps codes ending in `InvalidCredentials` to 401 (no code was bent to end in `Unauthenticated`). Event class lives in shared/domain/events and `UserCredential.Create` takes fullName/email only to raise it. Password policy server-side = 8..128 chars (client minimum is 8; the leaked-password check stays client-only; 128 protects the hasher). `IdentityPasswordHasher` (PasswordHasher<T>, Microsoft.Extensions.Identity.Core 10.0.12) verifies a null hash against a throwaway hash so unknown email costs the same; a corrupted (non-Base64) hash returns Failed instead of throwing (found by a unit test). `Email.Create` now validates the trimmed value. Rate limiter: fixed window per remote IP on /auth/* sharing one counter, 10/60 s in appsettings.json and 60/60 s in appsettings.Development.json (override with RateLimiting__Auth__PermitLimit / WindowSeconds).
- Evidence: dotnet build SumiClas.slnx 0 errors; dotnet test 171 passed (was 150 after T1, 132 before). Real stack (temp API on 5099, Docker Postgres): migration `Authentication` applied by the API on startup (UserCredentials table); register 201; same email in other case 409 `User.EmailAlreadyRegistered`; weak password 400 `Authentication.PasswordTooWeak`; login 200 `{userId, fullName}` (also with padded/upper-case email); wrong password and unknown email 401 with byte-identical bodies (cmp), also the pre-existing user without credential; 4 parallel registers of one email gave 1x201 and 3x409 with no orphan user or credential (credentials = users minus the owner); registering an existing user id with another email gave 409 `Shared.AlreadyExists` through the unique-key path and left no rows; with limit 12 the 13th /auth call gave 429 with Retry-After 60 and a ProblemDetails body, /healthy and /provinces unaffected; stored hash is 84 chars `AQAAAA...` and does not contain the password; API log has 0 occurrences of the password, wrong password or hash value (EF only logs column names); GET /users/me with X-User-Id of a registered user 200. Rough timing of 3 wrong-password logins: ~60-80 ms for both an existing and an unknown email.
- Cleanup: test rows deleted; Users has only the owner row (Ana Fernández Muñoz, no credential, left untouched; to log in with it the user must delete it and register again).
- Frontend that now breaks (T3/T4, not touched): `src/modules/users/infrastructure/NSwagUserRepository.ts` calls `client.userRegister(...)` (the generated client still has it until it is regenerated).
- Not done on purpose: JWT, HeaderUserProvider, frontend, NSwag client.

## Next step
T3: regenerate the NSwag client (userRegister disappears, accountRegister/accountLogin appear) and add the frontend `authentication` module; fix NSwagUserRepository.
