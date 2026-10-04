# Frontend <-> API integration

## Objective
Replace the mocks of `apps/frontend/sumiclas-web` with the real backend API, in vertical slices, using the generated NSwag client (`src/modules/Shared/Infrastructure/GeneratedApiClient.ts`).

## Decisions (user)
- Follow the order below, point by point.
- Identity stays PROVISIONAL: on register the client generates the user id (`crypto.randomUUID()`), stores it in localStorage and sends it as `X-User-Id`. The Login page is not functional until real authentication exists (it says so). The password is never sent anywhere.
- CORS and JWT are explicitly postponed ("more ahead"): tracked in `apps/backend/AGENTS.md` ("Pendiente de decidir" and "REVISIÓN COMPLETA PENDIENTE"). Until then the browser reaches the API only through the Vite dev proxy (same origin).

## Constraints
- Frontend rules in `apps/frontend/AGENTS.md`: repository pattern in `modules/<module>/{domain,infrastructure}` injected through React Context, no TanStack Query, code in English, UI text in Spanish, typecheck + lint must pass.
- Privacy of minors: name + surname initial only, birthdays month/day only, photo opt-in.
- The backend is not changed in this feature unless a task says so.

## Tasks
- [x] T1 Plumbing: Vite proxy (`/api` -> API, strip prefix, dev cert), base URL without trailing slash, `X-User-Id` injection, shared client in a React Context, ISO date handling.
- [x] T2 Identity and profile: real register (`userRegister`), stored user id, `userProfileFinderCurrent` + `studentFinderByCurrentGuardian` feed `ChildProvider` (children, current role); non-functional Login.
- [ ] T3 Onboarding: school search/register, school year, class, student, enrollment create + confirm.
- [ ] T4 Screens, in this order: announcements, lost and found, calendar, homework (needs subjects), expenses (blocked by the missing class-members endpoint), class role page (same blocker), dashboard (no API).

## Known gaps (not solved by this feature)
- No endpoint to list class members (blocks expense participants and the class role page).
- No photo upload for lost items (the UI uses device-local `blob:` URLs).
- No dashboard summary, no school city, no teacher/AMPA roles.
- Creates return no body and ids are client-generated; the client must keep ids it needs later (e.g. the enrollment id between create and confirm).

## Acceptance
- A new user can register, create or join a class, and use each migrated screen against the real API in the browser.
- `pnpm typecheck` and `pnpm lint` pass after every task.

## Route declaration
- Exploration: delegated (one read-only mapper, 4+ files).
- Each task: one delegated writer (2+ non-trivial files).

## Progress
- Exploration done.
- T1 done (not committed). Files: `vite.config.ts` (proxy `/api` -> `API_PROXY_TARGET`, default `https://localhost:7064`, prefix stripped, `secure:false`), `src/lib/auth.ts` (`getUserId`/`setUserId`, key `sumiclas-user-id`, cleared on logout), `src/modules/Shared/Infrastructure/ApiClient.ts` (baseUrl `/api`, `X-User-Id` wrapper read per request, date reviver), `apiDate.ts` (`toApiDateOnly`), `src/context/ApiClientContext.ts` + `ApiClientProvider.tsx`, `src/hooks/useApiClient.ts`, provider mounted in `src/main.tsx`; AGENTS.md of frontend and backend (CORS and JWT pending).
- T1 evidence: `pnpm typecheck` and `pnpm lint` pass. Vite on :4010 against a STUB backend on :5099 (Docker/PostgreSQL was down, so the real API could not start): `/api/healthy` -> 200 and the stub saw `GET /healthy` (prefix stripped); `/healthy` without prefix is served by Vite (SPA) and never reaches the backend. Client run through the proxy: no id -> 401 ProblemDetails and no header; with id -> 200 and `X-User-Id` received; date-only -> local-midnight Date, date-time -> correct instant, date-like free text untouched; `toApiDateOnly` sends `yyyy-MM-dd` (Madrid, +2h); no `//` in any URL.
- T1 NOT verified: other time zones (Node on Windows ignores `TZ`). The real API and the browser were verified later, in T2.
- Finding: the generated client throws the `ProblemDetails` object itself (not `ApiException`) when the error response has a body.
- T2 done (not committed). New: `src/modules/users/{domain,infrastructure}` (UserRepository + Http), `src/modules/students/{domain,infrastructure}` (StudentRepository + Http, Child), contexts/providers/hooks for both repositories (mounted in `main.tsx`), `src/lib/apiError.ts`, `RequireSession` route guard. Changed: `src/lib/auth.ts` (`isAuthenticated` = stored user id, `clearSession` replaces `setAuthenticated`), Register (real `userRegister`, errors, submit state), Login (non-functional with a notice, redirects to /home when a session exists), `ChildContext`/`ChildProvider` (real children and role, `loading`/`error`/`reload`), `AppLayout` (loading and error states), `ChildSelector` ("Sin clase"), `EventForm` (`ChildView` type), More (logout via `clearSession`), Onboarding (no fake auth), `data/appMock.ts` (removed `childrenMock`, `summaryByChild`, `classRoleByChild` and the `Child` type), frontend `AGENTS.md`.
- T2 evidence: `pnpm typecheck` and `pnpm lint` pass. REAL stack (temporary API on :5099 built from the current source against the Docker Postgres, Vite on :4010 with `API_PROXY_TARGET`), driven in a real headless Chrome through the DevTools protocol (the Claude in Chrome extension was not connected after 2 attempts) with the throwaway script `scratchpad/ui_check.mjs`: 29/29 checks passed. Covered: `/home` and `/onboarding` without a session redirect to `/login`; Login shows the notice and is disabled; UI registration lands on `/onboarding`, stores a GUID, creates the row in Postgres (full name and email) and stores no password; a duplicate email shows the Spanish error, keeps the previous id and leaves the form usable; with a class creator and 2 students (one enrolled, one not) the selector shows `Lucia P.` (no double dot), the real school and `3A`, the badge `Delegado/a de padres`, and the dropdown lists `Mateo P.` as `Sin clase`; the full surname never appears; reload keeps the session; real requests to `/api/users/me` and `/api/users/me/students` carried `X-User-Id` (the first register request had none); no uncaught page exceptions; logout clears the session and `/home` redirects to `/login`; an unknown stored id shows `No encontramos tu cuenta` with Retry and Logout, and Logout clears the session. Two screenshots were reviewed (login notice, selector with dropdown). All test rows were removed from Postgres, and only my own processes (Vite :4010, API :5099, test Chrome) were stopped.
- T2 NOT verified: the dashboard and other screens with real data (still mock until T3/T4), `Authorization` flows (no JWT yet), other browsers or viewports, dark mode of the new notice and error panels, a second guardian or a participant role (only the class-creator delegate case was exercised).
- Still mock after T2 (for T3/T4): onboarding (`schoolsMock`, `schoolClassesMock`, `registeredStudentsMock`; nothing is persisted yet, so a newly registered user has no child until T3), announcements, calendar, homework, expenses, lost and found, the dashboard summary (`currentSummary` is all zeros and `aiSummaryByChild` has no entry for real ids), `otherGuardians` (empty) and the representative actions (no-ops).
- Deviation from the T2 brief: `currentRole` is `null` for a child without a class (the brief said default `participant`), so no role badge is shown where there is no class. With a class and no matching role it still defaults to `participant`.
- Detail found in T2: the API sends `surnameInitial` already with its dot ("P."), so the student name is `firstName + ' ' + surnameInitial` without adding another dot.

## Next step
T3 (onboarding): without it a newly registered user still has no child in the real flow.
