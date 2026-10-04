# Verification and standards
Version 1.0 | Planning targets; no executable implementation verified.

| Reference | Purpose | Evidence required |
|---|---|---|
| ISO/IEC/IEEE 29148:2018 | Requirements traceability | DN IDs linked to code/tests |
| ISO/IEC/IEEE 42010:2022 | Architecture description | Stakeholders/views/ADRs reflect delivered system |
| OWASP ASVS 5.0 Level 2 | Applicable L1+L2 controls | Exact stable control IDs, test/review results and justified exclusions |
| OWASP File Upload guidance | Hostile file handling | Quarantine/scan/type/size/sandbox tests |
| RFC 7519/8725 | JWT validation | Algorithm/issuer/audience/key/expiry negative tests |
| RFC 9700 principles | Refresh rotation/replay | Atomic rotation/revoke race evidence |
| RFC 9457/OpenAPI 3.x | API consistency | Valid schema/contracts/error examples |
| WCAG 2.2 AA | Accessible web experience | Automated plus keyboard/screen-reader review |

Standards-informed planning is not certification or guaranteed security.
Formal ISO/IEEE conformance requires normative text and artifact review.
Custom login does not imply OAuth protocol compliance.

## Release checklist
- [ ] DN requirements and applicable ASVS controls mapped to verified evidence.
- [ ] Tenancy/document grants enforced in reads/search/snippets/jobs/files/exports.
- [ ] Refresh/CSRF/MFA/revoke/reset and owner protections verified.
- [ ] Quarantine scanner fail-closed; malformed/large/active files blocked or isolated.
- [ ] Process network/CPU/memory/page/pixel/time limits and safe argument handling verified.
- [ ] Immutable originals and safe derivatives; generation fencing preserves user edits.
- [ ] Staging/promote/DB failure reconciliation, quota races and orphan cleanup tested.
- [ ] Duplicate lookup never reveals unauthorized documents.
- [ ] Signed downloads check permissions and expiry; share limits/revoke/ranges race-tested.
- [ ] Logs/traces/telemetry contain no raw documents/OCR text/credentials.
- [ ] Reminders respect timezone and revoked permissions; trash cancels shares/jobs.
- [ ] Import traversal/symlink/decompression limits and export permissions tested.
- [ ] Encrypted production volumes/backups and key recovery documented and exercised.
- [ ] Backup restore verifies DB/blobs/keys/hash/search/queue without live notifications.
- [ ] Load/OCR quality reports identify fixtures/hardware/limits and actual results.
- [ ] Keyboard/screen-reader/mobile/themes and browser/PWA capability fallbacks reviewed.
- [ ] Dependency/SAST/secret/container scans, SBOM and tool-license attribution complete.
- [ ] Fresh install/upgrade docs and final owner-selected license ready.

Evidence fields: DN/control ID, code location, command/review, environment/date,
result, reviewer and limitations. Never mark an unexecuted check as passed.

## Primary references checked 2026-10-01
- .NET policy: https://dotnet.microsoft.com/en-us/platform/support/policy
- Requirements: https://www.iso.org/standard/72089.html
- Architecture: https://www.iso.org/standard/74393.html
- ASVS: https://github.com/OWASP/ASVS/releases
- Upload: https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html
- OCR: https://tesseract-ocr.github.io/tessdoc/Installation.html
- OCR arguments: https://tesseract-ocr.github.io/tessdoc/Command-Line-Usage.html
- JWT: https://www.rfc-editor.org/rfc/rfc7519
- JWT security: https://www.rfc-editor.org/rfc/rfc8725
- Refresh security: https://www.rfc-editor.org/rfc/rfc9700.html
- Problem Details: https://www.rfc-editor.org/rfc/rfc9457
- Accessibility: https://www.w3.org/TR/WCAG22/

Recheck supported stable versions/licenses at implementation; .NET 10 LTS baseline.
