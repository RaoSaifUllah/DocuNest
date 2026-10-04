# Development chat prompt
Copy the following into a new development chat:

Implement DocuNest in:
F:RF TechWorkingOpenSourceProjectsDocuNest

Read README.md and document/01-Requirements-and-Review.md through
04-Verification-and-Standards.md first. The original portfolio SRS is saved as
document/00-Original-Portfolio-SRS.md. Refined requirements and my requested
.NET architecture take precedence over its alternative stack suggestions.

Build the complete self-hosted DocuNest v1 following the milestone plan:
- Latest supported stable .NET 10 LTS patch, ASP.NET Core API and separate worker.
- Clean modular Domain/Application/Infrastructure architecture.
- Aggregate EF Core repositories and Unit of Work for writes.
- Dapper permission-scoped parameterized SELECT projections and PostgreSQL.
- Secure JWT/rotating refresh tokens/replay detection/CSRF/MFA/session revocation.
- React/TypeScript/Vite/Tailwind CSS modern responsive accessible light/dark UI.

Implement accounts/workspaces/membership/document grants; streaming PDF/image
uploads with quotas, quarantine and malware scan; scoped SHA256 duplicate choice;
sandboxed durable PDF extraction/rendering and Tesseract OCR with configurable
language packs; metadata suggestions that preserve user edits; categories/tags;
permission-scoped text search; safe multipage preview and authorized signed
downloads; dashboard/timezone reminders; expiring/password-protected/revocable
share links and download limits; trash/restore/purge; manifest export and bounded
directory import. Implement storage/DB crash reconciliation rather than assuming
filesystem and database writes are one atomic transaction.

Keep original files immutable; process untrusted files with network/resource
isolation. Never send documents to cloud OCR/scanners/analytics by default.
Document server plaintext processing and sharing/download-control limitations.
Use .NET orchestration and local audited OCR/rendering tools, not a replacement
Python web backend. Review tool licenses before selection; pin versions/data.

Complete Docker Compose, migrations, consistent backup/restore, observability,
security hardening, CI and user/developer/API documentation. Follow documented
OWASP ASVS Level 2 and IEEE/ISO alignment targets without claiming certification.

Use meaningful unit, real PostgreSQL integration, storage fault, OCR fixture,
provider-contract and Playwright tests. Fixtures must be synthetic/sanitized.
Use local notification/scanner mocks; no real malware or private documents in
the repository, and no live notifications without my explicit authorization.

No native mobile app in this MVP; implement browser camera/PWA capability
fallbacks. Later native scope, if authorized, uses React Native/TypeScript/
NativeWind on Android/iOS only.

Maintain document/Implementation-Status.md with actual milestones, commands/
results, limitations and next steps. Record justified changes as ADRs.
Work only in DocuNest; do not change NotifyHub/TaskFlow or create other projects.
Ask for my Git URL before configuring a remote or pushing. Do not externally
publish/deploy without authorization. Proposed AGPL-3.0-only license requires
my selection before publication. Inspect the scaffold/tools, execute Phase 0,
and continue through the development plan.
