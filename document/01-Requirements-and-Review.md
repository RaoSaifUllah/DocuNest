# Requirements and source review
Version 1.0 | 2026-10-01 | Proposed implementation baseline

## Scope and reconciliation
Source: sections 3-9 plus common requirements of the supplied portfolio SRS.
User-requested .NET stack replaces the suggested FastAPI/Celery application stack.
Use a .NET worker orchestrating local sandboxed OCR/decoder tools.
Core MVP: auth/workspaces, PDF/images, OCR, metadata, categories/tags, search,
reminders, viewer/download, trash/restore and Docker.
Complete v1 additionally implements source share links, JSON-manifest export,
directory bulk import, duplicate choice and dashboard. Cloud import adapters,
Office formats, LDAP/OIDC, AI classification and external search engines are later.
Formats: PDF/JPEG/PNG/WebP/TIFF required; HEIC only when an audited decoder is
available, otherwise explicit unsupported response and client conversion guidance.
No accuracy guarantee for OCR or metadata; suggestions never overwrite confirmed edits.
Encryption at rest is a deployment requirement for production volumes/backups;
application blob encryption is optional later and must include all derived data.
Server OCR/search sees plaintext: this is not a zero-knowledge vault.

## Permissions
System admin manages instance/users/registration/storage/OCR/email/backup policy
without automatic workspace content access. Owner manages members, all workspace
documents/settings. Member uploads and edits permitted documents; read-only
member views/searches/downloads permitted documents only.
Default document visibility is workspace-wide, with explicit restricted-document
mode for owner/uploader and designated members. Owner always retains workspace
document management access. Member deletion requires explicit document edit grant;
external sharing/export requires owner or explicit share/export grant.
Read-only may create personal reminders for visible documents but cannot change
document metadata. No last-owner removal. All queries/jobs enforce workspace
and document permissions; revoked access suppresses queued notifications.

## Requirements traceability
| ID | Requirement and acceptance | Source | Phase |
|---|---|---|---|
| DN-01 | Registration modes, auth/reset/MFA, sessions, JWT/refresh rotation | 5.1-2; user stack | 2 |
| DN-02 | Workspace isolation, membership and document grants | 4,5.3 | 2 |
| DN-03 | Stream valid uploads, enforce quotas/formats, camera/paste/PWA fallback | 5.4-5 | 3 |
| DN-04 | SHA256 duplicate warning with authorized existing link/retain/cancel | 5.17 | 3 |
| DN-05 | Durable sandboxed scan/render/extract/OCR/index pipeline with retry/status | 5.6-7 | 4 |
| DN-06 | Editable metadata and non-destructive suggestions with provenance | 5.8,10 | 4 |
| DN-07 | Categories and tag CRUD/merge | 5.11-12 | 5 |
| DN-08 | Permission-scoped text search and listed filters | 5.13-14 | 5 |
| DN-09 | Page viewer, zoom/rotate/fullscreen, metadata/OCR panels and downloads | 5.9 | 5 |
| DN-10 | Date-based personal reminders and dashboard expiration/storage/activity | 5.15-16 | 6 |
| DN-11 | Expiring/revocable/password shares with download limits | 5.18 | 6 |
| DN-12 | Trash/restore, retention and retryable permanent blob cleanup | 5.19 | 6 |
| DN-13 | Individual/selected/workspace export and bounded directory import | 5.20-21 | 7 |
| DN-14 | Compose, migrations, backups/restore, observability, docs/security CI | 8-9/common | 8 |

## Measurable gates
Reference Linux 4 vCPU/8 GiB, SSD, 20 users and 100,000 documents with recorded
OCR text volume. CRUD p95 <500ms, search p95 <1s, dashboard usable <2s on recorded
broadband profile. Benchmark OCR separately on named page/language fixtures;
do not include OCR completion in upload-response latency or invent OCR SLA.
Initial limits: 50 MiB/file, 200 pages, 40 megapixels/image, workspace 10 GiB;
configure lower/higher limits with operator guidance and bounded worker memory.
Lighthouse performance >=85/accessibility >=90 plus manual WCAG 2.2 AA checks.
Proposed RPO 24h/RTO 2h; isolated restore drill must prove both.
Store instants UTC, issue/expiration dates as date-only, reminder IANA timezone.
UTF-8, English initial UI, translation-ready/RTL-ready; opt-in telemetry only.
No private document upload to cloud scanners, OCR or analytics.

## Pending inputs
Git URL, final license, hostname/TLS, production secret/key store, SMTP, OCR
language selection and storage budget. Use mocks/local fixtures during development.
Native mobile is deferred; browser camera is the v1 approach.
