# Class members endpoint

## Objective
Add `GET /classes/{classId}/members`: the list of the members of a class (name and role only). It unblocks frontend tasks T4.5 (expenses: choose participants) and T4.6 (class-role page: assign/transfer delegates).

## Decisions (taken before implementation)
- Member of a class = (a) every guardian of a student with a CONFIRMED enrollment in the class (Suggested does not count) plus (b) every user holding the ClassRepresentative role of that class. Participants have no row in `UserClassRoles`, so membership is computed. A guardian of several children appears once.
- Use case `UserClassMemberFinderByClass` in `Users/Application`; it reads other modules only through `IQueryBus`: class (`ClassFinderById`), confirmed enrollments (`EnrollmentFinderByClass`) and a new minimal Students query `StudentGuardianFinderByStudents` that returns ONLY guardian ids (no student data).
- Privacy: the response carries `userId`, `fullName` and the role only. Never the email, never data (or the count) of the children.
- Order: delegates first, then by full name (ordinal, ignore case).
- Authorization (decision taken for this endpoint): the requester must be a member of the class, otherwise `User.ClassMembersForbidden` (403), because the list exposes the full names of other families. It is the first read endpoint that checks membership; the other GETs still do not (temporary decision tracked in "REVISIÓN COMPLETA PENDIENTE").
- No pagination (class sizes are small).
- Endpoint mapped in the Users endpoints class with the full route, `WithName("UserClassMemberFinderByClass")`, tag `Users`, typed 200 and 401/403/404.

## Tasks
- [x] T1 Students: `StudentSpecification` accepts `ids`; new bus query `StudentGuardianFinderByStudents` (guardian ids only).
- [x] T2 Users: error `User.ClassMembersForbidden`; use case `UserClassMemberFinderByClass`; endpoint with OpenAPI metadata.
- [x] T3 Tests (NUnit + Moq): 11 for the use case and 5 for the Students query; whole suite 63/63.
- [x] T4 Docs: backend AGENTS.md, frontend odd file and frontend AGENTS.md.
- [x] T5 Regenerate the NSwag client (`pnpm generate:client`): the diff only adds `userClassMemberFinderByClass` and `UserClassMemberResponse` (72 lines added, 0 deleted); base URL default still `https://localhost:7064/`; `pnpm typecheck`, `pnpm lint` and `vite build` pass.
- [x] T6 End-to-end verification against the real API and PostgreSQL: 18/18 checks (script `scratchpad/e2e_members.sh`).

## Acceptance
- `dotnet build` and `dotnet test` pass; the e2e script passes against a temporary API; the generated client only gains the new method and its DTO; `pnpm typecheck`, `pnpm lint` and `vite build` pass.

## Progress
- Done and verified: `dotnet build` (0 errors, to a temp dir) and `dotnet test` (63/63) with the new use case, the new Students query, `StudentSpecification.ids`, the error `User.ClassMembersForbidden`, the endpoint with OpenAPI metadata and the docs.
- T6 evidence (temporary API built to a temp dir and run on :5099 against the Docker PostgreSQL; seeded through the API: a delegate with NO children who created the class, a guardian of two children, a second guardian, a guardian whose only enrollment is Suggested, and an outsider): the delegate gets 200 and the list `Delegada Vega:ClassRepresentative|ana Gil:Participant|Zoe Perez:Participant` (delegate first, then ordinal ignore-case, so lower-case `ana` sorts before `Zoe`); the guardian of two children appears once (3 members); the Suggested-only guardian is NOT listed and gets 403, the outsider gets 403 (`User.ClassMembersForbidden`, Spanish detail), unknown class 404, no `X-User-Id` 401, invalid guid in the route 404; guardians can read the list too; the old archived class still answers 200 to its delegate; the JSON properties are exactly `classRole,fullName,userId` with no email and no student data; `/openapi/v1.json` has `UserClassMemberFinderByClass`, tag `Users`, responses 200/401/403/404 and a typed 200; PostgreSQL has exactly one ClassRepresentative row for the delegate in the class and no Participant rows (participants are implicit). The `Contains` with student and user ids translates to SQL (the calls returned real data).
- Fixes needed after the real run: none.
- Not verified: classes with many families (no pagination, expected small), concurrent changes while listing, and a guardian who is removed from a student while the enrollment stays (they stop being a member, by design).

## Next step
Frontend T4.5 (expenses: participants picker) and T4.6 (class-role page) can now consume `userClassMemberFinderByClass`.

## Known limits (noted by the user, not solved)
- No pagination and no size cap: each call reads every confirmed enrollment of the class, the guardians of those students and the users, in several queries, and returns the whole list. It is fine for a class of a few dozen families; revisit (cursor pagination as in announcements, a cap or a cache) if classes with many families appear. The frontend picker for expense participants would also need search or paging in that case.
