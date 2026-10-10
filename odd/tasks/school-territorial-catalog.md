# School territorial catalog

## Objective
Schools stop being free text. They come from the official Spanish school lists (three Excel files kept on the user's machine, never committed) and are stored in normalized tables: `Provinces` -> `Localities` -> `PostalCodes`, and `Schools` linked to a locality (required) and a postal code (optional). Families pick province, then locality, then search the school by name inside that locality, so the same school name in different towns can never be mixed up.

## Decisions (user, 2026-10)
- Data source: the three Excel files (`listado_centros_primaria.xls`, `_secundaria.xls`, `_bachillerato.xls`). They are only used to load the database once; the data then lives ONLY in PostgreSQL (no CSV or dataset in the repository). Another environment is filled by running the importer again with the Excel files on that machine.
- Importer: a new console project in the solution (explicit user permission given for `SumiClas.Tools.SchoolCatalogImporter` with the `ExcelDataReader` package), idempotent (key: center code), phase A2.
- Nothing repeated: unique indexes on province name, locality (province, normalized name), postal code (locality, code) and school official code.
- A postal code is unique per (locality, code), not globally: 615 codes appear under several localities in the real data.
- A school may carry an optional phone (max 20, trimmed, blank = null): stored and shown in results, not searchable, no format validation. The importer must fill it from the Excel `phone` column (~79% of rows have it).
- A school always has its locality; the postal code is optional (10 schools carry the placeholder `00000` = unknown; do not invent a code).
- Display name of a school = generic + specific denomination (composed by the importer); search uses a normalized name.
- Free registration of schools is removed (`POST /schools`, `SchoolRegister`): schools come only from the official catalog.
- `GET /schools` requires `localityId`; name is an optional filter.
- Once the import is done, whatever is no longer needed gets removed (user request).

Real figures from the analysis: 18,197 unique schools (8-digit center code), 52 provinces, 5,618 localities (no INE codes), 6,640 (locality, postal code) pairs, 6,913 codes present in more than one file with identical data, 33 schools without specific name, 3,114 without generic name, 266 groups with the same specific name in one locality.

## Tasks
- [x] A1 (done, not committed yet) Backend model: modules `Provinces`, `Localities`, `PostalCodes` (reference data, no write use cases), `TextNormalizer`, `School` changes (`Create`, `LocalityId`, `PostalCodeId?`, `NormalizedName`), removal of the free registration, `SchoolFinderByLocality`, `GET /schools` requires `localityId`, EF mappings and a new migration, tests, backend AGENTS.md.
- [x] A2 (done, not committed yet) Importer project (`SumiClas.Tools.SchoolCatalogImporter`): read the three Excel files, discard repeated header rows, de-duplicate by center code, normalize provinces/localities (unify spellings), skip `00000` postal codes, upsert idempotently, real import into the dev database, verify counts and re-run.
- [x] A3 (done, not committed yet) API: endpoints for provinces and localities (selectors), enriched `SchoolResponse` (province, locality and postal code names through the bus), regenerate the NSwag client.
- [ ] F Frontend: onboarding step 1 becomes province -> locality -> school search; remove the "register a new school" path.
- [ ] D Documentation and cleanup of what is no longer needed after the import.

## Acceptance
- The three reference tables and `Schools` hold the real data without duplicates (counts match the analysis) and re-running the importer changes nothing.
- A family can only find a school after choosing province and locality.
- `dotnet test` passes, typecheck/lint/build of the frontend pass, and each phase is verified on the real stack.

## Known transitional breakage
- After A1 the NSwag client and the onboarding wizard are NOT updated: the wizard still calls `GET /schools?name=` and `POST /schools`, which no longer exist as before, so the first onboarding step is broken at runtime until phase F. Accepted by the user.
- The database has no territorial data until A2 runs the importer.

## Progress
- Analysis of the Excel files done (read-only).
- A1 done: modules Provinces/Localities/PostalCodes, TextNormalizer, School.Create (LocalityId required, PostalCodeId optional, NormalizedName), SchoolFinderByLocality, GET /schools requires localityId, free registration removed, migration TerritorialCatalog, backend AGENTS.md. Route: delegated direct (fork). Evidence: dotnet build 0 errors; dotnet test 110 passed; real Postgres: each unique index (province name, locality per province, postal code per locality, school official code) rejected a duplicate, same postal code under two localities accepted, GET /schools without localityId -> 400, unknown locality -> 200 [], accent-insensitive search worked; test rows deleted, Users untouched.
- A1 extension (coordinator request): optional `Phone` on School; TerritorialCatalog migration regenerated (same name) and re-verified on the real DB (phone column present, 4 unique indexes reject duplicates, phone returned by GET /schools and /schools/{id}); 114 tests pass.
- A2 done: project apps/backend/tools/SumiClas.Tools.SchoolCatalogImporter (net10.0, references SumiClas.csproj, ExcelDataReader 3.9.0; System.Text.Encoding.CodePages needs no package on net10, only CodePagesEncodingProvider.Instance; added to SumiClas.slnx). Real run on the dev DB: 26393 school rows read, 18197 unique center codes (0 cross-file conflicts), inserted 52 provinces, 5618 localities, 6630 postal codes, 18197 schools; 10 schools without postal code (placeholder 00000, as planned), 3292 without phone (14905 with phone). Second run: 0 inserted, 0 updated, same counts. Postal pairs are 6630 and not the 6640 of the analysis because the 10 pairs with 00000 are not stored. No duplicates by any unique key, every school has a locality, no postal code belongs to another locality than its school. Spot checks OK (Abla / Almeria 04510 950359901; Menendez Pelayo, Lugo 27004; a school without phone). GET /schools?localityId=..&name=Menéndez and name=menendez%20PELAYO both return the school with phone; unknown text returns []. 114 tests pass, solution builds with 0 errors. Nothing was truncated or dropped: no phone over 20 chars (max 19), no address over 300.
- How to run: dotnet run --project apps/backend/tools/SumiClas.Tools.SchoolCatalogImporter -- [folderWithTheXls] [connectionString] (defaults: C:\Users\anusk\Desktop; connection from env SUMICLAS_DB or apps/backend/api/appsettings.Development.json). Run time about 6 s.
- Normalization: provinces/localities merged by TextNormalizer key, display name = most frequent spelling; school name = generic + specific (single-spaced); phone kept as written (some are like 924 024508/ 09); a school row with no usable territory or name would be skipped (0 happened).
- To delete afterwards (phase D): the whole apps/backend/tools folder, its entry in SumiClas.slnx, and the importer note in apps/backend/AGENTS.md. The Excel files stay on the Desktop, never in the repo.
- A3 done: GET /provinces (52, by name), GET /provinces/{provinceId}/localities?name= (accent/case-insensitive, no pagination, unknown province -> 200 []), SchoolResponse enriched (provinceId/provinceName, localityName, postalCode 5-digit or null) through IQueryBus finders (LocalityFinderByIds, ProvinceFinderByIds, PostalCodeFinderByIds) batched in SchoolResponseFactory; SchoolFinderByLocality cap of 50 now in the query (Specification.AddTake + order by normalized name). Evidence: dotnet build 0 errors, dotnet test 132 passed; real DB through a temp API on 5099: 52 provinces, 107 localities in Almeria (name=abla -> Abla, name=N%C3%8DJAR and nijar -> Níjar and San Isidro de Níjar), GET /schools?localityId=<Abla> and GET /schools/{id} return Almería / Abla / 04510 / phone 950359901, a 50-school listing and a name-filtered one cost 4 SQL commands each (log of Executed DbCommand), no extra per school. NSwag client regenerated with pnpm generate:client (only GeneratedApiClient.ts changed, CRLF kept, lint OK). The client no longer has schoolRegister/schoolFinderByName: pnpm typecheck fails only in src/modules/schools/infrastructure/NSwagSchoolRepository.ts (schoolFinderByName, schoolRegister, implicit any), to be fixed in phase F.
- Not done in A1: the migration adds Schools.LocalityId with an empty-Guid default, so it is only safe with an empty Schools table (true in dev).

## Next step
F (frontend onboarding wizard: province -> locality -> school search; fix NSwagSchoolRepository). Commit A3 in reviewed pieces first.
