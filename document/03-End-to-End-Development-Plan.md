# End-to-end development plan
Version 1.0 | 2026-10-01 | Execute sequentially, verify each exit gate.

## 0. Baseline
Read design/requirements. Verify stable .NET/Node/container/tool versions; pin
compatible packages/language data with lockfiles/checksums. Review renderer/OCR/
viewer licensing before selecting packages. Generate solution/manifests,
editorconfig/example config and contributor/security/changelog templates.
Exit: clean checkout setup/build passes; no secrets; license decision tracked.

## 1. Architecture and persistence
Create five clean-layer projects and module interfaces. PostgreSQL EF schema/
migrations/repositories/UoW; scoped Dapper reads/restricted roles; concurrency,
Problem Details/validation, OpenAPI, logs/health and frontend shell.
Exit: empty DB migration/transaction rollback/composite tenant constraints/
Dapper no-write and secret exclusion tests pass. DN-14 foundation.

## 2. Identity and workspace boundaries
Implement bootstrap/registration/invites/reset/verification/MFA/JWT/refresh/
CSRF/sessions. Workspaces/roles/document grants and last-owner protections.
Build auth/recovery/workspace/member/session/settings screens.
Exit: reuse/race/revocation/CSRF tests, cross-workspace/document policy matrix
and nonenumerating responses verified. DN-01/02.

## 3. Streaming uploads and storage
Implement private storage/quarantine, MIME/signature/size/quota checks, streaming
SHA256, duplicate choice scoped to visible docs, API upload, scanner contract,
staging/reservation finalization/reconciler. Frontend drag/file/camera/paste queue.
Exit: malformed/oversize/quota/duplicate/filename cases; API or DB failure around
staging/finalization; concurrent quota tests; unauthorized blobs inaccessible.
DN-03/04. PWA share target provides feature detection and fallback.

## 4. Safe processing and editable metadata
Implement durable step jobs/leases/fencing and sandboxed renderer/Tesseract.
Embedded-text/mixed/scanned PDF and image/TIFF fixtures; thumbnail/OCR output,
language pack configuration, generation publication, editable suggestions.
Exit: page/pixel/resource/time limits enforce; restart/retry/stale generation
tests pass; quarantine cannot preview; user edits survive reprocess; OCR quality
reported against sanitized fixtures with no fabricated accuracy claims.
DN-05/06.

## 5. Organization, search and viewer
Categories/tag CRUD/merge, metadata concurrency, permission-scoped PostgreSQL
search/filter/count/snippet projections, safe preview viewer and signed downloads.
Build list/detail/OCR/suggestion/category/tag UI with mobile/keyboard behavior.
Exit: inaccessible document content absent from search/snippets/counts and
download; expired/revoked tickets denied; multipage viewer and Range tested.
DN-07/08/09.

## 6. Dashboard, reminders, sharing and trash
Dashboard metrics/activity, date/timezone reminders/preferences/durable email
and in-app jobs, share token/password/revoke/download limits, trash/restore/purge.
Build corresponding pages with clear destructive-action confirmations.
Exit: DST/reminder generation/access revocation, concurrent share-limit/Range,
trash invalidation/restore and failed blob cleanup tests pass. DN-10/11/12.
Use local SMTP mocks; no live notification without authorization.

## 7. Portability
Streaming individual/selected/workspace manifest exports; directory/manifest
imports, schema/id mapping, limits and quarantine processing per item.
Exit: export checksums and round-trip metadata/tags/categories verified; revoked
permissions cancel artifacts; traversal/symlink/zip-bomb fixtures rejected;
partial failure reports and cleanup tested. DN-13.

## 8. Operations and release readiness
Production Compose/images/TLS examples/limits, explicit migrations, consistent
backup/restore tools, metrics/alerts, shutdown, retention and performance.
Complete install/OCR languages/API/backup/upgrade/troubleshooting/user/dev docs.
CI: formatting/lint/typecheck/unit/integration/E2E/build/security/container scans.
Generate SBOM/tool-license attribution; review scanner findings.
Exit: clean install/upgrade and isolated restore prove DB/file/key consistency
and OCR/search/pending-job recovery; no unintended live messages/share exposure.
Reference load measured; manual accessibility/browser review; all DN IDs linked
to actual evidence. Resolve final license/Git URL before publication. DN-14.

## Test priorities
Unit: grants, suggestions, reminder dates, state transitions, retry and token policy.
Real PostgreSQL integration: tenancy, Dapper projections, quota races, metadata
versions, transactional job insertion, leases/fencing and refresh races.
Storage fault integration: staging/final promotion/purge interruptions and reconciliation.
OCR contract: known sanitized native/scanned/mixed/multipage and language fixtures,
timeouts/huge-image bounds with synthetic safe inputs, quarantined cases.
E2E: auth/workspace -> upload -> OCR -> edit/tag -> search/view/download ->
reminder -> trash/restore; read-only denial; duplicate retain/cancel; share revoke;
export/import round trip; mobile camera fallback.
Provider/scanner mocks before dedicated sandbox smoke checks. No real malware or
sensitive IDs in fixtures. Browser tests use synthetic documents.

## Progress and done
Maintain document/Implementation-Status.md with milestone, changes, commands/
results, DN evidence, limitations and next actions. Required feature gates:
validation/permissions/error behavior, desktop/mobile accessibility, meaningful
tests, updated API/docs and passing CI. Unexecuted checks remain unverified.
No cross-project runtime dependency or edits to other portfolio folders.
