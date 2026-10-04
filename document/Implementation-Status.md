# Implementation status

Started 2026-10-04 (Asia/Karachi). This file records executed checks; planned or unexecuted gates are not passes.

## Milestones

| Phase | State | Evidence / next gate |
|---|---|---|
| 0 Baseline | Completed | Supported LTS verified; exact manifests/lockfiles, license review/ADRs/templates; backend and frontend clean builds pass. |
| 1 Architecture/persistence | Completed | Backend builds, 25 policy tests, 7 real PostgreSQL tests, frontend 7 unit + 4 browser checks pass. DB roles replaced by owner-selected application scoping/read-only transactions (ADR 0002). |
| 2 Identity/workspaces | In progress | Auth lifecycle, API boundaries and corresponding UI under implementation. |
| 3 Upload/storage | Pending | Requires Phase 2 gate. |
| 4 Processing/metadata | Pending | Requires Phase 3 gate. |
| 5 Search/viewer/organization | Pending | Requires Phase 4 gate. |
| 6 Dashboard/reminders/shares/trash | Pending | Requires Phase 5 gate. |
| 7 Portability | Pending | Requires Phase 6 gate. |
| 8 Operations/readiness | Pending | Requires all feature gates and operational evidence. |

## Actual environment / commands

- Read README.md, document/01–05 completely. Scaffold has no code or Git repository.
- `dotnet --info`: SDK 10.0.401, runtime/ASP.NET 10.0.12, Windows x64.
- `node --version`: v24.11.0; `npm --version`: 11.6.1.
- `git status --short`: no Git repository; no initialization/remote/push performed.
- Tool detection: Docker, Tesseract, ClamAV and PostgreSQL CLI absent on PATH. PostgreSQL installation directories have no server binaries. Bundled Poppler is available but not selected as an unsandboxed production processor.
- NuGet official registry queries: EF Core/Design/API/JWT 10.0.12; Npgsql/provider 10.0.3; Dapper 2.1.89; Argon2 implementation 1.3.1; xunit 2.9.3; runner 4.0.0; test SDK 18.10.1. Compatible restore/build still required.
- User supplied development PostgreSQL connection on 2026-10-04; use synthetic data in isolated schemas. Secret stored in ignored local configuration only.

## Unresolved release risks and next steps

- Feature milestones 2–8 and their integration/security/processing/browser gates remain pending.
- Docker and native OCR/scanner isolation smoke tests require a usable container runtime; do not mark these passed on this host.
- No live notifications or external deployment authorized. Repository configured only inside DocuNest following owner authorization.
- Owner selected AGPL-3.0-only and authorized progress pushes to https://github.com/RaoSaifUllah/DocuNest on 2026-10-04. External deployment remains unauthorized.
- Hostname/TLS, operator production key storage/encrypted volumes and measured restore/load/accessibility gates remain pending.

## Phase 0/1 evidence, 2026-10-04

- `dotnet restore DocuNest.slnx --configfile NuGet.Config`: passed. Per-project dependency lockfiles generated.
- `dotnet build DocuNest.slnx --no-restore --nologo`: passed, zero warnings/errors after explicitly unifying EF Core Relational 10.0.12.
- `dotnet test tests/DocuNest.UnitTests/DocuNest.UnitTests.csproj --no-restore --nologo`: passed 25/25, no skips. Covers permission matrix, tenant/grant denial, clean/current blob checks, last owner, confirmed metadata preservation, quota/state/lease policies, DST gap/overlap, token replay/expiry/stamp, import traversal/symlink/compression bounds and share budgets. These pure checks do not substitute for later integration gates.
- `dotnet list DocuNest.slnx package --vulnerable --include-transitive --format json`: zero published vulnerability records; report retained in ignored .runtime. This is dependency feed evidence, not proof of secure code.
- Frontend `npm install --ignore-scripts`: passed, 302 packages, audit found 0 vulnerabilities. Pins React 19.3.0, Vite 8.3.2, Tailwind 4.3.3, TypeScript 6.0.3. Chose Vitest 4.1.11 because 5.0.3 declarations conflict with jest-dom; kept declaration checking enabled. jsdom 29.1.1 matches installed Node 24.11.
- Frontend `npm run build`, `npm run typecheck`, `npm run lint`: passed.
- Frontend `npm test`: passed 7/7.
- Frontend `npm run test:e2e`: passed 4/4 Chromium desktop and Pixel 7 shell journeys. Desktop/mobile screenshots reviewed. Covers keyboard/navigation/theme/responsiveness/API safety; full feature journeys remain pending.
- Local API `/health/ready`: HTTP 200 against supplied development DB; `/api/v1/capabilities`: HTTP 200, expected format allowlist. Host stopped after checks.
- Initial PostgreSQL integration run exposed ignored schema startup state through Neon pooling. Initial synthetic tests/migration touched public. Development endpoint now direct; explicit schema selection + assertion fail closed. Isolated integration fixture checks schema before migration/truncation. Public authored-object inventory/cleanup and final test results are still pending.
- Final `dotnet test tests/DocuNest.IntegrationTests/DocuNest.IntegrationTests.csproj --logger 'console;verbosity=normal'`: passed 7/7 against real Neon PostgreSQL direct endpoint, no skips, 2.0525 minutes. Generated schema verified before migration and removed after tests. Tests prove empty migration down/up/views; transactional document/job rollback; composite blob/category/tag FKs; Dapper read-only write denial and secret-excluded columns; scoped search/details/count/snippet/dashboard; optimistic lost-update rejection; scoped cursor and allowlisted sort.
- Development schema migration completed after explicit schema assertion (`docunest_dev`). Automatic approval review rejected removal of earlier public synthetic objects because it would delete remote tables; read-only inventory and explicit owner approval are required before cleanup. No retry/workaround deletion attempted.
