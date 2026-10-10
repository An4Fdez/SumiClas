# Duplicate id / unique key violation: 500 -> 409

## Objective
A repeated client-generated id (or any other unique-key collision, such as the unique email race) must be answered with a 409 ProblemDetails (`code` `Shared.AlreadyExists`) instead of a 500, with no duplicate row and no partial side effects.

## Root cause
EF Core `SaveChanges` throws a `DbUpdateException` whose inner `PostgresException` has SqlState `23505` (unique_violation) when a primary key or a unique index is violated. Nothing translates it, so it surfaces as a 500.

## Decisions (taken before implementation)
- Central translation, no per-use-case checks:
  - `DuplicateKeyException` in `shared/application/` (the Application layer does not reference EF or Npgsql).
  - `SumiClasDbContext` (infrastructure, implements `IUnitOfWork`) overrides `SaveChangesAsync` and, through a small pure static helper, turns a `DbUpdateException` with an inner `PostgresException` `23505` into `DuplicateKeyException`. Anything else (other SqlStates, concurrency exceptions) stays a 500.
  - `UnitOfWorkBehavior` rolls back and returns a failed `Result`/`Result<T>` with a shared error `Shared.AlreadyExists` (Spanish message) when it catches `DuplicateKeyException` from the handler, the event dispatch or the commit. `ToProblem()` already maps the `AlreadyExists` suffix to 409.
- Every POST create endpoint declares `ProducesProblem(409)` so the regenerated NSwag client types it.
- The frontend source needs no change (it already treats 409 by status).

## Tasks
- [x] T1 `DuplicateKeyException`, the shared error and the detector helper.
- [x] T2 `SumiClasDbContext.SaveChangesAsync` override.
- [x] T3 `UnitOfWorkBehavior` handling.
- [x] T4 `ProducesProblem(409)` on every POST create endpoint.
- [x] T5 Unit tests (detector and `UnitOfWorkBehavior`, which had none).
- [x] T6 Real end-to-end check on every create endpoint + concurrency + OpenAPI.
- [x] T7 Regenerate the NSwag client (`pnpm generate:client`) and verify the diff.
- [x] T8 Documentation (backend AGENTS.md, frontend AGENTS.md, the frontend ODD document).

## Acceptance
- Same body and id sent twice to each create endpoint: first 201, second 409 with `code` `Shared.AlreadyExists` (or the use case's own AlreadyExists/AlreadyRegistered code where a check already existed).
- No duplicate row and no partial side effect (e.g. no extra class representative role).
- Two identical creates in parallel: exactly one 201 and one 409, never a 500.
- Unrelated failures (400 validation, 404) unchanged.
- `dotnet test` passes, the generated client only gains 409 branches.

## Progress
- All tasks done and verified; committed in a54a958 (backend and tests), d1fa8bb (endpoint metadata and generated client) and the documentation commit (route: inline, small cross-cutting change).
- T1-T3: DuplicateKeyException, SharedErrors.AlreadyExists, UniqueViolationDetector, SaveChangesAsync override, UnitOfWorkBehavior catch.
- T4: .ProducesProblem(409) added to 11 endpoints that lacked it (13 creates in total; EnrollmentCreate and UserRegister already had it).
- T5: 9 detector tests + 10 UnitOfWorkBehavior tests; suite 82/82 passing.
- T6: real e2e on PostgreSQL, 44/44 checks: every create endpoint answers 201 then 409, parallel creates give one 201 and one 409, no duplicate rows, no extra roles; OpenAPI lists 409.
- T7: client regenerated, +66/-0, 11 identical 409 branches typed ProblemDetails; frontend typecheck, lint and build pass.
- T8: backend AGENTS.md, frontend AGENTS.md and odd/tasks/frontend-api-integration.md updated (no more 500 claims, gap removed).
- Findings: repeated class answers 409 via User.ClassRepresentativeAlreadyExists (event handler), not the new mechanism; onboarding school/student/enrollment keep the same wizard id after a 409, so retries repeat the 409 (not a regression); EF logs each expected conflict at error level (log noise).

## Next step
Done and committed. Remaining on the frontend side: the onboarding steps for school, student and enrollment keep the same wizard id after a 409, so a retry gets the same 409 again (the class creation already discards the id and reloads); fix by treating the 409 there as "already created" and continuing with what was saved. The 409 notice of lost items, calendar, homework and expenses has not been checked from the UI, only through the API.
