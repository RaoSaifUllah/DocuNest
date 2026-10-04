# Software design
Version 1.0 | 2026-10-01

## Views, stakeholders and technology
Users need private retrieval; operators need bounded file processing/recovery;
contributors need clear module boundaries; reviewers need permission evidence.
Views: dependency/runtime, persistence, upload/processing, security, UI, operations.
.NET 10 LTS ASP.NET Core/EF Core with compatible Npgsql, Dapper and PostgreSQL.
React/TypeScript/Vite/Tailwind CSS, accessible components and TanStack Query.
Local Tesseract with pinned language data; audited PDF/image renderer selected
with license/security review before implementation. No mandatory Python server.
PaddleOCR and external search engines deferred behind interfaces.

Browser -> HTTPS same-origin proxy -> static Frontend + /api/v1 API.
API -> local private storage and PostgreSQL; Worker -> durable jobs -> scanner
and bounded network-isolated rendering/OCR sandbox -> derived blobs/search data.
Malware scanner required by secure default; unavailable scanner keeps quarantine.
Separate API/worker deployments, PostgreSQL durable queue, no mandatory broker.

## Clean modular architecture
Domain: entities/value objects/state/permission rules, no HTTP/EF.
Application: use cases/validation/policies, aggregate repositories/UoW, read,
storage, OCR, malware, notification, encryption and clock interfaces.
Infrastructure: EF mappings/migrations/repos/UoW, scoped Dapper reads, private
filesystem adapter, jobs, sandbox adapters, Identity and SMTP.
Api/Worker: HTTP/background hosts and composition roots.
Modules: Identity, Workspaces, Documents, Processing, Organization, Search,
Reminders, Sharing, Portability, Operations.
Dependency direction Domain <- Application <- Infrastructure, hosts compose.
No public unrestricted IQueryable or repositories committing independently.

## EF / Dapper
One scoped DbContext/IUnitOfWork per command. Repositories load/mutate aggregates;
application owns transaction/commit. Metadata+activity+jobs commit together.
External file/SMTP/OCR work outside transaction. Optimistic versions protect
metadata edits (If-Match required; 428 missing, 409 conflict).
Dapper SELECT projections only, parameterized scope/visibility predicates,
allowlisted sorting and cursor pages <=100. Separate SELECT-granted role on
reviewed tables/views excluding auth/share secrets; write/migration roles separate.
EF filters cannot protect Dapper. Token/authorization/job state use authoritative
primary write context. Tests prove denied rows also absent from counts/snippets.

## Data model
UUID IDs, UTC instants, composite workspace FKs, tenant indexes and versions.
User/Workspace/Membership/Invite/RefreshSession/Token follow secure hashed
credential lifecycles. Last workspace owner cannot be removed.
Document: workspace/uploader, title/description/vendor, amount decimal/currency,
issue/document/expiration date-only fields, category, custom metadata JSON,
visibility/version/trash timestamps, processing generation/status.
DocumentGrant: authorized user/edit/share/export capability.
Blob: workspace/document, opaque key, original display name, verified MIME,
size/SHA256/scan status. Immutable original; derivative type/generation tracked.
OCRPage: document/page/generation, raw/normalized text, language, confidence.
ExtractionSuggestion: field/value/confidence/source generation and accepted flag.
Category/Tag/DocumentTag: workspace foreign keys, unique normalized names.
Reminder: document/user/zone/local date-time or relative offset, generation/status.
ShareLink: random token hash, expiry/password hash/revoke/version/download budget.
ExportJob/ImportJob: requester, scope, manifest version, state/artifact expiry.
DurableJob/Attempt: document/generation/step/lease/fencing/retry/due time.
AuditEvent: actor/action/resource/time/correlation; no document/OCR bodies.
Search projection: permitted title/filename/vendor/notes/tags/categories/text;
GIN text index plus selected trigram fallback. Language-aware config with simple
fallback for unsupported stemming; disclose OCR/search language limitations.

## Upload and crash consistency
Authorize before streaming. Enforce request/file/count/byte quota and rate limits.
Stream to random quarantine staging key, SHA256 while reading, never user paths.
Verify extension/MIME/magic and decoding constraints. Reject encrypted PDFs in v1
with clear reason; no password retained. Scanner checks before preview/download.
Reserve quota transactionally to prevent concurrent quota bypass.
On finalized staging blob, commit Document/Blob/job references and reservation.
If DB commit fails, staged blob becomes orphan collected after grace period.
Worker promotes immutable clean original to final private key with idempotent
copy/rename and records completion; reconciler resumes after crash. Preserve
source until DB confirms target; no false assumption DB/filesystem commit atomically.
Unfinished uploads expire reservations and staging; storage failure returns error.
Duplicate lookup within workspace AND visible documents only. Never reveal
existence/name of inaccessible matching file. Retain both creates separate
documents; no cross-tenant dedup physical storage in v1.
Pipeline: Quarantined -> Uploaded -> Queued -> Processing -> Completed/Failed;
public statuses source-compatible plus explicit quarantine/scan failure reason.
Scan failure/quarantine cannot be downloaded or exported.

## Processing and OCR
Durable steps scan, promote, inspect, render thumbnails, extract embedded PDF
text per page, OCR scanned/empty-text pages, suggestions, search projection.
Process native-text/mixed PDFs per page; OCR multi-page TIFF/images after bounded
decode. Do not execute PDF JavaScript, attachments, network fetches or active content.
Sandbox decoder/OCR non-root, read-only base, isolated network, ephemeral temp,
hard CPU/memory/page/pixel/output/time limits; kill process tree on timeout.
Use argument arrays and generated paths, never shell interpolation/user flags.
Pin executable versions/language data hashes and review licenses (including
transitive renderer components); enforce package/CVE review and attribution.
English default language, selectable installed packs; Urdu optional quality-
tested fixture set. Admin allowlist language IDs, no arbitrary filesystem paths.
Store page text/confidence when supported; bound extracted text size.
Heuristic title/vendor/date/amount suggestions, no external AI/cloud by default.
Preserve user-confirmed fields on retry/reprocess; generation fences reject stale
outputs. Thumbnail/search publication atomic per finished generation.
Claim jobs through EF PostgreSQL FOR UPDATE SKIP LOCKED; expiring lease/heartbeat/
fencing; retry transient failures with bounded backoff, terminal dead letter for
invalid input. Cleanup failed temporary outputs. UI offers authorized reprocess.
Cancellation/trash invalidates active generation and prevents later publication.

## Auth and security
Identity lifecycle with reviewed Argon2id password hasher/salt/rehash policy.
JWT 5-minute asymmetric signing with issuer/audience/algorithm/exp/nbf/kid checks.
Browser JWT memory only; opaque 256-bit refresh cookie HttpOnly/Secure/
SameSite=Strict/path /api/v1/auth/no Domain; persist hash; 7d idle/30d absolute.
Atomic rotate; reuse revokes family; serialize tabs; lost response relogin.
CSRF+Origin on login/refresh/logout and cookie endpoints; exact CORS.
Check active session/security stamp and current grants for every protected action;
revoke/password reset immediate; fail closed when authorization unavailable.
Invite-only default, no default admin password, single-use hashed reset/invites,
rate limits/nonenumerating responses, owner/admin MFA and sensitive-action step-up.
Strict CSP, text-only OCR/metadata rendering, nosniff, secure headers/redacted
logs, restricted audit. Original PDFs downloaded as attachment; browser preview
uses safe sanitized/raster derivatives and isolated viewer.
Storage outside web root, no directory listing or exposed internal paths.
Signed download tickets bind subject/session/workspace/document/blob/action and
<=60s expiry; endpoint rechecks current permissions/trash/scan/revocation, never
unrevocable static filesystem URL. Support bounded Range reads for viewer.
No zero-knowledge claim: server processes/searches plaintext. Production encrypted
volumes protect DB/originals/derivatives/temp/swap/backups; TLS in transit.
Application envelope file encryption optional later, not improvised in v1.
Deployment secret/config encryption uses reviewed AES-256-GCM unique nonce,
resource AAD/key versions; key backup/rotation tested.
Malware samples mocked in automated tests unless dedicated isolated scanner
test explicitly authorized; never place real malware in repository.

## Sharing
Owner or explicit share grant issues random 256-bit expiring token once, stores
hash, default 24h/max 7d, optional Argon2id password, revoke and max downloads.
Share landing exchanges URL-fragment token by POST for short-lived scoped share
session; scrub history/referrer, no third-party scripts or analytics.
Rate limit token/password attempts; do not reveal document before validation.
Read-only derived preview allowed; download disabled is a UX/policy restriction,
cannot prevent screenshots or capture of content already rendered.
Count downloads at authorized transfer-session start atomically; Range requests
within same short-lived session do not increment again. Aborted start still
consumes slot; parallel starts cannot exceed limit. Revocation stops new requests,
already-sent bytes cannot be recalled. No direct origin/bucket URLs bypass counters.

## Reminders, trash, import/export
Expiry/warranty/renewal/payment/custom reminders use explicit recipient zone/time
(default 09:00 displayed). Recompute on date edit with generation; execute only
with current access/nontrashed doc; gap moves to next valid instant, overlap first.
In-app/email v1 with opt-in preferences; one event/generation uniqueness guard,
bounded retries, missed reminders <=24h grace; external duplicates possible.
Trash default 30d: hide search, revoke shares, cancel reminders. Restore requeues
required indexing, keeps revoked shares revoked. Purge DB tombstone then leased
blob cleanup job; retry cleanup idempotently; reflect pending purge storage.
Export scope authorized at create AND execution/download; permission change
cancels/rebuilds export; short-lived private artifact, no sharing secrets.
ZIP originals plus versioned JSON manifest with checksums/metadata/tags/categories;
no auth tokens, internal paths or unchecked filenames; bounded streaming.
Bulk directory uploads preserve logical relative folders, never physical paths.
Manifest/archive imports enforce byte/file/depth/compression limits, reject
traversal/absolute paths/symlinks/zip bombs; each file follows quarantine pipeline.
Database/file id remapping, schema validation, resumable item status and dedupe.
Full owner export excludes trash by default; explicit inclusion documented.

## API, UI and operations
/api/v1 auth/session/MFA; workspace/member/grants; documents/upload/metadata/
preview/download/reprocess/trash/restore; tags/merge/categories; search;
reminders/dashboard; shares/revoke; imports/exports/status/download.
Scoped workspace paths; API upload via JWT in v1, scoped service keys later.
RFC 9457 error code/correlationId; 202 for durable jobs, 400/401/403/scoped404/
409/413/428/429 Retry-After/503. Versioned generated OpenAPI.
UI: auth, workspace switcher, dashboard, drag/file/camera/paste upload queue,
duplicate choice, processing progress, searchable list, viewer and metadata/OCR,
suggestion review, tags/categories, reminders, shares, trash, import/export,
members/settings/sessions/admin health. Accessible mobile camera fallback,
light/dark tokens, keyboard/focus, reduced motion and externalized RTL-ready text.
No original/derived document caching by service worker; offline shell only,
clear scope-specific caches on logout/switch; never imply offline vault access.

Compose: proxy/web/API/worker/PostgreSQL/scanner and isolated processing runtime.
Non-root images, internal DB/storage networks, trusted forwarded-header config,
volumes/resource limits, explicit migration job/compatibility guard.
Backup consistent DB/storage snapshot or coordinated ingestion pause, config and
protected keys; restore verifies original/derived hashes and OCR/search/queue
recovery in isolated environment with email/shares disabled.
Monitor queue age, OCR duration/failures, storage/quota, scanner availability and
cleanup backlog; labels bounded/no document/user IDs; logs no bodies/filenames.
Graceful shutdown stops claims and fences/leases unfinished jobs.
ADRs: modular .NET monolith+worker; EF writes/Dapper reads; PostgreSQL jobs;
local privacy-first OCR; immutable originals+derived safe previews; responsive
web/no native MVP; filesystem reconciliation rather than fictitious atomic commit.
